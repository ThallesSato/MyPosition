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

    //TODO MONTHLY
    //TODO annually
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

            result.PercentagePerSectors = sectorValues.OrderByDescending(x => x.Value)
                .ToDictionary(x => x.Key, x => decimal.Round(x.Value / totalValue * 100));
            return Ok(result);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(e.Message);
        }
    }

    
    [HttpGet("Cdi/Daily/Absolute")]
    public async Task<IActionResult> CdiAbsolute(int walletId, DateTime? date)
    {
        try
        {
            var wallet = await _walletService.GetByIdOrDefaultAsync(walletId);
            if (wallet == null)
                return NotFound("Wallet not found");
            if (date >= DateTime.Now)
                return BadRequest("Date must be in past");

            var totalAmountList = _positionService.GetTotalAmountByDate(wallet, date);

            var interestsSinceDate = await _bacen.GetInterestsSinceDateAsync(date ?? totalAmountList.MinBy(x=>x.Key).Key);
            if (interestsSinceDate == null || interestsSinceDate.Count == 0)
                return BadRequest("Bacen service unavailable. Try again later");
            
            decimal total = 0;
            decimal tds = 0;
            var cumulativeProfit  = new Dictionary<DateTime, decimal>();

            foreach (var interest in interestsSinceDate)
            {
                var position = totalAmountList.FirstOrDefault(x => x.Key <= interest.date.Date);
                if (position.Value != 0)
                {
                    total += position.Value;
                    tds += position.Value;
                    totalAmountList.Remove(position.Key);
                }

                total *= 1 + interest.interest / 100;
                cumulativeProfit [interest.date.Date] = decimal.Round(total - tds, 2);
            }

            return Ok(cumulativeProfit);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(e.Message);
        }
    }
}