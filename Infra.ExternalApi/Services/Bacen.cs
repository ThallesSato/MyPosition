using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Infra.ExternalApi.Dtos;
using Infra.ExternalApi.Interfaces;

namespace Infra.ExternalApi.Services;

public class Bacen : IBacen
{
    
    private readonly HttpClient _httpClient;

    public Bacen()
    {
        _httpClient = new HttpClient();
    }

    public async Task<List<(DateTime date, Double interest)>?> GetInterestsSinceDate(DateTime date)
    {
        var teste =
            "https://api.bcb.gov.br/dados/serie/bcdata.sgs.12/dados?formato=json&dataInicial=" +
            date.ToString("dd/MM/yyyy");
        var response = await _httpClient.GetAsync(teste);
        if (!response.IsSuccessStatusCode)
            return null;
        
        var interestList = await response.Content.ReadFromJsonAsync<List<Interest>>();
        return interestList?
            .Select(x => (
                DateTime.ParseExact(x.Data, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                Convert.ToDouble(x.Valor, CultureInfo.InvariantCulture)))
            .ToList();
    }

}