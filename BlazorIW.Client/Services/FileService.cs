using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;

namespace BlazorIW.Client.Services;

public record WebRootFileInfo(string Path, string Owner, string Permissions);

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
}
