using Api.utils;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class VariationController : ControllerBase
{
    private readonly IWalletService _walletService;
    private readonly IPositionService _positionService;
    private readonly IStockHistoryService _stockHistoryService;

    public VariationController(IWalletService walletService, IPositionService positionService, IStockHistoryService stockHistoryService)
    {
        _walletService = walletService;
        _positionService = positionService;
        _stockHistoryService = stockHistoryService;
    }

    [HttpGet("Absolute/Accumulated")]
    public async Task<IActionResult> VariationAbsoluteAccumulated(int walletId, DateTime? date, Periodicity periodicity)
    {
        if (date >= DateTime.Now)
            return BadRequest("Date must be in past");
        
        var wallet = await _walletService.GetByIdOrDefaultAsync(walletId);
        if (wallet == null)
            return NotFound();
        
        var result = new SortedDictionary<object, decimal>();
        
        foreach (var positions in wallet.Positions)
        {
            var positionHistoryList = _positionService.GetPositionHistoriesAfterDateAndLast(positions, date);
            if (positionHistoryList.Count == 0)
                continue;
            
            var stockHistoryList =
                await _stockHistoryService.GetStockHistoryList(positions.Stock,
                    date ?? positionHistoryList.First().Date);
            if (stockHistoryList.Count == 0)
                continue;
            
            stockHistoryList = Utils.PeriodicityListAndFiltered(periodicity, stockHistoryList, date);
            
            var qnt = 0;
            decimal cost = 0;
            foreach (var stock in stockHistoryList)
            {
                var t = positionHistoryList.Where(x => x.Date.Date <= stock.Date.Date).MaxBy(x=>x.Date);
                if (t != null)
                {
                    qnt = t.Amount;
                    cost = t.TotalPrice;
                    positionHistoryList.Remove(t);
                }

                var key = Utils.GetKey(stock.Date, periodicity);
                
                if (result.TryGetValue(key, out var value))
                    result[key] = value + (stock.Close * qnt - cost);
                else
                    result[key] = stock.Close * qnt-cost;
            }
        }
        return Ok(result);
    }
    
    [HttpGet("Absolute")]
    public async Task<IActionResult> VariationAbsolute(int walletId, DateTime? date, Periodicity periodicity)
    {
        if (date >= DateTime.Now)
            return BadRequest("Date must be in past");
        
        var wallet = await _walletService.GetByIdOrDefaultAsync(walletId);
        if (wallet == null)
            return NotFound();
        
        var result = new SortedDictionary<object, decimal>();
        
        foreach (var positions in wallet.Positions)
        {
            var positionHistoryList = _positionService.GetPositionHistoriesAfterDateAndLast(positions, date);
            if (positionHistoryList.Count == 0)
                continue;
            
            var stockHistoryList =
                await _stockHistoryService.GetStockHistoryList(positions.Stock,
                    date ?? positionHistoryList.First().Date);
            if (stockHistoryList.Count == 0)
                continue;
            var stockOld = stockHistoryList.Last(x=>x.Date.Date < positionHistoryList.First().Date.Date.AddDays(-1)).Close;

            stockHistoryList = Utils.PeriodicityListAndFiltered(periodicity, stockHistoryList, date);
            
            var qnt = 0;
            foreach (var stock in stockHistoryList)
            {
                var t = positionHistoryList.FirstOrDefault(x => x.Date.Date <= stock.Date.Date);
                if (t != null)
                {
                    qnt = t.Amount;
                    positionHistoryList.Remove(t);
                }
                
                if (qnt == 0 && date == null)
                    continue;

                var key = Utils.GetKey(stock.Date, periodicity);
                
                if (result.TryGetValue(key, out var value))
                    result[key] = value + (stock.Close - stockOld) * qnt;
                else
                    result[key] = (stock.Close - stockOld) * qnt;
                
                stockOld = stock.Close;
            }
        }
        return Ok(result);
    }
    
    [HttpGet("Percentage/Accumulated")]
    public async Task<IActionResult> VariationPercentageAccumulated(int walletId, DateTime? date, Periodicity periodicity)
    {
        if (date >= DateTime.Now)
            return BadRequest("Date must be in past");
        
        var wallet = await _walletService.GetByIdOrDefaultAsync(walletId);
        if (wallet == null)
            return NotFound();
        
        var result = new SortedDictionary<object, decimal>();
        
        foreach (var positions in wallet.Positions)
        {
            var positionHistoryList = _positionService.GetPositionHistoriesAfterDateAndLast(positions, date);
            if (positionHistoryList.Count == 0)
                continue;
            
            var stockHistoryList =
                await _stockHistoryService.GetStockHistoryList(positions.Stock,
                    date ?? positionHistoryList.First().Date);
            if (stockHistoryList.Count == 0)
                continue;
            
            stockHistoryList = Utils.PeriodicityListAndFiltered(periodicity, stockHistoryList, date);
            
            var qnt = 0;
            decimal cost = 0;
            foreach (var stock in stockHistoryList)
            {
                var t = positionHistoryList.Where(x => x.Date.Date <= stock.Date.Date).MaxBy(x=>x.Date);
                if (t != null)
                {
                    qnt = t.Amount;
                    cost = t.TotalPrice;
                    positionHistoryList.Remove(t);
                }
                
                if (qnt == 0 && date == null)
                    continue;

                var key = Utils.GetKey(stock.Date, periodicity);

                if (result.TryGetValue(key, out var value))
                    result[key] = value + (stock.Close * qnt-cost)/ cost * 100;
                else
                    result[key] = (stock.Close * qnt-cost)/ cost * 100;
            }
        }
        return Ok(result.OrderBy(x=>x.Key).ToDictionary());
    }
    
    [HttpGet("Percentage")]
    public async Task<IActionResult> VariationPercentage(int walletId, DateTime? date, Periodicity periodicity)
    {
        if (date >= DateTime.Now)
            return BadRequest("Date must be in past");
        
        var wallet = await _walletService.GetByIdOrDefaultAsync(walletId);
        if (wallet == null)
            return NotFound();
        
        var result = new SortedDictionary<object, decimal>();
        
        foreach (var positions in wallet.Positions)
        {
            var positionHistoryList = _positionService.GetPositionHistoriesAfterDateAndLast(positions, date);
            if (positionHistoryList.Count == 0)
                continue;
            
            var stockHistoryList =
                await _stockHistoryService.GetStockHistoryList(positions.Stock,
                    date ?? positionHistoryList.First().Date);
            if (stockHistoryList.Count == 0)
                continue;
            
            var stockOld = stockHistoryList.Last(x=>x.Date.Date < positionHistoryList.First().Date.Date.AddDays(-1)).Close;

            stockHistoryList = Utils.PeriodicityListAndFiltered(periodicity, stockHistoryList, date);

            var qnt = 0;
            foreach (var stock in stockHistoryList)
            {
                var positionHistory = positionHistoryList.FirstOrDefault(x => x.Date.Date <= stock.Date.Date);
                if (positionHistory != null)
                {
                    qnt = positionHistory.Amount;
                    positionHistoryList.Remove(positionHistory);
                }
                
                if (qnt == 0 && date == null)
                    continue;

                var key = Utils.GetKey(stock.Date, periodicity);

                if (result.TryGetValue(key, out var value))
                    result[key] = value + decimal.Round((stock.Close - stockOld) / stockOld * 100, 2);
                else
                    result[key] = decimal.Round((stock.Close - stockOld) / stockOld * 100, 2);

                stockOld = stock.Close;
            }
        }
        return Ok(result.OrderBy(x=>x.Key).ToDictionary());
    }
}