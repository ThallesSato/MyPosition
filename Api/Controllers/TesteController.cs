using System.Collections.Specialized;
using System.Security.Claims;
using Api.Facades;
using Application.Dtos.Output;
using Application.Interfaces;
using Application.utils;
using Domain.Models;
using Infra.Dtos.Internal;
using Infra.ExternalApi.Interfaces;
using Infra.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Dtos.Output;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
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
    private readonly VariationFacade _variationFacade;
    private readonly IUserService _userService;

    public TesteController(IBovespa bovespa, IStockService stockService, ISectorService sectorService,
        IPositionService positionService, IWalletService walletService, ITransactionHistoryService transactionService,
        IUnitOfWork unitOfWork, IStockHistoryService stockHistoryService, IBacen bacen,
        IPositionHistoryService positionHistoryService, IUserService userService)
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
        _userService = userService;
        _variationFacade = new VariationFacade(positionService, stockHistoryService);
    }


    [HttpGet("Chart/Wallet/Sectors")]
    public async Task<IActionResult> ChartWalletSectors(int walletId, DateTime? date, GraphType graphType,
        Periodicity periodicity)
    {
        try
        {
            if (date >= DateTime.Now)
                return BadRequest("Date must be in past");
            var claim = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            if (claim == null)
                return Unauthorized();
            var user = await _userService.GetUserByEmailAsync(claim);
            if (user == null)
                return Unauthorized();
            var wallet = await _walletService.GetByIdOrDefaultAsync(walletId);
            if (wallet == null)
                return NotFound();
            if (wallet.User.Id != user.Id)
                return BadRequest("This wallet is not yours");
            date ??= _transactionService.GetFirstByWalletIdAsync(walletId)?.Date.Date;
            
            var positionsBySector = new Dictionary<string, List<Positions>>();

            foreach (var position in wallet.Positions)
            {
                if (positionsBySector.TryGetValue(position.Stock.Sector.Name, out List<Positions>? value))
                    value.Add(position);
                else
                    positionsBySector.Add(position.Stock.Sector.Name, new List<Positions> { position });
            }
            var response = new List<ChartDto>();
            foreach (var (key, value) in positionsBySector)
            {
                SortedDictionary<string, decimal> variation;
                switch (graphType)
                {
                    case GraphType.Absolute:
                        variation = await _variationFacade.VariationAbsolute(date, periodicity, value);
                        break;
                    case GraphType.AbsoluteAccumulated:
                        variation = await _variationFacade.VariationAbsoluteAccumulated(date, periodicity, value);
                        break;
                    case GraphType.Percentage:
                        variation = await _variationFacade.VariationPercentage(date, periodicity, value);
                        break;
                    case GraphType.PercentageAccumulated:
                    default:
                        variation = await _variationFacade.VariationPercentageAccumulated(date, periodicity, value);
                        break;
                }

                response.Add(new ChartDto
                {
                    Name = key,
                    Values = variation.Values.ToList()
                });
            }
            
            return Ok(response);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}