using Application.Interfaces;
using Application.utils;
using Domain.Models;

namespace Api.Facades;

public sealed class VariationFacade
{
    private readonly IPositionService _positionService;
    private readonly IStockHistoryService _stockHistoryService;

    public VariationFacade(IPositionService positionService, IStockHistoryService stockHistoryService)
    {
        _positionService = positionService;
        _stockHistoryService = stockHistoryService;
    }

    public async Task<SortedDictionary<string, decimal>> VariationAbsoluteAccumulated(DateTime? date, Periodicity periodicity, List<Positions> positionsList)
    {
        var preresult = new SortedDictionary<string, decimal>();

        foreach (var positions in positionsList)
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
                
                if (preresult.TryGetValue(key, out var value))
                    preresult[key] = value + (stock.Close * qnt - cost);
                else
                    preresult[key] = stock.Close * qnt-cost;
            }
        }

        decimal help = 0;
        var result = new SortedDictionary<string, decimal>();
        foreach (var (data, total) in preresult)
        {
            if (help == 0 && date != null)
                help = total;
            
            result[data] = decimal.Round(total - help, 2);
        }
        return result;
    }

    public async Task<SortedDictionary<string, decimal>> VariationAbsolute(DateTime? date, Periodicity periodicity, List<Positions> positionsList)
    {
        var result = new SortedDictionary<string, decimal>();

        foreach (var positions in positionsList)
        {
            var positionHistoryList = _positionService.GetPositionHistoriesAfterDateAndLast(positions, date);
            if (positionHistoryList.Count == 0)
                continue;
            
            var stockHistoryList =
                await _stockHistoryService.GetStockHistoryList(positions.Stock,
                     positionHistoryList.First().Date);
            if (stockHistoryList.Count == 0)
                continue;
            var referenceDate = date?.Date ?? positionHistoryList.First().Date.Date.AddDays(-1);
            var stockOld = stockHistoryList.Last(x => x.Date.Date <= referenceDate).Close;


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

        return result;
    }
    // public async Task<SortedDictionary<object, decimal>> VariationPercentageAccumulated(DateTime? date, Periodicity periodicity, List<Positions> positionsList)
    // {
    //     var preResult = new SortedDictionary<object, (decimal TotalCost ,List<(decimal StockCost,decimal StockVariation)>)>();
    //     
    //     foreach (var positions in positionsList)
    //     {
    //         var positionHistoryList = _positionService.GetPositionHistoriesAfterDateAndLast(positions, date);
    //         if (positionHistoryList.Count == 0)
    //             continue;
    //         
    //         var stockHistoryList =
    //             await _stockHistoryService.GetStockHistoryList(positions.Stock,
    //                 date ?? positionHistoryList.First().Date);
    //         if (stockHistoryList.Count == 0)
    //             continue;
    //
    //         stockHistoryList = Utils.PeriodicityListAndFiltered(periodicity, stockHistoryList, date);
    //
    //         var qnt = 0;
    //         decimal cost = 0;
    //         foreach (var stock in stockHistoryList)
    //         {
    //             var positionHistory = positionHistoryList.FirstOrDefault(x => x.Date.Date <= stock.Date.Date);
    //             if (positionHistory != null)
    //             {
    //                 qnt = positionHistory.Amount;
    //                 cost = positionHistory.Amount*stock.Close;
    //                 positionHistoryList.Remove(positionHistory);
    //             }
    //             
    //             if (qnt == 0 && date == null)
    //                 continue;
    //
    //             var key = Utils.GetKey(stock.Date, periodicity);
    //             
    //             if (preResult.TryGetValue(key, out var value))
    //             {
    //                 if (cost == 0)
    //                     continue;
    //                 value.Item2.Add((cost, (stock.Close * qnt - cost) / cost));
    //                 preResult[key] = (value.Item1 += cost, value.Item2);
    //             }
    //             else
    //                 if (cost == 0)
    //                     preResult[key] = (0, [(0, 0)]);
    //                 else
    //                     preResult[key] = (cost, [(cost, (stock.Close * qnt - cost) / cost)]);
    //             
    //         }
    //     }
    //
    //     var result = new SortedDictionary<object, decimal>();
    //     foreach (var (data, (total, acoes)) in preResult)
    //     {
    //         foreach (var (stockCost, stockVariation) in acoes)
    //         {
    //             var current = result.GetValueOrDefault(data);
    //             result[data] = current + total == 0 ? 0 : stockCost / total * stockVariation * 100;
    //         }
    //     }
    //
    //     return result;
    // }
    public async Task<SortedDictionary<string, decimal>> VariationPercentage(DateTime? date, Periodicity periodicity, List<Positions> positionsList)
    {
        var preResult = new SortedDictionary<string, (decimal TotalLastMonth ,List<(decimal StockLastMonth,decimal StockVariation)>)>();
        
        foreach (var positions in positionsList)
        {
            var positionHistoryList = _positionService.GetPositionHistoriesAfterDateAndLast(positions, date);
            if (positionHistoryList.Count == 0)
                continue;
            
            var stockHistoryList =
                await _stockHistoryService.GetStockHistoryList(positions.Stock,
                    date ?? positionHistoryList.First().Date);
            if (stockHistoryList.Count == 0)
                continue;
            
            var referenceDate = date?.Date ?? positionHistoryList.First().Date.Date.AddDays(-1);
            var stockOld = stockHistoryList.Last(x => x.Date.Date <= referenceDate.Date).Close;

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

                if (preResult.TryGetValue(key, out var value))
                {
                    value.Item2.Add((stockOld * qnt, (stock.Close - stockOld) / stockOld));
                    preResult[key] = (value.Item1 += stockOld * qnt, value.Item2);
                }
                else
                    preResult[key] = (stockOld * qnt,
                        new List<(decimal StockLastMonth, decimal StockVariation)>
                            { (stockOld * qnt, (stock.Close - stockOld) / stockOld) });
                

                stockOld = stock.Close;
            }
        }

        var result = new SortedDictionary<string, decimal>();
        foreach (var (data, (total, stocks)) in preResult)
        {
            foreach (var (stockLastMonth, stockVariation) in stocks)
            {
                var current = result.GetValueOrDefault(data);
                result[data] = current + stockLastMonth / total * stockVariation * 100;
            }
        }

        return result;
    }
    
    
