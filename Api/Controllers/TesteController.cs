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


}