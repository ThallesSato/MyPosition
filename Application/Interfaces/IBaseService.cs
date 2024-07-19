namespace Application.Interfaces;

public interface IBaseService <TEntity> where TEntity : class
{
    Task<List<TEntity>> GetAllAsync();
    Task<TEntity?> GetByIdOrDefaultAsync(int id);
    Task<TEntity> CreateAsync(TEntity entity);
    void Put(TEntity entity);
    bool Delete(TEntity entity);
}