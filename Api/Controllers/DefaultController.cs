using System.Security.Claims;
using Api.Facades;
using Application.Dtos.Input;
using Application.Dtos.Output;
using Application.Interfaces;
using Application.utils;
using Domain.Models;
using Infra.ExternalApi.Interfaces;
using Infra.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Api.Controllers;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class DefaultController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBovespa _bovespa;
    private readonly IStockService _stockService;
    private readonly ISectorService _sectorService;
    private readonly IWalletService _walletService;
    private readonly ITransactionHistoryService _transactionService;
    private readonly IPositionService _positionService;
    private readonly IPositionHistoryService _positionHistoryService;
    private readonly IUserService _userService;
    private readonly IBacen _bacen;
    private readonly VariationFacade _variationFacade;


    public DefaultController(IBovespa bovespa, IStockService stockService, ISectorService sectorService,
        IUnitOfWork unitOfWork, IWalletService walletService, IPositionService positionService,
        ITransactionHistoryService transactionService, IPositionHistoryService positionHistoryService, IUserService userService, IBacen bacen, IStockHistoryService stockHistoryService)
    {
        _bovespa = bovespa;
        _stockService = stockService;
        _sectorService = sectorService;
        _unitOfWork = unitOfWork;
        _walletService = walletService;
        _transactionService = transactionService;
        _positionHistoryService = positionHistoryService;
        _userService = userService;
        _bacen = bacen;
        _positionService = positionService;
        _variationFacade = new VariationFacade(positionService, stockHistoryService);
    }

    [HttpPost("CreateWallet")]
    public async Task<ActionResult<Wallet>> CreateWaollet(string name)
    {
        try
        {
            var claim = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            if (claim == null)
                return Unauthorized();
            var user = await _userService.GetUserByEmailAsync(claim);
            if (user == null)
                return Unauthorized();
            var wallet = new Wallet
            {
                Name = name,
                User = user
            };
            await _walletService.CreateAsync(wallet);
            await _unitOfWork.SaveChangesAsync();
            return Ok(wallet);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(e.Message);
        }
    }
    
    [HttpPost("DeleteWallet")]
    public async Task<ActionResult> DeleteWallet(int id)
    {
        try
        {
            var claim = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            if (claim == null)
                return Unauthorized();
            var userId = await _userService.GetUserIdByEmailAsync(claim);
            if (userId == 0)
                return Unauthorized();
            var wallet = await _walletService.GetByIdOrDefaultAsync(id);
            if (wallet == null)
                return NotFound();
            if (wallet.User.Id != userId)
                return BadRequest("This wallet is not yours");
            _walletService.Delete(wallet);
            await _unitOfWork.SaveChangesAsync();
            return Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(e.Message);
        }
    }

    [HttpPost("BuyStock")]
    public async Task<ActionResult<Positions>> BuyStock(TransactionDto transactionDto)
    {
        try
        {
            if (transactionDto.StockSymbol == null)
                return BadRequest("Stock cannot be null");

            var claim = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            if (claim == null)
                return Unauthorized();
            var userId = await _userService.GetUserIdByEmailAsync(claim);
            if (userId == 0)
                return Unauthorized();
            await _stockService.UpdateAllStocksAsync();

            var wallet = await _walletService.GetByIdOrDefaultAsync(transactionDto.WalletId);
            if (wallet == null)
                return NotFound();
            if (wallet.User.Id != userId)
                return BadRequest("This wallet is not yours");

            transactionDto.Date = transactionDto.Date.Date;

            var stock = await _stockService.GetStockBySymbolOrDefaultAsync(transactionDto.StockSymbol);

            if (stock == null)
            {
                var (stockDto, message) = await _bovespa.GetStock(transactionDto.StockSymbol);

                if (stockDto == null)
                    return BadRequest(message);

                var sector = await _sectorService.GetOrCreateSectorAsync(stockDto.Sector);
                stock = await _stockService.CreateStockAsync(stockDto, sector);

                if (stock == null)
                    return BadRequest("Cannot create stock, try again");
            }

            var history = transactionDto.Adapt<TransactionHistory>();

            history.Stock = stock;
            history.EquityEffect = transactionDto.Amount * transactionDto.Price;

            await _transactionService.CreateAsync(history);

            // Get an existing position or create a new one
            var position = await _positionService.GetPositionByWalletAndStockOrCreateAsync(history, stock);

            position.Amount += transactionDto.Amount;
            position.TotalPrice += transactionDto.Amount * transactionDto.Price;

            if (position.Id != 0)
                _positionService.Put(position);
            else
                await _positionService.CreateAsync(position);

            await _positionHistoryService.UpdateOrCreatePositionHistory(history, position);
            await _positionHistoryService.UpdateAllPositionHistory(history, position);

            await _unitOfWork.SaveChangesAsync();

            return Ok(position);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(e.Message);
        }
    }

    [HttpPost("SellStock")]
    public async Task<ActionResult<Positions>> SellStock(TransactionDto transactionDto)
    {
        try
        {
            if (transactionDto.StockSymbol == null)
                return BadRequest("Stock cannot be null");

            var claim = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            if (claim == null)
                return Unauthorized();
            var userId = await _userService.GetUserIdByEmailAsync(claim);
            if (userId == 0)
                return Unauthorized();
            await _stockService.UpdateAllStocksAsync();

            var wallet = await _walletService.GetByIdOrDefaultAsync(transactionDto.WalletId);
            if (wallet == null)
                return NotFound();
            if (wallet.User.Id != userId)
                return BadRequest("This wallet is not yours");

            transactionDto.Date = transactionDto.Date.Date;

            var stock = await _stockService.GetStockBySymbolOrDefaultAsync(transactionDto.StockSymbol);
            if (stock == null)
                return BadRequest("Cannot find stock");

            transactionDto.Amount = transactionDto.Amount < 0 ? -transactionDto.Amount : transactionDto.Amount;

            var history = transactionDto.Adapt<TransactionHistory>();
            history.Stock = stock;
            history.Amount = -history.Amount;
            await _transactionService.CreateAsync(history);

            var position =
                await _positionService.GetPositionByWalletAndStockOrDefaultAsync(history.WalletId, history.StockId);

            if (position == null || position.Amount == 0)
                return BadRequest("You dont have position for this stock");

            history.EquityEffect = -(position.TotalPrice / position.Amount * transactionDto.Amount);
            position.TotalPrice -= position.TotalPrice / position.Amount * transactionDto.Amount;
            position.Amount -= transactionDto.Amount;

            if (position.Amount < 0 || position.TotalPrice < 0)
                return BadRequest("Invalid amount, ");

            _positionService.Put(position);

            await _positionHistoryService.UpdateOrCreatePositionHistory(history, position);
            await _positionHistoryService.UpdateAllPositionHistory(history, position);

            await _unitOfWork.SaveChangesAsync();

            return Ok(history);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(e.Message);
        }
    }

    [HttpGet("TransactionHistory")]
    public async Task<ActionResult<List<PositionHistory>>> GetTransactionHistory(int id)
    {
        try
        {
            var claim = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            if (claim == null)
                return Unauthorized();
            var userId = await _userService.GetUserIdByEmailAsync(claim);
            if (userId == 0)
                return Unauthorized();
            await _stockService.UpdateAllStocksAsync();

            var wallet = await _walletService.GetByIdOrDefaultAsync(id);
            if (wallet == null)
                return NotFound();
            if (wallet.User.Id != userId)
                return BadRequest("This wallet is not yours");

            return Ok(await _transactionService.GetAllByWalletIdAsync(wallet.Id));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(e.Message);
        }
    }

    [HttpDelete("DeleteTransaction")]
    public async Task<ActionResult> DeleteTransaction(int id)
    {
        try
        {
            var claim = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            if (claim == null)
                return Unauthorized();
            var userId = await _userService.GetUserIdByEmailAsync(claim);
            if (userId == 0)
                return Unauthorized();
            await _stockService.UpdateAllStocksAsync();

            var transaction = await _transactionService.GetByIdOrDefaultAsync(id);
            if (transaction == null)
                return NotFound();

            var wallet = await _walletService.GetByIdOrDefaultAsync(transaction.WalletId);
            if (wallet == null)
                return NotFound();
            if (wallet.User.Id != userId)
                return BadRequest("This wallet is not yours");


            var position =
                await _positionService.GetPositionByWalletAndStockOrDefaultAsync(transaction.WalletId,
                    transaction.StockId);
            if (position == null)
                return BadRequest("You dont have position for this stock.");

            position.Amount -= transaction.Amount;
            position.TotalPrice -= transaction.EquityEffect;

            transaction.EquityEffect = -transaction.EquityEffect;
            transaction.Amount = -transaction.Amount;

            await _positionHistoryService.UpdateOrCreatePositionHistory(transaction, position);
            await _positionHistoryService.UpdateAllPositionHistory(transaction, position);

            _transactionService.Delete(transaction);
            _positionService.Put(position);

            await _unitOfWork.SaveChangesAsync();
            return Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(e.Message);
        }
    }
    
    
    [HttpGet("MainMenu")]
    public async Task<ActionResult<MenuDto>> MainMenu()
    {
        try
        {
            var claim = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            if (claim == null)
                return Unauthorized();
            var user = await _userService.GetUserByEmailLoadedAsync(claim);
            if (user == null)
                return Unauthorized();

            var result = user.Wallets.Select(x => new MenuDto
            {
                Id = x.Id,
                Name = x.Name
            });
            return Ok(result);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(e.Message);
        }
    }
    
    [HttpGet("HomePage")]
    public async Task<ActionResult<UserDto>> HomePage()
    {
        try
        {
            var claim = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            if (claim == null)
                return Unauthorized();
            var user = await _userService.GetUserByEmailLoadedAsync(claim);
            if (user == null)
                return Unauthorized();
            await _stockService.UpdateAllStocksAsync();
            var userDto = new UserDto
            {
                Email = user.Email,
                Name = user.Name,
                Wallets = user.Wallets.Select(x => new WalletDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Positions = x.Positions
                }).ToList()
            };

            foreach (var wallet in userDto.Wallets)
            {
                foreach (var position in wallet.Positions)
                {
                    wallet.TotalValue += position.Amount * position.Stock.LastPrice;
                    wallet.TotalCost += position.TotalPrice;
                }

                wallet.TotalProfit = wallet.TotalValue - wallet.TotalCost;
                wallet.ProfitPctg = wallet.TotalCost == 0
                    ? 0
                    : decimal.Round(wallet.TotalProfit / wallet.TotalCost * 100, 2);
                userDto.TotalValue += wallet.TotalValue;
                userDto.TotalCost += wallet.TotalCost;
                wallet.Positions = new List<Positions>();
            }

            userDto.TotalProfit = userDto.TotalValue - userDto.TotalCost;
            userDto.ProfitPctg = userDto.TotalCost == 0
                ? 0
                : decimal.Round(userDto.TotalProfit / userDto.TotalCost * 100, 2);

            return Ok(userDto);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(e.Message);
        }
    }
    
    [HttpGet("Chart/Wallet/Cdi")]
    public async Task<ActionResult<ChartCdiDto>> ChartWalletCdi(int walletId, DateTime? date, GraphType graphType,
        Periodicity periodicity)
    {
        try
        {
            if (date >= DateTime.Now)
                return BadRequest("Date must be in past");

            var claim = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            if (claim == null)
                return Unauthorized();
            var userId = await _userService.GetUserIdByEmailAsync(claim);
            if (userId == 0)
                return Unauthorized();

            var wallet = await _walletService.GetByIdOrDefaultAsync(walletId);
            if (wallet == null)
                return NotFound();
            if (wallet.User.Id != userId)
                return BadRequest("This wallet is not yours");

            var totalAmountList = await _transactionService.GetTotalAmountByDateAsync(walletId);

            List<(DateTime date, decimal interest)>? interestsSinceDate = null;

            var minDate = date ?? totalAmountList.MinBy(x => x.Date)?.Date;
            if (minDate == null)
                return StatusCode(StatusCodes.Status500InternalServerError, "Min date is null");

            for (int i = 0; i < 10; i++)
            {
                interestsSinceDate = await _bacen.GetInterestsSinceDateAsync(minDate.Value);
                if (interestsSinceDate != null)
                    break;

                await Task.Delay(1000); // Add a delay of 1 second between each call
            }

            if (interestsSinceDate == null || interestsSinceDate.Count == 0)
                return BadRequest("Bacen service unavailable. Try again later");


            Dictionary<string, decimal> cdi;
            SortedDictionary<string, decimal> variation;

            switch (graphType)
            {
                case GraphType.Absolute:
                    cdi = CdiFacade.CdiAbsolute(periodicity, interestsSinceDate, totalAmountList);
                    variation = await _variationFacade.VariationAbsolute(date, periodicity, wallet.Positions);
                    break;
                case GraphType.AbsoluteAccumulated:
                    cdi = CdiFacade.CdiAbsoluteAccumulated(periodicity, interestsSinceDate, totalAmountList);
                    variation = await _variationFacade.VariationAbsoluteAccumulated(date, periodicity, wallet.Positions);
                    break;
                case GraphType.Percentage:
                    cdi = CdiFacade.CdiPercentage(periodicity, interestsSinceDate, totalAmountList);
                    variation = await _variationFacade.VariationPercentage(date, periodicity, wallet.Positions);
                    break;
                case GraphType.PercentageAccumulated:
                default:
                    cdi = CdiFacade.CdiPercentageAccumulated(periodicity, interestsSinceDate, totalAmountList);
                    variation = await _variationFacade.VariationPercentageAccumulated(date, periodicity, wallet.Positions);
                    break;
            }

            if (variation.Count < cdi.Count)
            {
                foreach (var cv in cdi.Where(cv => !variation.ContainsKey(cv.Key)))
                {
                    cdi.Remove(cv.Key);
                }
            }
            if (variation.Count > cdi.Count)
            {
                variation.Remove(variation.Last().Key);
            }

            var response = new ChartCdiDto
            {
                Dates = variation.Select(x => x.Key.ToString()).ToList(),
                Variation = variation.Select(x => x.Value).ToList(),
                Cdi = cdi.Select(x => x.Value).ToList()
            };
            return Ok(response);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(e.Message);
        }
    }
    
    [HttpGet("Sectors")]
    public async Task<ActionResult<TotalDto>> Sectors(int id)
    {
        try
        {
            var claim = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            if (claim == null)
                return Unauthorized();
            var userid = await _userService.GetUserIdByEmailAsync(claim);
            if (userid == 0)
                return Unauthorized();
            await _stockService.UpdateAllStocksAsync();

            var wallet = await _walletService.GetByIdOrDefaultAsync(id);
            if (wallet == null)
                return NotFound();
            if (wallet.User.Id != userid)
            {
                return BadRequest("This wallet is not yours");
            }

            decimal totalValue = 0, totalCost = 0;

            var result = new TotalDto();
            foreach (var position in wallet.Positions)
            {
                totalCost += position.TotalPrice;
                totalValue += position.Amount * position.Stock.LastPrice;
                var positionDto = new PositionDto
                {
                    Amount = position.Amount,
                    Stock = position.Stock,
                    StockId = position.Stock.Id,
                    TotalPrice = position.TotalPrice,
                    WalletId = wallet.Id,
                    TotalValue = position.Amount * position.Stock.LastPrice,
                    Profit = position.Amount * position.Stock.LastPrice - position.TotalPrice,
                    ProfitPctg =
                        decimal.Round(
                            (position.Amount * position.Stock.LastPrice - position.TotalPrice) / position.TotalPrice *
                            100, 2)
                };

                var sector = position.Stock.Sector.Name;
                if (sector.IsNullOrEmpty())
                    continue;

                var sla = result.PercentagePerSectors.FirstOrDefault(x => x.Name == sector);
                if (sla == null)
                {
                    result.PercentagePerSectors.Add(new SectorDto
                    {
                        Name = sector,
                        TotalPrice = position.TotalPrice,
                        TotalValue = position.Amount * position.Stock.LastPrice,
                        Positions = new List<PositionDto> { positionDto }
                    });
                }
                else
                {
                    sla.TotalPrice += position.TotalPrice;
                    sla.TotalValue += position.Amount * position.Stock.LastPrice;
                    sla.Positions.Add(positionDto);
                }
            }

            result.TotalCost = totalCost;
            result.TotalValue = totalValue;
            result.ResultValue = totalValue - totalCost;
            if (totalValue == 0)
                return Ok(result);

            result.ResultPercentage = totalCost != 0 ? decimal.Round((totalValue - totalCost) / totalCost * 100, 2) : 0;

            result.PercentagePerSectors =
                result.PercentagePerSectors.Select(x =>
                    {
                        x.Positions = x.Positions.OrderByDescending(positionDto => positionDto.TotalValue).ToList();
                        x.Percentage = decimal.Round(x.TotalValue / totalValue * 100, 2);
                        x.Profit = x.TotalValue - x.TotalPrice;
                        x.ProfitPctg = decimal.Round(x.Profit / x.TotalPrice * 100, 2);
                        return x;
                    })
                    .OrderByDescending(x => x.Percentage)
                    .ToList();

            return Ok(result);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(e.Message);
        }
    }
}