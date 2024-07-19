namespace Infra.Interfaces;

public interface IBaseRepository <TEntity> where TEntity : class
{
    Task<TEntity?> GetByIdOrDefaultAsync(int id);
    Task<List<TEntity>> GetAllAsync();
    void Update(TEntity entity);
    Task<TEntity> CreateAsync(TEntity entity);
    bool Delete(TEntity entity);
}