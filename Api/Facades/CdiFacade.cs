using Application.utils;

namespace Api.Facades;

public static class CdiFacade
{
    public static Dictionary<object, decimal> CdiAbsolute(Periodicity periodicity, List<(DateTime date, decimal interest)> interestsSinceDate, Dictionary<DateTime, decimal> totalAmountList)
    {
        decimal total = 0, tds = 0, lastPosition = 0;
        var cumulativeProfit  = new Dictionary<object, decimal>();
        var helper = 0;

        foreach (var interest in interestsSinceDate)
        {
            var position = totalAmountList.FirstOrDefault(x => x.Key <= interest.date.Date);
            if (position.Value != 0)
            {
                total += position.Value - lastPosition;
                tds += position.Value - lastPosition;
                lastPosition = position.Value;
                totalAmountList.Remove(position.Key);
            }

            var key = Utils.GetKey(interest.date, periodicity);
                
            if (helper != periodicity switch
                {
                    Periodicity.Monthly => interest.date.Month,
                    Periodicity.Annually => interest.date.Year,
                    Periodicity.Daily => -1,
                    _ => -1
                })
            {
                tds = total;
                helper = periodicity switch
                {
                    Periodicity.Monthly => interest.date.Month,
                    Periodicity.Annually => interest.date.Year,
                    Periodicity.Daily => 0,
                    _ => 0
                };
            }
                
            total *= 1 + interest.interest / 100;
            cumulativeProfit[key] = decimal.Round(total - tds, 2);
        }

        return cumulativeProfit;
    }

    public static Dictionary<object, decimal> CdiAbsoluteAccumulated(Periodicity periodicity, List<(DateTime date, decimal interest)> interestsSinceDate,
        Dictionary<DateTime, decimal> totalAmountList)
    {
        decimal total = 0, tds = 0;
        var cumulativeProfit  = new Dictionary<object, decimal>();

        foreach (var interest in interestsSinceDate)
        {
            var position = totalAmountList.FirstOrDefault(x => x.Key <= interest.date.Date);
            if (position.Value != 0)
            {
                total = position.Value + total - tds;
                tds = position.Value;
                totalAmountList.Remove(position.Key);
            }
            var key = Utils.GetKey(interest.date, periodicity);
                
            total *= 1 + interest.interest / 100;
            cumulativeProfit [key] = decimal.Round(total - tds, 2);
        }

        return cumulativeProfit;
    }
    
    public static Dictionary<object, decimal> CdiPercentage(Periodicity periodicity, List<(DateTime date, decimal interest)> interestsSinceDate, Dictionary<DateTime, decimal> totalAmountList)
    {
        decimal total = 0;
        decimal tds = 0;
        var cumulativeProfit = new Dictionary<object, decimal>();
        object helper = 0;
            
        foreach (var interest in interestsSinceDate)
        {
            var position = totalAmountList.FirstOrDefault(x => x.Key <= interest.date.Date);
            if (position.Value != 0)
            {
                total += position.Value;
                tds += position.Value;
                totalAmountList.Remove(position.Key);
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
    
    public static Dictionary<object, decimal> CdiPercentageAccumulated(Periodicity periodicity, List<(DateTime date, decimal interest)> interestsSinceDate,
        Dictionary<DateTime, decimal> totalAmountList)
    {
        decimal total = 0;
        decimal tds = 0;
        var cumulativeProfit = new Dictionary<object, decimal>();
    
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
                
            var key = Utils.GetKey(interest.date.Date, periodicity);
                
            cumulativeProfit [key] = tds == 0 ? 0 : decimal.Round((total - tds)/ tds * 100, 2);
        }

        return cumulativeProfit;
    }
}