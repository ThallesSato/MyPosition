using Api.Facades;
using Application.Interfaces;
using Application.utils;
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
            
            var cumulativeProfit = CdiFacade.CdiAbsolute(periodicity, interestsSinceDate, totalAmountList);

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
            
            var cumulativeProfit = CdiFacade.CdiAbsoluteAccumulated(periodicity, interestsSinceDate, totalAmountList);

            return Ok(cumulativeProfit);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(e.Message);
        }
    }
    
    [HttpGet("Cdi/Percentage")]
    public async Task<IActionResult> CdiPercentage(int walletId, DateTime? date, Periodicity periodicity)
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
            
            var cumulativeProfit = CdiFacade.CdiPercentage(periodicity, interestsSinceDate, totalAmountList);

            return Ok(cumulativeProfit);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(e.Message);
        }
    }

    [HttpGet("Cdi/Percentage/Accumulated")]
    public async Task<IActionResult> CdiPercentageAccumulated(int walletId, DateTime? date, Periodicity periodicity)
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
            
            var cumulativeProfit = CdiFacade.CdiPercentageAccumulated(periodicity, interestsSinceDate, totalAmountList);

            return Ok(cumulativeProfit);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(e.Message);
        }
    }
}