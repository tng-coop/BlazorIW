using Microsoft.JSInterop;

namespace BlazorIW.Client.Services;

public class BrowserStorageService
{
    private readonly IJSRuntime _js;

    public BrowserStorageService(IJSRuntime js)
    {
        _js = js;
    }

    public ValueTask SetLocalStorageAsync(string key, string value) =>
        _js.InvokeVoidAsync("localStorage.setItem", key, value);

    public ValueTask<string?> GetLocalStorageAsync(string key) =>
        _js.InvokeAsync<string?>("localStorage.getItem", key);

    public ValueTask RemoveLocalStorageAsync(string key) =>
        _js.InvokeVoidAsync("localStorage.removeItem", key);

    public ValueTask SetSessionStorageAsync(string key, string value) =>
        _js.InvokeVoidAsync("sessionStorage.setItem", key, value);

    public ValueTask<string?> GetSessionStorageAsync(string key) =>
        _js.InvokeAsync<string?>("sessionStorage.getItem", key);

    public ValueTask RemoveSessionStorageAsync(string key) =>
        _js.InvokeVoidAsync("sessionStorage.removeItem", key);
}
