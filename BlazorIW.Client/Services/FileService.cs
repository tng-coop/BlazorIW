using System.Net.Http.Json;

namespace BlazorIW.Client.Services;

public class FileService(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<IEnumerable<string>> GetFilesAsync()
    {
        return await _httpClient.GetFromJsonAsync<IEnumerable<string>>("api/files")
            ?? Enumerable.Empty<string>();
    }
}
