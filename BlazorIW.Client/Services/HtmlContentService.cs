using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;

namespace BlazorIW.Client.Services;

public class HtmlContentService(HttpClient httpClient, NavigationManager navigationManager)
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly NavigationManager _navigationManager = navigationManager;

    public async Task<HashSet<string>> GetTitlesAsync()
    {
        if (_httpClient.BaseAddress is null)
        {
            _httpClient.BaseAddress = new Uri(_navigationManager.BaseUri);
        }

        try
        {
            var titles = await _httpClient.GetFromJsonAsync<List<string>>("api/html-content-titles");
            return titles is null ? new HashSet<string>() : new HashSet<string>(titles);
        }
        catch
        {
            return new HashSet<string>();
        }
    }
}
