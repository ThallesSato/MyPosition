using Api.Facades;
using Application.Interfaces;
using Application.utils;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class VariationController : ControllerBase
{
    private readonly IWalletService _walletService;
    private readonly VariationFacade _variationFacade;

    public VariationController(IWalletService walletService, IPositionService positionService, IStockHistoryService stockHistoryService)
    {
        _walletService = walletService;
        _variationFacade = new VariationFacade(positionService, stockHistoryService);
    }

    [HttpGet("Absolute/Accumulated")]
    public async Task<IActionResult> VariationAbsoluteAccumulated(int walletId, DateTime? date, Periodicity periodicity)
    {
        if (date >= DateTime.Now)
            return BadRequest("Date must be in past");
        
        var wallet = await _walletService.GetByIdOrDefaultAsync(walletId);
        if (wallet == null)
            return NotFound();
        
        var result = await _variationFacade.VariationAbsoluteAccumulated(date, periodicity, wallet);
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
        
        var result = await _variationFacade.VariationAbsolute(date, periodicity, wallet);
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
        
        var result = await _variationFacade.VariationPercentageAccumulated(date, periodicity, wallet);
        return Ok(result);
    }

    [HttpGet("Percentage")]
    public async Task<IActionResult> VariationPercentage(int walletId, DateTime? date, Periodicity periodicity)
    {
        if (date >= DateTime.Now)
            return BadRequest("Date must be in past");
        
        var wallet = await _walletService.GetByIdOrDefaultAsync(walletId);
        if (wallet == null)
            return NotFound();
        
        var result = await _variationFacade.VariationPercentage(date, periodicity, wallet);
        return Ok(result);
    }

}