using Application.utils;
using Infra.Dtos.Internal;

namespace Api.Facades;

public static class CdiFacade
{
    public static Dictionary<string, decimal> CdiAbsolute(
        Periodicity periodicity, 
        List<(DateTime date, decimal interest)> interestsSinceDate, 
        List<TotalAmount> totalAmountList)
    {
        decimal total = 0, tds = 0;
        var cumulativeProfit  = new Dictionary<string, decimal>();
        var helper = string.Empty;

        foreach (var interest in interestsSinceDate)
        {
            var position = totalAmountList.FirstOrDefault(x => x.Date <= interest.date.Date);
            if (position != null)
            {
                total += position.Amount;
                tds += position.Amount;
                totalAmountList.Remove(position);
            }

            var key = Utils.GetKey(interest.date, periodicity);
                
            if (helper != key)
            {
                tds = total;
                helper = key;
            }
                
            total *= 1 + interest.interest / 100;
            cumulativeProfit[key] = decimal.Round(total - tds, 2);
        }

        return cumulativeProfit;
    }

    public static Dictionary<string, decimal> CdiAbsoluteAccumulated(
        Periodicity periodicity, 
        List<(DateTime date, decimal interest)> interestsSinceDate,
        List<TotalAmount> totalAmountList)
    {
        decimal total = 0, tds = 0;
        var cumulativeProfit  = new Dictionary<string, decimal>();

        foreach (var interest in interestsSinceDate)
        {
            var position = totalAmountList.FirstOrDefault(x => x.Date <= interest.date.Date);
            if (position != null)
            {
                total += position.Amount;
                tds += position.Amount;
                totalAmountList.Remove(position);
            }
            var key = Utils.GetKey(interest.date, periodicity);
                
            total *= 1 + interest.interest / 100;
            cumulativeProfit [key] = decimal.Round(total - tds, 2);
        }

        return cumulativeProfit;
    }
    
    public static Dictionary<string, decimal> CdiPercentage(
        Periodicity periodicity, 
        List<(DateTime date, decimal interest)> interestsSinceDate, 
        List<TotalAmount> totalAmountList)
    {
        decimal total = 0;
        decimal tds = 0;
        var cumulativeProfit = new Dictionary<string, decimal>();
        object helper = 0;
            
        foreach (var interest in interestsSinceDate)
        {
            var position = totalAmountList.FirstOrDefault(x => x.Date <= interest.date.Date);
            if (position != null)
            {
                total += position.Amount;
                tds += position.Amount;
                totalAmountList.Remove(position);
            }
    
            var key = Utils.GetKey(interest.date.Date, periodicity);

            if (!helper.Equals(key)) 
            {
                tds = total;
                helper = key;
            }
                
            total *= 1 + interest.interest / 100;
            cumulativeProfit [key] = decimal.Round((total - tds)/ tds * 100, 2);
                
        }

        return cumulativeProfit;
    }
    
    public static Dictionary<string, decimal> CdiPercentageAccumulated(
        Periodicity periodicity, 
        List<(DateTime date, decimal interest)> interestsSinceDate,
        List<TotalAmount> totalAmountList)
    {
        decimal total = 0;
        decimal tds = 0;
        var cumulativeProfit = new Dictionary<string, decimal>();
    
        foreach (var interest in interestsSinceDate)
        {
            var position = totalAmountList.FirstOrDefault(x => x.Date <= interest.date.Date);
            if (position != null)
            {
                total += position.Amount;
                tds += position.Amount;
                totalAmountList.Remove(position);
            }
    
            total *= 1 + interest.interest / 100;
                
            var key = Utils.GetKey(interest.date.Date, periodicity);
                
            cumulativeProfit [key] = tds == 0 ? 0 : decimal.Round((total - tds)/ tds * 100, 2);
        }

        return cumulativeProfit;
    }
}