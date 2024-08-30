using Application.Dtos.Input;
using Application.Interfaces;
using Domain.Models;
using Infra.ExternalApi.Interfaces;
using Infra.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;


[Route("api/[controller]")]
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
    private readonly IStockHistoryService _stockHistoryService;
    private readonly IBacen _bacen;
    
    
    public DefaultController(IBovespa bovespa, IStockService stockService, ISectorService sectorService, IUnitOfWork unitOfWork, IWalletService walletService, IPositionService positionService, ITransactionHistoryService transactionService, IPositionHistoryService positionHistoryService, IStockHistoryService stockHistoryService, IBacen bacen)
    {
        _bovespa = bovespa;
        _stockService = stockService;
        _sectorService = sectorService;
        _unitOfWork = unitOfWork;
        _walletService = walletService;
        _transactionService = transactionService;
        _positionHistoryService = positionHistoryService;
        _stockHistoryService = stockHistoryService;
        _bacen = bacen;
        _positionService = positionService;
    }
    
    [HttpPost("CreateWallet")]
    public async Task<IActionResult> CreateWallet(string name)
    {
        try
        {
            var wallet = new Wallet
            {
                Name = name
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
    
    [HttpPost("BuyStock")]
    public async Task<IActionResult> BuyStock(TransactionDto transactionDto)
    {
        try
        {
            if (transactionDto.StockSymbol == null)
                return BadRequest("Stock cannot be null");

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
    public async Task<IActionResult> SellStock(TransactionDto transactionDto)
    {
        try
        {
            if (transactionDto.StockSymbol == null)
                return BadRequest("Stock cannot be null");

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
    public async Task<IActionResult> GetTransactionHistory(int id)
    {try
        {

            var wallet = await _walletService.GetByIdOrDefaultAsync(id);
            if (wallet == null)
                return NotFound();

            return Ok(await _transactionService.GetAllByWalletIdAsync(wallet.Id));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(e.Message);
        }
    }
    
    [HttpDelete("DeleteTransaction")]
    public async Task<IActionResult> DeleteTransaction(int id)
    {
        try
        {
            var transaction = await _transactionService.GetByIdOrDefaultAsync(id);
            if (transaction == null)
                return NotFound();
            
            var position =
                await _positionService.GetPositionByWalletAndStockOrDefaultAsync(transaction.WalletId, transaction.StockId);
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
    
    [HttpGet("Cdi/Daily/Absolute")]
    public async Task<IActionResult> CdiAbsolute(int walletId, DateTime? date)
    {
        try
        {
            var wallet = await _walletService.GetByIdOrDefaultAsync(walletId);
            if (wallet == null)
                return NotFound("Wallet not found");
            if (date >= DateTime.Now)
                return BadRequest("Date must be in past");

            var totalAmountList = _positionService.GetTotalAmountByDate(wallet, date);

            var interestsSinceDate = await _bacen.GetInterestsSinceDateAsync(date ?? totalAmountList.MinBy(x=>x.Key).Key);
            if (interestsSinceDate == null || interestsSinceDate.Count == 0)
                return BadRequest("Bacen service unavailable. Try again later");
            
            decimal total = 0;
            decimal tds = 0;
            var cumulativeProfit  = new Dictionary<DateTime, decimal>();

            foreach (var interest in interestsSinceDate)
            {
                var position = totalAmountList.FirstOrDefault(x => x.Key <= interest.date.Date);
                if (position.Value != 0)
                {
                    total += position.Value;
                    tds += position.Value;
                    totalAmountList.Remove(position.Key);
                }

                total *= 1 + interest.interest / 100;

                cumulativeProfit [interest.date.Date] = decimal.Round(total - tds, 2);
            }

            return Ok(cumulativeProfit );
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(e.Message);
        }
    }

    [HttpGet("Cdi/Daily/Percentage")]
    public async Task<IActionResult> CdiPercentage(int walletId, DateTime? date)
    {
        try
        {
            var wallet = await _walletService.GetByIdOrDefaultAsync(walletId);
            if (wallet == null)
                return NotFound("Wallet not found");
            if (date >= DateTime.Now)
                return BadRequest("Date must be in past");

            var totalAmountList = _positionService.GetTotalAmountByDate(wallet, date);

            var interestsSinceDate = await _bacen.GetInterestsSinceDateAsync(date ?? totalAmountList.MinBy(x=>x.Key).Key);
            if (interestsSinceDate == null || interestsSinceDate.Count == 0)
                return BadRequest("Bacen service unavailable. Try again later");
            
            decimal total = 0;
            decimal tds = 0;
            var cumulativeProfit = new Dictionary<DateTime, decimal>();

            foreach (var interest in interestsSinceDate)
            {
                var position = totalAmountList.FirstOrDefault(x => x.Key <= interest.date.Date);
                if (position.Value != 0)
                {
                    total += position.Value;
                    tds += position.Value;
                    totalAmountList.Remove(position.Key);
                }

                total *= 1 + interest.interest / 100;

                cumulativeProfit[interest.date.Date] = decimal.Round((total - tds)/ tds * 100, 3);
            }

            return Ok(cumulativeProfit);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(e.Message);
        }
    }
    
    [HttpGet("Variation/Daily/Absolute")]
    public async Task<IActionResult> VariationAbsolute(int walletId, DateTime? date)
    {
        var wallet = await _walletService.GetByIdOrDefaultAsync(walletId);
        if (wallet == null)
            return NotFound();
        if (date >= DateTime.Now)
            return BadRequest("Date must be in past");
        
        var result = new Dictionary<DateTime, decimal>();
        
        foreach (var positions in wallet.Positions)
        {
            var positionHistoryList = _positionService.GetPositionHistoriesAfterDateOrLast(positions, date);
            if (positionHistoryList.Count == 0)
                continue;
            
            var stockHistoryList =
                await _stockHistoryService.GetStockHistoryList(positions.Stock,
                    date ?? positionHistoryList.First().Date);
            if (stockHistoryList.Count == 0)
                continue;
            
            var qnt = 0;
            decimal cost = 0;
            foreach (var stock in stockHistoryList)
            {
                if (stock.Date.Date < date?.Date)
                    continue;
                
                var t = positionHistoryList.FirstOrDefault(x => x.Date.Date <= stock.Date.Date);
                if (t != null)
                {
                    qnt = t.Amount;
                    cost = t.TotalPrice;
                }
                
                if (qnt == 0 && date == null)
                    continue;

                if (result.TryGetValue(stock.Date.Date, out var value))
                    result[stock.Date.Date] = value + (stock.Close * qnt - cost);
                else
                    result[stock.Date.Date] = stock.Close * qnt-cost;
            }
        }
        return Ok(result.OrderBy(x=>x.Key).ToDictionary());
    }
    
    [HttpGet("Variation/Daily/Percentage")]
    public async Task<IActionResult> VariationPercentage(int walletId, DateTime? date)
    {
        var wallet = await _walletService.GetByIdOrDefaultAsync(walletId);
        if (wallet == null)
            return NotFound();
        if (date >= DateTime.Now)
            return BadRequest("Date must be in past");
        
        var result = new Dictionary<DateTime, decimal>();
        
        foreach (var positions in wallet.Positions)
        {
            var positionHistoryList = _positionService.GetPositionHistoriesAfterDateOrLast(positions, date);
            if (positionHistoryList.Count == 0)
                continue;
            
            var stockHistoryList =
                await _stockHistoryService.GetStockHistoryList(positions.Stock,
                    date ?? positionHistoryList.First().Date);
            if (stockHistoryList.Count == 0)
                continue;
            
            var qnt = 0;
            decimal cost = 0;
            foreach (var stock in stockHistoryList)
            {
                if (stock.Date.Date < date?.Date)
                    continue;
                
                var t = positionHistoryList.FirstOrDefault(x => x.Date.Date <= stock.Date.Date);
                if (t != null)
                {
                    qnt = t.Amount;
                    cost = t.TotalPrice;
                }
                
                if (qnt == 0 && date == null)
                    continue;

                if (result.TryGetValue(stock.Date.Date, out var value))
                    result[stock.Date.Date] = decimal.Round((value + (stock.Close * qnt - cost) / cost * 100)/2, 2);
                else
                    result[stock.Date.Date] = (stock.Close * qnt-cost)/ cost * 100;
            }
        }
        return Ok(result.OrderBy(x=>x.Key).ToDictionary());
    }
}