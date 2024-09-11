using Api.utils;
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
    private readonly IPositionHistoryService _positionHistoryService;

    public TesteController(IBovespa bovespa, IStockService stockService, ISectorService sectorService, IPositionService positionService, IWalletService walletService, ITransactionHistoryService transactionService, IUnitOfWork unitOfWork, IStockHistoryService stockHistoryService, IBacen bacen, IPositionHistoryService positionHistoryService)
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
        _positionHistoryService = positionHistoryService;
    }

    //TODO by sectors (daily, monthly, annually)
    
    //TODO IPORTIFOLIO
    
    [HttpGet]
    public async Task<IActionResult> Get(int id)
    {
        try
        {
            await _stockService.UpdateAllStocksAsync();
            
            var wallet = await _walletService.GetByIdOrDefaultAsync(id);
            if (wallet == null)
                return NotFound();

            decimal totalValue = 0, totalCost = 0;
            var sectorValues = new Dictionary<string, decimal>();
            
            foreach (var position in wallet.Positions)
            {
                totalCost += position.TotalPrice;
                totalValue += position.Amount * position.Stock.LastPrice;
                
                var sector = position.Stock.Setor?.Name;
                if (sector == null) 
                    continue;

                if (sectorValues.TryGetValue(sector, out var currentValue))
                {
                    sectorValues[sector] = currentValue + position.Amount * position.Stock.LastPrice;
                }
                else
                {
                    sectorValues[sector] = position.Amount * position.Stock.LastPrice;
                }
            }
            
            var result = new TotalDto
            {
                TotalCost = totalCost,
                TotalValue = totalValue,
                ResultValue = totalValue - totalCost,
                Wallet = wallet,
                ResultPercentage = totalCost != 0 ? 
                    decimal.Round((totalValue - totalCost) / totalCost * 100, 2) : 0
            };
            
            if (totalValue == 0)
                return Ok(result);
            
            //TODO COLOCAR OS 5 MAIORES E DPS "OUTROS"
            
            result.PercentagePerSectors = sectorValues.OrderByDescending(x => x.Value)
                .Select(x => new SectorPctg(x.Key, decimal.Round(x.Value / totalValue * 100, 2))).ToList();
            return Ok(result);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(e.Message);
        }
    }
[HttpGet("Percentage")]
    public async Task<IActionResult> VariationPercentage(int walletId, DateTime? date, Periodicity periodicity)
    {
        if (date >= DateTime.Now)
            return BadRequest("Date must be in past");
        
        var wallet = await _walletService.GetByIdOrDefaultAsync(walletId);
        if (wallet == null)
            return NotFound();
        
        var result = new SortedDictionary<object, (decimal TotalMesAnterior ,List<(decimal AcaoMesAnterior,decimal VariacaoAcao)> AcaoMesAnteriorIVariacaoAcao)>();
        
        foreach (var positions in wallet.Positions)
        {
            var positionHistoryList = _positionService.GetPositionHistoriesAfterDateAndLast(positions, date);
            if (positionHistoryList.Count == 0)
                continue;
            
            var stockHistoryList =
                await _stockHistoryService.GetStockHistoryList(positions.Stock,
                    date ?? positionHistoryList.First().Date);
            if (stockHistoryList.Count == 0)
                continue;
            
            var stockOld = positionHistoryList.First().TotalPrice/positionHistoryList.First().Amount;

            stockHistoryList = Utils.PeriodicityListAndFiltered(periodicity, stockHistoryList, date);

            var qnt = 0;
            foreach (var stock in stockHistoryList)
            {
                var positionHistory = positionHistoryList.FirstOrDefault(x => x.Date.Date <= stock.Date.Date);
                if (positionHistory != null)
                {
                    qnt = positionHistory.Amount;
                    positionHistoryList.Remove(positionHistory);
                }
                
                if (qnt == 0 && date == null)
                    continue;

                var key = Utils.GetKey(stock.Date, periodicity);

                if (result.TryGetValue(key, out var value))
                {
                    value.Item2.Add((stockOld * qnt, (stock.Close - stockOld) / stockOld));
                    result[key] = (value.Item1 += stockOld * qnt, value.Item2);
                }
                else
                {
                    result[key] = (stockOld * qnt, [(stockOld * qnt, (stock.Close - stockOld) / stockOld)]);
                }

                stockOld = stock.Close;
            }
        }
        foreach (var (data, (total, acoes)) in result)
        {
            foreach (var (acaoMesAnterior, variacao) in acoes)
            {
                //TODO AAAAAAAAAAAAA
            }
        }
        return Ok(result);
    }
}