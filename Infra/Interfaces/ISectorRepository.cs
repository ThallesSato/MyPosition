using Domain.Models;

namespace Infra.Interfaces;

public interface ISectorRepository : IBaseRepository<Sector>
{
    Task<Sector?> GetSectorByNameAsync(string name);
}