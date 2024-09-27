using Domain.Models;

namespace Application.utils;

public static class Utils
{
    public static string GetKey(DateTime date, Periodicity periodicity)
    {
        switch (periodicity)
        {
            case Periodicity.Monthly:
                return date.Date.ToString("yyyy/MM");
            case Periodicity.Annually:
                return date.Date.ToString("yyyy");
            case Periodicity.Daily:
            default:
                return date.Date.ToString("yyyy-MM-dd");
        }
    }
    
    public static List<StockHistory> PeriodicityListAndFiltered(Periodicity periodicity, List<StockHistory> stockHistoryList, DateTime? date)
    {
        stockHistoryList = periodicity switch
        {
            Periodicity.Monthly => stockHistoryList.GroupBy(sh => new { sh.Date.Year, sh.Date.Month })
                .Select(g => g.OrderByDescending(sh => sh.Date).First())
                .ToList(),
            Periodicity.Annually => stockHistoryList.GroupBy(sh => new { sh.Date.Year })
                .Select(g => g.OrderByDescending(sh => sh.Date).First())
                .ToList(),
            _ => stockHistoryList
        };
        if (date != null)
            stockHistoryList = stockHistoryList.Where(sh => sh.Date.Date >= date?.Date).ToList();
        return stockHistoryList;
    }
}