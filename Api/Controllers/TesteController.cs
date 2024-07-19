using Infra.ExternalApi.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TesteController : ControllerBase
{
    private readonly IBovespa _bovespa;
    public TesteController(IBovespa bovespa)
    {
        _bovespa = bovespa;
    }
    
    [HttpGet]
    public async Task<IActionResult> Get(string symbol)
    {
        var (a, b) = await _bovespa.GetStock(symbol);
        
        var c = new
        {
            a, b
        };
        
        return Ok(c);
    }
}