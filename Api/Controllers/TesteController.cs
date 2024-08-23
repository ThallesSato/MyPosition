using System.Globalization;
using Application.Dtos.Output;
using Application.Interfaces;
using Infra.ExternalApi.Interfaces;
using Infra.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TesteController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBovespa _bovespa;
    private readonly IStockService _stockService;
    private readonly ISectorService _sectorService;
    private readonly IPositionService _positionService;
    private readonly ITransactionHistoryService _transactionService;
    private readonly IWalletService _walletService;
    private readonly IStockHistoryService _stockHistoryService;
    private readonly IBacen _bacen;

    public TesteController(IBovespa bovespa, IStockService stockService, ISectorService sectorService, IPositionService positionService, IWalletService walletService, ITransactionHistoryService transactionService, IUnitOfWork unitOfWork, IStockHistoryService stockHistoryService, IBacen bacen)
    {
        _bovespa = bovespa;
        _stockService = stockService;
        _sectorService = sectorService;
        _positionService = positionService;
        _walletService = walletService;
        _transactionService = transactionService;
        _unitOfWork = unitOfWork;
        _stockHistoryService = stockHistoryService;
        _bacen = bacen;
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
                totalCost += position.TotalPrice;
                totalValue += position.Amount * position.Stock.LastPrice;
            }

            var result = new TotalDto()
            {
                TotalCost = totalCost,
                TotalValue = totalValue,
                ResultValue = totalValue - totalCost,
            };
            Console.WriteLine("TotalCost: " + totalCost);
            Console.WriteLine("TotalValue: " + totalValue);
            Console.WriteLine("Result Value: " + (totalValue - totalCost));

            if (totalValue != 0)
            {
                Console.WriteLine("Result % : " + decimal.Round((totalValue - totalCost) / totalCost * 100, 2));
                result.ResultPercentage = decimal.Round((totalValue - totalCost) / totalCost * 100, 2);
            }

            var sectors = wallets.Positions
                .Select(position => position.Stock?.Setor?.Name)
                .Distinct()
                .ToList();

            Console.WriteLine("% por setores");

            foreach (var sector in sectors)
            {
                if (sector == null)
                    continue;
                
                var totalValueBySector = wallets.Positions
                    .Where(position => position.Stock.Setor?.Name == sector)
                    .Sum(position => position.Amount * position.Stock?.LastPrice) ?? 0;
                
                if (totalValue != 0 && totalValueBySector != 0){
                    Console.WriteLine(sector + " % : " + decimal.Round(totalValueBySector / totalValue * 100));

                    result.PercentagePerSectors.Add(sector, decimal.Round(totalValueBySector / totalValue * 100));
                }
            }

            return Ok(result);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(e.Message);
        }
    }

    [HttpGet("teste")]
    public async Task<IActionResult> Testeinfra(int id, DateTime data)
    {
        var stock = await _stockService.GetByIdOrDefaultAsync(id);
        if (stock == null)
            return NotFound();
        var result = await _stockHistoryService.GetStockHistoryListOrCreateAllAsync(stock, data);

        return Ok(result);
    }

    [HttpGet("cdi")]
    public async Task<IActionResult> TesteCdi(int id)
    {
        var wallets = await _walletService.GetByIdOrDefaultAsync(id);
        if (wallets == null)
            return NotFound();

        var lista = await _transactionService.GetTotalAmountByDateAsync(id);
        
        
        double total = 0;
        double tds = 0;
        var lucrosAcumulados = new List<decimal>();
        if (lista != null && lista.Count > 0)
        {
            var juros = await _bacen.GetInterestsSinceDate(lista.First().Date);
            
            if (juros == null || juros.Count == 0)
                return BadRequest("Try again later");
            
            foreach (var juro in juros)
            {
                
                var position = lista.FirstOrDefault(x => x.Date.Day <= juro.date.Day);
                if (position != null)
                {
                    total += Convert.ToDouble(position.Amount);
                    tds += Convert.ToDouble(position.Amount);
                    lista.Remove(position);
                }

                var teste = Convert.ToDouble(juro.Item2, CultureInfo.InvariantCulture);
                total *= (1 + teste / 100);

                lucrosAcumulados.Add(decimal.Round(Convert.ToDecimal(total - tds),2));
            }
            
        }

        return Ok(lucrosAcumulados);
    }
}