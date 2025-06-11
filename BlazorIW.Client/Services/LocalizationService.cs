using System.Globalization;
using Microsoft.JSInterop;
using System.Net.Http;
using System.Net.Http.Json;

namespace BlazorIW.Client.Services;

public class LocalizationService
{
    private readonly IJSRuntime _js;
    private readonly BrowserStorageService _storage;
    private readonly HttpClient _http;
    private bool _loaded;
    public event Action? OnChange;

    public LocalizationService(IJSRuntime js, BrowserStorageService storage, HttpClient http)
    {
        _js = js;
        _storage = storage;
        _http = http;
        CurrentCulture = CultureInfo.CurrentCulture;
    }

    public CultureInfo CurrentCulture { get; private set; }

    public async Task LoadCultureAsync()
    {
        if (_loaded)
        {
            return;
        }

        var culture = await _storage.GetLocalStorageAsync("blazorCulture");
        if (string.IsNullOrEmpty(culture))
        {
            try
            {
                var langs = await _js.InvokeAsync<string[]>("localization.getBrowserLanguages");
                if (langs != null && langs.Any(l => l.StartsWith("ja", StringComparison.OrdinalIgnoreCase)))
                {
                    culture = "ja";
                }
                else
                {
                    culture = "en";
                }
            }
            catch
            {
                culture = "en";
            }
        }

        SetThreadCulture(culture);
        await _http.GetAsync($"api/set-culture?culture={Uri.EscapeDataString(culture)}");
        _loaded = true;
        OnChange?.Invoke();
    }

    public async Task SetCultureAsync(string culture)
    {
        await _storage.SetLocalStorageAsync("blazorCulture", culture);
        await _http.GetAsync($"api/set-culture?culture={Uri.EscapeDataString(culture)}");
        SetThreadCulture(culture);
        OnChange?.Invoke();
    }

    private void SetThreadCulture(string culture)
    {
        var cultureInfo = new CultureInfo(culture);
        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
        CurrentCulture = cultureInfo;
    }
}
