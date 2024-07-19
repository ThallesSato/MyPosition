namespace Infra.Interfaces;

public interface IUnitOfWork
{
    Task SaveChangesAsync();
}