// Outro pensamento do acumulado
    public async Task<SortedDictionary<string, decimal>> VariationPercentageAccumulated(DateTime? date, Periodicity periodicity, List<Positions> positionsList)
    {
        var preResult = new SortedDictionary<string, (decimal, decimal)>();
        
        DateTime firstDate = DateTime.Today;
        
        foreach (var positions in positionsList)
        {
            var positionHistoryList = _positionService.GetPositionHistoriesAfterDateAndLast(positions, date);
            if (positionHistoryList.Count == 0)
                continue;
            
            var dateTemp = positionHistoryList.First().Date.Date;
            firstDate = firstDate < dateTemp ? firstDate : dateTemp;
            
            var stockHistoryList =
                await _stockHistoryService.GetStockHistoryList(positions.Stock,
                    date ?? firstDate);
            if (stockHistoryList.Count == 0)
                continue;
    
            stockHistoryList = Utils.PeriodicityListAndFiltered(periodicity, stockHistoryList, date);
    
            var qnt = 0;
            decimal cost = 0;
            foreach (var stock in stockHistoryList)
            {
                var positionHistory = positionHistoryList.FirstOrDefault(x => x.Date.Date <= stock.Date.Date);
                if (positionHistory != null)
                {
                    qnt = positionHistory.Amount;
                    cost = positionHistory.TotalPrice;
                    positionHistoryList.Remove(positionHistory);
                }
                
                if (qnt == 0 && date == null)
                    continue;
    
                var key = Utils.GetKey(stock.Date, periodicity);
    
                if (preResult.TryGetValue(key, out var value))
                {
                    if (cost == 0)
                        continue;
                    preResult[key] = (value.Item1 += cost, value.Item2+=stock.Close * qnt);
                }
                else
                {
                    if (cost == 0)
                        preResult[key] = (0, 0);
                    preResult[key] = (cost, stock.Close * qnt);
                }
                
            }
        }
    
        var result = new SortedDictionary<string, decimal>();
        decimal help = 0;
        foreach (var (data, (cost, total)) in preResult)
        {
            if (cost ==0)
            {
                result[data] = 0;
                continue;
            }
            if (help == 0 && date > firstDate)
                help = (total - cost) / cost * 100;
            
            result[data] = decimal.Round((total - cost) / cost * 100-help, 2);
        }
    
        return result;
    }
    
}


