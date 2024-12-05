using Microsoft.Extensions.Hosting;

namespace Application.TESte;

public class Testeback: BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Console.WriteLine("Hello");
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Wait for 1 minute
        }
    }
}