using Application.Interfaces;
using Infra.ExternalApi.Interfaces;
using Infra.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;


[Route("api/[controller]")]
[ApiController]
public class DefaultController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBovespa _bovespa;
    private readonly IStockService _stockService;
    private readonly ISectorService _sectorService;
    
    public DefaultController(IBovespa bovespa, IStockService stockService, ISectorService sectorService, IUnitOfWork unitOfWork)
    {
        _bovespa = bovespa;
        _stockService = stockService;
        _sectorService = sectorService;
        _unitOfWork = unitOfWork;
    }
    
    [HttpPost]
    public async Task<IActionResult> InsertStock(string symbol)
    {
        var (stockDto, message) = await _bovespa.GetStock(symbol);
        
        if (stockDto == null) 
            return BadRequest(message);
        
        var sector = await _sectorService.GetOrCreateSectorAsync(stockDto.Name);
        var stock =await _stockService.CreateStock(stockDto, sector);

        await _unitOfWork.SaveChangesAsync();
        return Ok(stock);

    }
    
    [HttpGet]
    public async Task<IActionResult> GetStocks()
    {
        var stocks = await _stockService.GetAllAsync();
        return Ok(stocks);
    }
}