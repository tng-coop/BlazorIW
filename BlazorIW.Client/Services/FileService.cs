using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;

namespace BlazorIW.Client.Services;

public record WebRootFileInfo(string Path, string Attributes);

public class FileService(HttpClient httpClient, NavigationManager navigationManager)
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly NavigationManager _navigationManager = navigationManager;

    public async Task<IEnumerable<WebRootFileInfo>> GetFilesAsync()
    {
        if (_httpClient.BaseAddress is null)
        {
            _httpClient.BaseAddress = new Uri(_navigationManager.BaseUri);
        }

        return await _httpClient.GetFromJsonAsync<IEnumerable<WebRootFileInfo>>("api/files")
            ?? Enumerable.Empty<WebRootFileInfo>();
    }

    public async Task<bool> IsFileAccessibleAsync(string path)
    {
        if (_httpClient.BaseAddress is null)
        {
            _httpClient.BaseAddress = new Uri(_navigationManager.BaseUri);
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Head, path);
            using var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
