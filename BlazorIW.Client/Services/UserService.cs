using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;

namespace BlazorIW.Client.Services;

public class UserService(HttpClient httpClient, NavigationManager navigationManager)
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly NavigationManager _navigationManager = navigationManager;

    private void EnsureBaseAddress()
    {
        if (_httpClient.BaseAddress is null)
        {
            _httpClient.BaseAddress = new Uri(_navigationManager.BaseUri);
        }
    }

    public async Task<List<UserInfo>> GetUsersAsync()
    {
        EnsureBaseAddress();
        var users = await _httpClient.GetFromJsonAsync<List<UserInfo>>("api/users");
        return users ?? new List<UserInfo>();
    }

    public async Task CreateUserAsync(string email, string password, string role)
    {
        EnsureBaseAddress();
        var dto = new CreateUserDto(email, password, role);
        var response = await _httpClient.PostAsJsonAsync("api/users", dto);
        response.EnsureSuccessStatusCode();
    }

    public async Task SetRoleAsync(string id, string role)
    {
        EnsureBaseAddress();
        var dto = new SetRoleDto(role);
        var response = await _httpClient.PostAsJsonAsync($"api/users/{id}/role", dto);
        response.EnsureSuccessStatusCode();
    }

    public async Task SetDisabledAsync(string id, bool disabled)
    {
        EnsureBaseAddress();
        var dto = new SetDisabledDto(disabled);
        var response = await _httpClient.PostAsJsonAsync($"api/users/{id}/disable", dto);
        response.EnsureSuccessStatusCode();
    }
}

public record UserInfo(string Id, string Email, List<string> Roles, bool IsDisabled);
public record CreateUserDto(string Email, string Password, string Role);
public record SetRoleDto(string Role);
public record SetDisabledDto(bool Disabled);
