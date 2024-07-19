using Application.Interfaces;
using Domain.Models;
using Infra.Interfaces;

namespace Application.Services;

public class BaseService <TEntity> : IBaseService <TEntity> where TEntity : BaseEntity
{
    private readonly IBaseRepository<TEntity> _repository;
    
    public BaseService(IBaseRepository<TEntity> repository)
    {
        _repository = repository;
    }

    public async Task<List<TEntity>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<TEntity?> GetByIdOrDefaultAsync(int id)
    {
        return await _repository.GetByIdOrDefaultAsync(id);
    }
    
    public async Task<TEntity> CreateAsync(TEntity entity)
    {
        return await _repository.CreateAsync(entity);
    }

    public void Put(TEntity entity)
    {
        _repository.Update(entity);
    }

    public bool Delete(TEntity entity)
    {
        return _repository.Delete(entity);
    }
}