using System.Net.Http.Json;
using Domain.Models;
using Infra.ExternalApi.Dtos;
using Infra.ExternalApi.Interfaces;

namespace Infra.ExternalApi.Services;

public class Bovespa : IBovespa
{
    private readonly HttpClient _httpClient;

    public Bovespa()
    {
        _httpClient = new HttpClient();
    }

    public async Task<(StockApiDto? stock, string? message)> GetStock(string symbol)
    {
        StockApiDto? stock = null;
        string? message = "service offline";
        
        HttpResponseMessage response = await _httpClient.GetAsync($"https://mfinance.com.br/api/v1/stocks/{symbol}");
        
        if (!response.IsSuccessStatusCode)
        {
            message = response.ReasonPhrase;
        }
        else
        {
            stock = await response.Content.ReadFromJsonAsync<StockApiDto>();
        }
        
        return(stock,message);
    }
}