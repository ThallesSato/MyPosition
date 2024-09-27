using System.Collections.Specialized;
using Api.Facades;
using Application.Dtos.Output;
using Application.Interfaces;
using Application.utils;
using Domain.Models;
using Infra.ExternalApi.Interfaces;
using Infra.Interfaces;
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
    private readonly ITransactionHistoryService _transactionService;
    private readonly IWalletService _walletService;
    private readonly IStockHistoryService _stockHistoryService;
    private readonly IBacen _bacen;
    private readonly IPositionHistoryService _positionHistoryService;
    private readonly VariationFacade _variationFacade;

    public TesteController(IBovespa bovespa, IStockService stockService, ISectorService sectorService, IPositionService positionService, IWalletService walletService, ITransactionHistoryService transactionService, IUnitOfWork unitOfWork, IStockHistoryService stockHistoryService, IBacen bacen, IPositionHistoryService positionHistoryService)
    {
        _bovespa = bovespa;
        _stockService = stockService;
        _sectorService = sectorService;
        _positionService = positionService;
        _walletService = walletService;
        _transactionService = transactionService;
        _unitOfWork = unitOfWork;
        _stockHistoryService = stockHistoryService;
        _bacen = bacen;
        _positionHistoryService = positionHistoryService;
        _variationFacade = new VariationFacade(positionService, stockHistoryService);
    }

    //TODO by sectors (daily, monthly, annually)
    
    //TODO IPORTIFOLIO
    
    [HttpGet]
    public async Task<IActionResult> Get(int id)
    {
        try
        {
            await _stockService.UpdateAllStocksAsync();
            
            var wallet = await _walletService.GetByIdOrDefaultAsync(id);
            if (wallet == null)
                return NotFound();

            decimal totalValue = 0, totalCost = 0;
            var sectorValues = new Dictionary<string, decimal>();
            
            foreach (var position in wallet.Positions)
            {
                totalCost += position.TotalPrice;
                totalValue += position.Amount * position.Stock.LastPrice;
                
                var sector = position.Stock.Setor?.Name;
                if (sector == null) 
                    continue;

                if (sectorValues.TryGetValue(sector, out var currentValue))
                {
                    sectorValues[sector] = currentValue + position.Amount * position.Stock.LastPrice;
                }
                else
                {
                    sectorValues[sector] = position.Amount * position.Stock.LastPrice;
                }
            }
            
            var result = new TotalDto
            {
                TotalCost = totalCost,
                TotalValue = totalValue,
                ResultValue = totalValue - totalCost,
                Wallet = wallet,
                ResultPercentage = totalCost != 0 ? 
                    decimal.Round((totalValue - totalCost) / totalCost * 100, 2) : 0
            };
            
            if (totalValue == 0)
                return Ok(result);
            
            //TODO COLOCAR OS 5 MAIORES E DPS "OUTROS"
            
            result.PercentagePerSectors = sectorValues.OrderByDescending(x => x.Value)
                .Select(x => new SectorPctg(x.Key, decimal.Round(x.Value / totalValue * 100, 2))).ToList();
            return Ok(result);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(e.Message);
        }
    }
    
    [HttpGet("Percentage/Accumulated")]
    public async Task<IActionResult> PercentageAccumulated(int walletId, DateTime? date, GraphType graphType, Periodicity periodicity)
    {
        if (date >= DateTime.Now)
            return BadRequest("Date must be in past");
        
        var wallet = await _walletService.GetByIdOrDefaultAsync(walletId);
        if (wallet == null)
            return NotFound();
        
        var totalAmountList = _positionService.GetTotalAmountByDate(wallet, date);
        List<(DateTime date, decimal interest)>? interestsSinceDate;
        var helper = 0;
        do {
            interestsSinceDate = await _bacen.GetInterestsSinceDateAsync(date ?? totalAmountList.MinBy(x=>x.Key).Key);
            helper++;
        } while (interestsSinceDate == null && helper < 5);
        if (interestsSinceDate == null || interestsSinceDate.Count == 0)
            return BadRequest("Bacen service unavailable. Try again later");

        Dictionary<object, decimal> cdi;
        SortedDictionary<string, decimal> variation;
        
        switch (graphType)
        {
            case GraphType.Absolute:
                cdi = CdiFacade.CdiAbsolute(periodicity, interestsSinceDate, totalAmountList);
                variation = await _variationFacade.VariationAbsolute(date, periodicity, wallet);
                break;
            case GraphType.AbsoluteAccumulated:
                cdi = CdiFacade.CdiAbsoluteAccumulated(periodicity, interestsSinceDate, totalAmountList);
                variation = await _variationFacade.VariationAbsoluteAccumulated(date, periodicity, wallet);
                break;
            case GraphType.Percentage:
                cdi = CdiFacade.CdiPercentage(periodicity, interestsSinceDate, totalAmountList);
                variation = await _variationFacade.VariationPercentage(date, periodicity, wallet);
                break;
            case GraphType.PercentageAccumulated:
            default:
                cdi = CdiFacade.CdiPercentageAccumulated(periodicity, interestsSinceDate, totalAmountList);
                variation = await _variationFacade.VariationPercentageAccumulated(date, periodicity, wallet);
                break;
        }
        
        var response = new
        {
            falha = variation.Count != cdi.Count,
            dates = variation.Select(x=>x.Key.ToString()), 
            variation = variation.Select(x=>x.Value),
            cdi = cdi.Select(x=>x.Value)
        };
        return Ok(response);
    }
    
}