using Application.Interfaces;
using Domain.Models;
using Infra.Interfaces;

namespace Application.Services;

public class SectorService : BaseService<Sector>, ISectorService
{
    private readonly ISectorRepository _repository;
    public SectorService(ISectorRepository repository, ISectorRepository repository1) : base(repository)
    {
        _repository = repository1;
    }

    public async Task<Sector> GetOrCreateSectorAsync(string name)
    {
        var sector = await _repository.GetSectorByNameAsync(name);

        if (sector == null)
        {
            sector = new Sector
            {
                Name = name
            };
            await _repository.CreateAsync(sector);
        }
        return sector;
    }
}