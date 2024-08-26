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
            await _stockService.UpdateAllStocksAsync();
            
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
                Wallet = wallets
            };

            if (totalValue != 0)
                result.ResultPercentage = decimal.Round((totalValue - totalCost) / totalCost * 100, 2);

            var sectors = wallets.Positions
                .Select(position => position.Stock?.Setor?.Name)
                .Distinct()
                .ToList();

            foreach (var sector in sectors)
            {
                if (sector == null)
                    continue;
                
                var totalValueBySector = wallets.Positions
                    .Where(position => position.Stock.Setor?.Name == sector)
                    .Sum(position => position.Amount * position.Stock?.LastPrice) ?? 0;
                
                if (totalValue != 0 && totalValueBySector != 0)
                    result.PercentagePerSectors.Add(sector, decimal.Round(totalValueBySector / totalValue * 100));
            }
            result.PercentagePerSectors = result.PercentagePerSectors.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
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
    public async Task<IActionResult> TesteCdi(int walletId)
    {
        var wallet = await _walletService.GetByIdOrDefaultAsync(walletId);
        if (wallet == null)
            return NotFound();

        var positions = await _transactionService.GetTotalAmountByDateAsync(walletId);
        
        
        double total = 0;
        double tds = 0;
        var lucrosAcumulados = new Dictionary<DateTime, decimal>();
        if (positions != null && positions.Count > 0)
        {
            var interestsSinceDate = await _bacen.GetInterestsSinceDate(positions.First().Date);
            
            if (interestsSinceDate == null || interestsSinceDate.Count == 0)
                return BadRequest("Try again later");
            
            foreach (var interest in interestsSinceDate)
            {
                
                var position = positions.FirstOrDefault(x => x.Date.Day <= interest.date.Day);
                if (position != null)
                {
                    total += Convert.ToDouble(position.Amount);
                    tds += Convert.ToDouble(position.Amount);
                    positions.Remove(position);
                }

                var teste = Convert.ToDouble(interest.interest, CultureInfo.InvariantCulture);
                total *= (1 + teste / 100);

                lucrosAcumulados.Add(interest.date.Date,decimal.Round(Convert.ToDecimal(total - tds),2));
            }
            
        }

        return Ok(lucrosAcumulados);
    }

    [HttpGet("variation")]
    public async Task<IActionResult> TesteVariation(int walletId)
    {
        var wallet = await _walletService.GetByIdOrDefaultAsync(walletId);
        if (wallet == null)
            return NotFound();
        var result = new Dictionary<DateTime, decimal>();
        var teste = await _transactionService.GetAllByWalletIdAsync(walletId);
        if (teste == null || teste.Count == 0)
            return BadRequest("You dont have any transactions");
        foreach (var temp in teste)
        {
            var positions = temp
                .GroupBy(x => x.Date)
                .OrderBy(x=>x.Key)
                .Select(g=>new
                {
                    Date = g.Key,
                    Amount = g.Sum(x => x.Amount),
                    Cost = g.Sum(x=>x.Price * x.Amount),
                    g.First().Stock
                })
                .ToList();
            var first = positions.First();
            var history = await _bovespa.GetStockHistory(first.Stock, first.Date);
            if (history == null || history.Count == 0)
                continue;
            var qnt = 0;
            decimal cost = 0;
            foreach (var stock in history)
            {
                var t = positions.FirstOrDefault(x => x.Date.Date == stock.Date.Date);
                if (t != null)
                {
                    qnt += t.Amount;
                    cost += t.Cost;
                }
                
                if (qnt == 0)
                    continue;

                if (result.ContainsKey(stock.Date.Date))
                {
                    result[stock.Date.Date] += (stock.Close * qnt)-cost;
                }
                else
                {
                    result.Add(stock.Date.Date, (stock.Close * qnt)-cost);
                }
            }
        }
        result = result.OrderBy(x=>x.Key).ToDictionary();
        return Ok(result);
    }
}