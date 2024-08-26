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
    
    public DefaultController(IBovespa bovespa, IStockService stockService, ISectorService sectorService, IUnitOfWork unitOfWork, IWalletService walletService, IPositionService positionService, ITransactionHistoryService transactionService)
    {
        _bovespa = bovespa;
        _stockService = stockService;
        _sectorService = sectorService;
        _unitOfWork = unitOfWork;
        _walletService = walletService;
        _transactionService = transactionService;
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

            var position =
                await _positionService
                    .GetPositionByWalletAndStockOrDefaultAsync(history.WalletId, history.StockId) ??
                new Positions
                {
                    WalletId = history.WalletId,
                    Stock = stock
                };

            position.Amount += transactionDto.Amount;
            position.TotalPrice += transactionDto.Amount * transactionDto.Price;

            if (position.Id != 0)
                _positionService.Put(position);
            else
                await _positionService.CreateAsync(position);

            // TODO Inserir codigo para o positionhistory
            
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

            var history = transactionDto.Adapt<TransactionHistory>();
            history.Stock = stock;
            history.Amount = history.Amount > 0 ? -history.Amount : history.Amount;
            await _transactionService.CreateAsync(history);

            var position =
                await _positionService.GetPositionByWalletAndStockOrDefaultAsync(history.WalletId, history.StockId);

            if (position == null)
                return BadRequest("You dont have position for this stock");
            
            if (position.Amount != 0)
            {
                history.EquityEffect = -(position.TotalPrice / position.Amount * transactionDto.Amount);
                position.TotalPrice -= position.TotalPrice / position.Amount * transactionDto.Amount;
                position.Amount -= transactionDto.Amount;
            }

            if (position.Amount < 0 || position.TotalPrice < 0)
                return BadRequest("Invalid amount, ");

            _positionService.Put(position);
            
            // TODO Inserir codigo para o positionhistory
            
            await _unitOfWork.SaveChangesAsync();

            return Ok(history);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(e.Message);
        }
    }
}