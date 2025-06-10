using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace BlazorIW.Client.Services;

public class HtmlContentService(HttpClient httpClient, NavigationManager navigationManager, ILogger<HtmlContentService> logger)
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly NavigationManager _navigationManager = navigationManager;
    private readonly ILogger<HtmlContentService> _logger = logger;

    public async Task<HashSet<string>> GetTitlesAsync()
    {
        if (_httpClient.BaseAddress is null)
        {
            _httpClient.BaseAddress = new Uri(_navigationManager.BaseUri);
        }

        _logger.LogInformation("Requesting existing titles from {BaseAddress}", _httpClient.BaseAddress);

        try
        {
            var titles = await _httpClient.GetFromJsonAsync<List<string>>("api/html-content-titles");
            return titles is null
                ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                : new HashSet<string>(titles.Select(t => t.Trim()), StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            _logger.LogError("Failed to fetch titles");
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    public async Task<int> ImportPostsAsync(IEnumerable<ImportPostDto> posts)
    {
        if (_httpClient.BaseAddress is null)
        {
            _httpClient.BaseAddress = new Uri(_navigationManager.BaseUri);
        }

        var postList = posts.ToList();
        _logger.LogInformation("Posting {Count} posts to {BaseAddress}", postList.Count, _httpClient.BaseAddress);
        var response = await _httpClient.PostAsJsonAsync("api/import-html-content", postList);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ImportResult>();
        var added = result?.Added ?? 0;
        _logger.LogInformation("Import returned {Added} added posts", added);
        return added;
    }

    public async Task<List<HtmlContentDto>> GetAllAsync()
    {
        if (_httpClient.BaseAddress is null)
        {
            _httpClient.BaseAddress = new Uri(_navigationManager.BaseUri);
        }

        _logger.LogInformation("Requesting all html content from {BaseAddress}", _httpClient.BaseAddress);

        try
        {
            var items = await _httpClient.GetFromJsonAsync<List<HtmlContentDto>>("api/html-contents");
            return items ?? new List<HtmlContentDto>();
        }
        catch
        {
            _logger.LogError("Failed to fetch html contents");
            return new List<HtmlContentDto>();
        }
    }

    public async Task SetStatusAsync(Guid id, int revision, HtmlContentStatus status)
    {
        if (_httpClient.BaseAddress is null)
        {
            _httpClient.BaseAddress = new Uri(_navigationManager.BaseUri);
        }

        var dto = new UpdateStatusDto(id, revision, status.ToString());
        var response = await _httpClient.PostAsJsonAsync("api/html-content-status", dto);
        response.EnsureSuccessStatusCode();
    }
}

public record ImportPostDto(string Date, string Title, string Excerpt, string Content);
public record ImportResult(int Added);
public record HtmlContentDto(Guid Id, int Revision, DateTime Date, string Title, string Excerpt, string Content, bool IsReviewRequested, bool IsPublished);
public record UpdateStatusDto(Guid Id, int Revision, string Status);

public enum HtmlContentStatus
{
    Draft,
    Review,
    Published
}
