using System.Text.Json;

namespace BlazorIW.Client.Services;

public class WordPressService(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<List<JsonElement>> FetchPostsAsync(string baseUrl)
    {
        var posts = new List<JsonElement>();
        int page = 1;
        while (true)
        {
            var url = $"{baseUrl.TrimEnd('/')}/posts?per_page=100&page={page}";
            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind != JsonValueKind.Array || doc.RootElement.GetArrayLength() == 0)
                {
                    break;
                }
                foreach (var item in doc.RootElement.EnumerateArray())
                {
                    posts.Add(item.Clone());
                }
            }
            catch
            {
                break;
            }
            page++;
        }
        return posts;
    }
}
