using System.Globalization;
using Application.Dtos.Output;
using Application.Interfaces;
using Infra.Dtos.Internal;
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
    private readonly IPositionHistoryService _positionHistoryService;
    private readonly IBacen _bacen;

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

    [HttpDelete("DeleteTransaction")]
    public async Task<IActionResult> DeleteTransaction(int id)
    {
        try
        {
            var transaction = await _transactionService.GetByIdOrDefaultAsync(id);
            if (transaction == null)
                return NotFound();
            
            var position =
                await _positionService.GetPositionByWalletAndStockOrDefaultAsync(transaction.WalletId, transaction.StockId);
            if (position == null)
                return BadRequest("You dont have position for this stock.");
            
            position.Amount -= transaction.Amount;
            position.TotalPrice -= transaction.EquityEffect;

            transaction.EquityEffect = -transaction.EquityEffect;
            transaction.Amount = -transaction.Amount;
            
            _transactionService.Delete(transaction);
            _positionService.Put(position);
            
            await _positionHistoryService.UpdateOrCreatePositionHistory(transaction, position);
            await _positionHistoryService.UpdateAllPositionHistory(transaction, position);
            
            await _unitOfWork.SaveChangesAsync();
            return Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(e.Message);
        }
    }
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

            foreach (var position in wallet.Positions)
            {
                totalCost += position.TotalPrice;
                totalValue += position.Amount * position.Stock.LastPrice;
            }

            var result = new TotalDto()
            {
                TotalCost = totalCost,
                TotalValue = totalValue,
                ResultValue = totalValue - totalCost,
                Wallet = wallet
            };

            if (totalValue != 0)
                result.ResultPercentage = decimal.Round((totalValue - totalCost) / totalCost * 100, 2);

            var sectors = wallet.Positions
                .Select(position => position.Stock.Setor?.Name)
                .Distinct()
                .ToList();

            foreach (var sector in sectors)
            {
                if (sector == null)
                    continue;
                
                var totalValueBySector = wallet.Positions
                    .Where(position => position.Stock.Setor?.Name == sector)
                    .Sum(position => position.Amount * position.Stock.LastPrice);
                
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

    [HttpGet("cdi")]
    public async Task<IActionResult> TesteCdi(int walletId)
    {
        try
        {
            var wallet = await _walletService.GetByIdOrDefaultAsync(walletId);
            if (wallet == null)
                return NotFound("Wallet not found");

            var positions = await _transactionService.GetTotalAmountByDateAsync(walletId);
            if (positions == null || positions.Count <= 0)
                return NotFound("Position not found");

            var interestsSinceDate = await _bacen.GetInterestsSinceDate(positions.First().Date);
            if (interestsSinceDate == null || interestsSinceDate.Count == 0)
                return BadRequest("Try again later");
            
            double total = 0;
            double tds = 0;
            var lucrosAcumulados = new Dictionary<DateTime, decimal>();

            foreach (var interest in interestsSinceDate)
            {
                var position = positions.FirstOrDefault(x => x.Date.Day <= interest.date.Day);
                if (position != null)
                {
                    total += Convert.ToDouble(position.Amount);
                    tds += Convert.ToDouble(position.Amount);
                    positions.Remove(position);
                }

                var interestValue = Convert.ToDouble(interest.interest, CultureInfo.InvariantCulture);
                total *= (1 + interestValue / 100);

                lucrosAcumulados.Add(interest.date.Date, decimal.Round(Convert.ToDecimal(total - tds), 2));
            }

            return Ok(lucrosAcumulados);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(e.Message);
        }
    }

    [HttpGet("newcdi")]
    public async Task<IActionResult> NewTesteCdi(int walletId, DateTime? date)
    {
        try
        {
            var wallet = await _walletService.GetByIdOrDefaultAsync(walletId);
            if (wallet == null)
                return NotFound("Wallet not found");
            if (date >= DateTime.Now)
                return BadRequest("Date must be in past");

            var totalAmountList = new Dictionary<DateTime, decimal>();

            foreach (var positions in wallet.Positions)
            {
                var historyList = positions.GetPositionHistoriesAfterDateOrLast(date);
                if (historyList.Count == 0)
                    continue;

                foreach (var history in historyList)
                {
                    if (totalAmountList.ContainsKey(history.Date.Date))
                    {
                        totalAmountList[history.Date.Date] += history.Amount;
                    }
                    else
                    {
                        totalAmountList.Add(history.Date.Date, history.TotalPrice);
                    }
                }
            }

            var interestsSinceDate = await _bacen.GetInterestsSinceDate(date ?? totalAmountList.MinBy(x=>x.Key).Key);
            if (interestsSinceDate == null || interestsSinceDate.Count == 0)
                return BadRequest("Try again later");
            
            double total = 0;
            double tds = 0;
            var lucrosAcumulados = new Dictionary<DateTime, decimal>();

            foreach (var interest in interestsSinceDate)
            {
                var position = totalAmountList.FirstOrDefault(x => x.Key <= interest.date.Date);
                if (position.Value != 0)
                {
                    total += Convert.ToDouble(position.Value);
                    tds += Convert.ToDouble(position.Value);
                    totalAmountList.Remove(position.Key);
                }

                var interestValue = Convert.ToDouble(interest.interest, CultureInfo.InvariantCulture);
                total *= (1 + interestValue / 100);

                lucrosAcumulados.Add(interest.date.Date, decimal.Round(Convert.ToDecimal(total - tds), 2));
            }

            return Ok(lucrosAcumulados);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(e.Message);
        }
    }

    [HttpGet("variation")]
    public async Task<IActionResult> TesteVariation(int walletId)
    {
        var wallet = await _walletService.GetByIdOrDefaultAsync(walletId);
        if (wallet == null)
            return NotFound();
        
        var transactionsMatrix = await _transactionService.GetAllByWalletIdAsync(walletId);
        if (transactionsMatrix == null || transactionsMatrix.Count == 0)
            return BadRequest("You dont have any transactions");
        
        var result = new Dictionary<DateTime, decimal>();
        foreach (var transactionList in transactionsMatrix)
        {
            var positions = transactionList
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
            var history = await _stockHistoryService.GetStockHistoryList(first.Stock, first.Date);
            
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
                    result[stock.Date.Date] += (stock.Close * qnt)-cost;
                else
                    result.Add(stock.Date.Date, (stock.Close * qnt)-cost);
            }
        }
        return Ok(result.OrderBy(x=>x.Key).ToDictionary());
    }
    [HttpGet("newvariation")]
    public async Task<IActionResult> NewTesteVariation(int walletId, DateTime? date)
    {
        var wallet = await _walletService.GetByIdOrDefaultAsync(walletId);
        if (wallet == null)
            return NotFound();
        if (date >= DateTime.Now)
            return BadRequest("Date must be in past");
        
        var result = new Dictionary<DateTime, decimal>();
        
        foreach (var positions in wallet.Positions)
        {
            var positionHistoryList = positions.GetPositionHistoriesAfterDateOrLast(date);
            if (positionHistoryList.Count == 0)
                continue;
            
            var stockHistoryList =
                await _stockHistoryService.GetStockHistoryList(positions.Stock,
                    date ?? positionHistoryList.First().Date);
            if (stockHistoryList.Count == 0)
                continue;
            
            var qnt = 0;
            decimal cost = 0;
            foreach (var stock in stockHistoryList)
            {
                if (stock.Date.Date < date?.Date)
                    continue;
                
                var t = positionHistoryList.FirstOrDefault(x => x.Date.Date <= stock.Date.Date);
                if (t != null)
                {
                    qnt = t.Amount;
                    cost = t.TotalPrice;
                }
                
                if (qnt == 0)
                    continue;

                if (result.ContainsKey(stock.Date.Date))
                    result[stock.Date.Date] += (stock.Close * qnt)-cost;
                else
                    result.Add(stock.Date.Date, (stock.Close * qnt)-cost);
            }
        }
        return Ok(result.OrderBy(x=>x.Key).ToDictionary());
    }
}