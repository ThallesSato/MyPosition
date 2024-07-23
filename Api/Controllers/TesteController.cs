using Application.Interfaces;
using Domain.Models;
using Infra.ExternalApi.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TesteController : ControllerBase
{
    private readonly IBovespa _bovespa;
    private readonly IStockService _stockService;
    private readonly ISectorService _sectorService;
    private readonly IBaseService<Positions> _positionService;
    private readonly IWalletService _walletService;

    public TesteController(IBovespa bovespa, IStockService stockService, ISectorService sectorService, IBaseService<Positions> positionService, IWalletService walletService)
    {
        _bovespa = bovespa;
        _stockService = stockService;
        _sectorService = sectorService;
        _positionService = positionService;
        _walletService = walletService;
    }

    [HttpGet]
    public async Task<IActionResult> Get(int id)
    {
        try
        {
            var wallets = await _walletService.GetByIdOrDefaultAsync(id);
            if (wallets == null)
                return NotFound();

            decimal totalValue = 0, totalCost = 0;
            
            foreach (var position in wallets.Positions)
            {
                totalCost += position.Amount * position.Price;
                totalValue += position.Amount * position.Stock.LastPrice;
            }
            Console.WriteLine("totalCost: " + totalCost);
            Console.WriteLine("totalValue: " + totalValue);
            Console.WriteLine("Lucro: " + (totalValue-totalCost));
            
            
            var sectors = wallets.Positions
                .Select(position => position.Stock?.Setor?.Name)
                .Distinct()
                .ToList();

            Console.WriteLine("% por setores");
            
            foreach (var sector in sectors)
            {
                var totalValueBySector = wallets.Positions
                    .Where(position => position.Stock?.Setor?.Name == sector)
                    .Sum(position => position.Amount * position.Stock?.LastPrice);

                Console.WriteLine(sector + ": " + (totalValueBySector/totalValue)*100);
            }
            
            return Ok(wallets);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(e.Message);
        }
    }
}