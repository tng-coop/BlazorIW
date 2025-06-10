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
            return titles is null
                ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                : new HashSet<string>(titles.Select(t => t.Trim()), StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    public async Task<int> ImportPostsAsync(IEnumerable<ImportPostDto> posts)
    {
        if (_httpClient.BaseAddress is null)
        {
            _httpClient.BaseAddress = new Uri(_navigationManager.BaseUri);
        }

        var response = await _httpClient.PostAsJsonAsync("api/import-html-content", posts);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ImportResult>();
        return result?.Added ?? 0;
    }
}

public record ImportPostDto(string Date, string Title, string Excerpt, string Content);
public record ImportResult(int Added);
