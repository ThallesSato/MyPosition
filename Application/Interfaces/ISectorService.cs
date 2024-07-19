using Domain.Models;

namespace Application.Interfaces;

public interface ISectorService : IBaseService<Sector>
{
    Task<Sector> GetOrCreateSectorAsync(string name);
}