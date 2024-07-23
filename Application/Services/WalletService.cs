using Application.Interfaces;
using Domain.Models;
using Infra.Interfaces;

namespace Application.Services;

public class WalletService : BaseService<Wallet>, IWalletService
{
    private readonly IWalletRepository _repository;
    public WalletService(IWalletRepository repository) : base(repository)
    {
        _repository = repository;
    }
}