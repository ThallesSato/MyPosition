using System;
using System.Threading.Tasks;
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
    private readonly IBaseService<Positions> _positionService;
    private readonly IWalletService _walletService;
    
    public DefaultController(IBovespa bovespa, IStockService stockService, ISectorService sectorService, IUnitOfWork unitOfWork, IWalletService walletService, IBaseService<Positions> positionService)
    {
        _bovespa = bovespa;
        _stockService = stockService;
        _sectorService = sectorService;
        _unitOfWork = unitOfWork;
        _walletService = walletService;
        _positionService = positionService;
    }
    
    [HttpPost("stock")]
    public async Task<IActionResult> InsertStock(string symbol)
    {
        try
        {
            var (stockDto, message) = await _bovespa.GetStock(symbol);
            
            if (stockDto == null) 
                return BadRequest(message);
            
            var sector = await _sectorService.GetOrCreateSectorAsync(stockDto.Sector);
            var stock =await _stockService.CreateStock(stockDto, sector);

            await _unitOfWork.SaveChangesAsync();
            return Ok(stock);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(e.Message);
        }
    }
    
    [HttpGet("stock")]
    public async Task<IActionResult> GetStocks()
    {
        var stocks = await _stockService.GetAllAsync();
        return Ok(stocks);
    }
    
    [HttpPost("wallet")]
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
    
    [HttpGet("wallet")]
    public async Task<IActionResult> GetWallets()
    {
        var wallets = await _walletService.GetAllAsync();
        return Ok(wallets);
    }
    
    [HttpGet("walletid")]
    public async Task<IActionResult> GetWallet(int id)
    {
        try
        {
            var wallets = await _walletService.GetByIdOrDefaultAsync(id);
            if (wallets == null)
                return NotFound();

            decimal totalValue = 0, totalCost = 0;
            
            foreach (var position in wallets.Positions)
            {
                totalCost += position.Amount * position.TotalPrice;
                totalValue += position.Amount * position.Stock.LastPrice;
            }
            Console.WriteLine("totalCost: " + totalCost);
            Console.WriteLine("totalValue: " + totalValue);
            Console.WriteLine("Lucro: " + (totalValue-totalCost));
            return Ok(wallets);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(e.Message);
        }
    }
    
    [HttpPost("UpdateStocks")]
    public async Task<IActionResult> UpdateAllStocks()
    {
        try
        {
            var stocks = await _stockService.GetAllAsync();
            foreach (var stock in stocks)
            {
                stock.LastPrice = await _bovespa.UpdatePrice(stock) ?? stock.LastPrice;
                _stockService.Put(stock);
            }
            await _unitOfWork.SaveChangesAsync();
            return Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(e.Message);
        }
    }
}