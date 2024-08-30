namespace Infra.ExternalApi.Interfaces;

public interface IBacen
{
    Task<List<(DateTime date, decimal interest)>?> GetInterestsSinceDateAsync(DateTime date);
}