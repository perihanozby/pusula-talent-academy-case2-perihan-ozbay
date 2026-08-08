using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.JSInterop;

namespace StudentAutomation.Web.Services;

public class AuthService
{
    private readonly HttpClient _http;
    private readonly IJSRuntime _js;
    private string? _token;
    public string? Role { get; private set; }

    public AuthService(HttpClient http, IJSRuntime js)
    {
        _http = http;
        _js = js;
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        var res = await _http.PostAsJsonAsync("/api/auth/login", new { email, password });
        if (!res.IsSuccessStatusCode)
            return false;

        var payload = await res.Content.ReadFromJsonAsync<LoginResponse>();
        _token = payload!.token;

        // token'ı localStorage'a yaz
        await _js.InvokeVoidAsync("localStorage.setItem", "token", _token);

        // role'ü decode et
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(_token);
        Role = jwt.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        return true;
    }

    public async Task LogoutAsync()
    {
        _token = null;
        Role = null;

        await _js.InvokeVoidAsync("localStorage.removeItem", "token");
        _http.DefaultRequestHeaders.Authorization = null;
    }

    public async Task TryRestoreAsync()
    {
        var t = await _js.InvokeAsync<string?>("localStorage.getItem", "token");
        if (!string.IsNullOrWhiteSpace(t))
        {
            _token = t;

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(_token);
            Role = jwt.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                _token
            );
        }
    }

    private class LoginResponse
    {
        public string token { get; set; } = "";
    }
}
