namespace Infra.ExternalApi.Interfaces;

public interface IBacen
{
    Task<List<(DateTime date, Double interest)>?> GetInterestsSinceDate(DateTime date);
}