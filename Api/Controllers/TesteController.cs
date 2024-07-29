using System;
using System.Linq;
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
public class TesteController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBovespa _bovespa;
    private readonly IStockService _stockService;
    private readonly ISectorService _sectorService;
    private readonly IPositionService _positionService;
    private readonly IBaseService<TransactionHistory> _transactionService;
    private readonly IWalletService _walletService;

    public TesteController(IBovespa bovespa, IStockService stockService, ISectorService sectorService, IPositionService positionService, IWalletService walletService, IBaseService<TransactionHistory> transactionService, IUnitOfWork unitOfWork)
    {
        _bovespa = bovespa;
        _stockService = stockService;
        _sectorService = sectorService;
        _positionService = positionService;
        _walletService = walletService;
        _transactionService = transactionService;
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> Get(int id)
    {
        try
        {
            var wallets = await _walletService.GetByIdOrDefaultAsync(id);
            if (wallets == null)
                return NotFound();

            decimal totalValue = 0, totalCost = 0;

            foreach (var position in wallets.Positions)
            {
                if (position.Stock == null)
                    continue;

                totalCost += position.TotalPrice;
                totalValue += position.Amount * position.Stock.LastPrice;
            }

            Console.WriteLine("TotalCost: " + totalCost);
            Console.WriteLine("TotalValue: " + totalValue);
            Console.WriteLine("Result Value: " + (totalValue - totalCost));

            if (totalValue != 0)
                Console.WriteLine("Result % : " + decimal.Round((totalValue - totalCost) / totalCost * 100, 2));

            var sectors = wallets.Positions
                .Select(position => position.Stock?.Setor?.Name)
                .Distinct()
                .ToList();

            Console.WriteLine("% por setores");

            foreach (var sector in sectors)
            {
                var totalValueBySector = wallets.Positions
                    .Where(position => position.Stock?.Setor?.Name == sector)
                    .Sum(position => position.Amount * position.Stock?.LastPrice) ?? 0;
                
                if (totalValue != 0 && totalValueBySector != 0)
                    Console.WriteLine(sector + " % : " + decimal.Round(totalValueBySector / totalValue * 100));
            }

            return Ok(wallets);
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
        var history = transactionDto.Adapt<TransactionHistory>();
        await _transactionService.CreateAsync(history);
        
        var position = await _positionService.GetOrCreateAsync(history.WalletId, history.StockId);
        
        position.Amount += transactionDto.Amount;
        position.TotalPrice += transactionDto.Amount * transactionDto.Price;
        
        if (position.Id > 0)
            _positionService.Put(position);
        
        await _unitOfWork.SaveChangesAsync();
        
        return Ok(position);
    }
    [HttpPost("SellStock")]
    public async Task<IActionResult> SellStock(TransactionDto transactionDto)
    {
        var history = transactionDto.Adapt<TransactionHistory>();
        await _transactionService.CreateAsync(history);
        
        var position = await _positionService.GetOrCreateAsync(history.WalletId, history.StockId);

        if (position.Amount != 0)
        {
            position.TotalPrice -= position.TotalPrice/position.Amount * transactionDto.Amount;
            position.Amount -= transactionDto.Amount;
        }

        if (position.Amount < 0 || position.TotalPrice < 0)
            return BadRequest("Invalid amount, ");
        
        _positionService.Put(position);
        await _unitOfWork.SaveChangesAsync();
        
        return Ok(history);
    }
}