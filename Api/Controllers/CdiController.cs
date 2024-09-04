using Api.utils;
using Application.Interfaces;
using Infra.ExternalApi.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CdiController : ControllerBase
{
    private readonly IWalletService _walletService;
    private readonly IPositionService _positionService;
    private readonly IBacen _bacen;

    public CdiController(IWalletService walletService, IPositionService positionService, IBacen bacen)
    {
        _walletService = walletService;
        _positionService = positionService;
        _bacen = bacen;
    }

    [HttpGet("Cdi/Absolute")]
    public async Task<IActionResult> CdiAbsolute(int walletId, DateTime? date, Periodicity periodicity)
    {
        try
        {
            if (date >= DateTime.Now)
                return BadRequest("Date must be in past");
            
            var wallet = await _walletService.GetByIdOrDefaultAsync(walletId);
            if (wallet == null)
                return NotFound("Wallet not found");

            var totalAmountList = _positionService.GetTotalAmountByDate(wallet, date);

            var interestsSinceDate = await _bacen.GetInterestsSinceDateAsync(date ?? totalAmountList.MinBy(x=>x.Key).Key);
            if (interestsSinceDate == null || interestsSinceDate.Count == 0)
                return BadRequest("Bacen service unavailable. Try again later");
            
            decimal total = 0, tds = 0, accumulatedPerformance = 0;
            var cumulativeProfit  = new Dictionary<object, decimal>();
            var helper = 0;

            foreach (var interest in interestsSinceDate)
            {
                var position = totalAmountList.FirstOrDefault(x => x.Key <= interest.date.Date);
                if (position.Value != 0)
                {
                    total = position.Value + accumulatedPerformance;
                    tds = position.Value + accumulatedPerformance;
                    totalAmountList.Remove(position.Key);
                }

                var key = Utils.GetKey(interest.date, periodicity);
                
                switch (periodicity)
                {
                    case Periodicity.Monthly:
                        if (helper != interest.date.Month)
                        {//TODO ERRO - NO EXCEL SOMA DE TODOS DA DIFERENÇA
                            tds = total;
                            helper = interest.date.Month;
                        }
                        break;
                    
                    case Periodicity.Annually:
                        if (helper != interest.date.Year)
                        {//TODO ERRO - NO EXCEL SOMA DE TODOS DA DIFERENÇA
                            tds = total;
                            helper = interest.date.Year;
                        }
                        break;
                    
                    default:
                        tds = total;
                        break;
                }
                total *= 1 + interest.interest / 100;
                accumulatedPerformance += total - tds;
                cumulativeProfit[key] = decimal.Round(total - tds, 2);
            }

            return Ok(cumulativeProfit);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(e.Message);
        }
    }
    
    [HttpGet("Cdi/Absolute/Accumulated")]
    public async Task<IActionResult> CdiAbsoluteAccumulated(int walletId, DateTime? date, Periodicity periodicity)
    {
        try
        {
            if (date >= DateTime.Now)
                return BadRequest("Date must be in past");
            
            var wallet = await _walletService.GetByIdOrDefaultAsync(walletId);
            if (wallet == null)
                return NotFound("Wallet not found");

            var totalAmountList = _positionService.GetTotalAmountByDate(wallet, date);

            var interestsSinceDate = await _bacen.GetInterestsSinceDateAsync(date ?? totalAmountList.MinBy(x=>x.Key).Key);
            if (interestsSinceDate == null || interestsSinceDate.Count == 0)
                return BadRequest("Bacen service unavailable. Try again later");
            
            decimal total = 0, tds = 0, accumulatedPerformance = 0;
            var cumulativeProfit  = new Dictionary<object, decimal>();

            foreach (var interest in interestsSinceDate)
            {
                if (interest.date.Date >= new DateTime(2024,03,27))
                {
                    
                }
                var position = totalAmountList.FirstOrDefault(x => x.Key <= interest.date.Date);
                if (position.Value != 0)
                {
                    total = position.Value + accumulatedPerformance;
                    tds = position.Value;
                    totalAmountList.Remove(position.Key);
                }

                
                var key = Utils.GetKey(interest.date, periodicity);
                
                total *= 1 + interest.interest / 100;
                accumulatedPerformance = total - tds;
                cumulativeProfit [key] = decimal.Round(total - tds, 2);
            }

            return Ok(cumulativeProfit);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(e.Message);
        }
    }

    // [HttpGet("Cdi/Percentage")]
    // public async Task<IActionResult> CdiPercentage(int walletId, DateTime? date, Periodicity periodicity)
    // {
    //     try
    //     {
    //         var monthly = false;
    //         var daily = false;
    //         
    //         switch (periodicity)
    //         {
    //             case 1:
    //                 monthly = true;
    //                 break;
    //             case 2:
    //                 break;
    //             default:
    //                 daily = true;
    //                 break;
    //         }
    //         
    //         var wallet = await _walletService.GetByIdOrDefaultAsync(walletId);
    //         if (wallet == null)
    //             return NotFound("Wallet not found");
    //         if (date >= DateTime.Now)
    //             return BadRequest("Date must be in past");
    //
    //         var totalAmountList = _positionService.GetTotalAmountByDate(wallet, date);
    //
    //         var interestsSinceDate = await _bacen.GetInterestsSinceDateAsync(date ?? totalAmountList.MinBy(x=>x.Key).Key);
    //         if (interestsSinceDate == null || interestsSinceDate.Count == 0)
    //             return BadRequest("Bacen service unavailable. Try again later");
    //         
    //         decimal total = 0;
    //         decimal tds = 0;
    //         var cumulativeProfit = new Dictionary<object, decimal>();
    //         var helper = 0;
    //
    //         foreach (var interest in interestsSinceDate)
    //         {
    //             var position = totalAmountList.FirstOrDefault(x => x.Key <= interest.date.Date);
    //             if (position.Value != 0)
    //             {
    //                 total += position.Value;
    //                 tds += position.Value;
    //                 totalAmountList.Remove(position.Key);
    //             }
    //
    //             if (daily)
    //             {
    //                 total *= 1 + interest.interest / 100;
    //                 cumulativeProfit [interest.date.Date] = decimal.Round((total - tds)/ tds * 100, 2);
    //                 tds = total;
    //             }
    //             else if (monthly)
    //             {
    //                 if (helper != interest.date.Month) 
    //                 {
    //                     tds = total;
    //                     helper = interest.date.Month;
    //                 }
    //                 total *= 1 + interest.interest / 100;
    //                 cumulativeProfit [interest.date.ToString("MM/yyyy")] = decimal.Round((total - tds)/ tds * 100, 2);
    //
    //             }
    //             else 
    //             {
    //                 if (helper != interest.date.Year) 
    //                 {
    //                     tds = total;
    //                     helper = interest.date.Year;
    //                 }
    //                 total *= 1 + interest.interest / 100;
    //                 cumulativeProfit [interest.date.Year] = decimal.Round((total - tds)/ tds * 100, 2);
    //             }
    //         }
    //
    //         return Ok(cumulativeProfit);
    //     }
    //     catch (Exception e)
    //     {
    //         Console.WriteLine(e);
    //         return BadRequest(e.Message);
    //     }
    // }
    //
    // [HttpGet("Cdi/Percentage/Accumulated")]
    // public async Task<IActionResult> CdiPercentageAccumulated(int walletId, DateTime? date, Periodicity periodicity)
    // {
    //     try
    //     {
    //         var monthly = false;
    //         var daily = false;
    //         
    //         switch (periodicity)
    //         {
    //             case 1:
    //                 monthly = true;
    //                 break;
    //             case 2:
    //                 break;
    //             default:
    //                 daily = true;
    //                 break;
    //         }
    //         
    //         var wallet = await _walletService.GetByIdOrDefaultAsync(walletId);
    //         if (wallet == null)
    //             return NotFound("Wallet not found");
    //         if (date >= DateTime.Now)
    //             return BadRequest("Date must be in past");
    //
    //         var totalAmountList = _positionService.GetTotalAmountByDate(wallet, date);
    //
    //         var interestsSinceDate = await _bacen.GetInterestsSinceDateAsync(date ?? totalAmountList.MinBy(x=>x.Key).Key);
    //         if (interestsSinceDate == null || interestsSinceDate.Count == 0)
    //             return BadRequest("Bacen service unavailable. Try again later");
    //         
    //         decimal total = 0;
    //         decimal tds = 0;
    //         var cumulativeProfit = new Dictionary<object, decimal>();
    //
    //         foreach (var interest in interestsSinceDate)
    //         {
    //             var position = totalAmountList.FirstOrDefault(x => x.Key <= interest.date.Date);
    //             if (position.Value != 0)
    //             {
    //                 total += position.Value;
    //                 tds += position.Value;
    //                 totalAmountList.Remove(position.Key);
    //             }
    //
    //             total *= 1 + interest.interest / 100;
    //             
    //             if (daily)
    //             {
    //                 cumulativeProfit [interest.date.Date] = decimal.Round((total - tds)/ tds * 100, 2);
    //             }
    //             else if (monthly)
    //             {
    //                 cumulativeProfit [interest.date.ToString("MM/yyyy")] = decimal.Round((total - tds)/ tds * 100, 2);
    //             }
    //             else 
    //             {
    //                 cumulativeProfit [interest.date.Year] = decimal.Round((total - tds)/ tds * 100, 2);
    //             }
    //         }
    //
    //         return Ok(cumulativeProfit);
    //     }
    //     catch (Exception e)
    //     {
    //         Console.WriteLine(e);
    //         return BadRequest(e.Message);
    //     }
    // }
}