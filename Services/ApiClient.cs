using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace PencaMaui.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    private const string BaseUrl = "https://tupenca-api-production-ed1d.up.railway.app";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiClient()
    {
        _http = new HttpClient { BaseAddress = new Uri(BaseUrl) };
        _http.DefaultRequestHeaders.Add("X-Sitio", "penca.com");
    }

    public void SetToken(string token)
    {
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    public void ClearToken()
    {
        _http.DefaultRequestHeaders.Authorization = null;
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        try
        {
            var response = await _http.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ApiClient] GET {endpoint} error: {ex.Message}");
            return default;
        }
    }

    public async Task<T?> PostAsync<T>(string endpoint, object body)
    {
        try
        {
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ApiClient] POST {endpoint} error: {ex.Message}");
            return default;
        }
    }

    public async Task<bool> PostAsync(string endpoint, object body)
    {
        try
        {
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync(endpoint, content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ApiClient] POST {endpoint} error: {ex.Message}");
            return false;
        }
    }

    public async Task<T?> PutAsync<T>(string endpoint, object body)
    {
        try
        {
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PutAsync(endpoint, content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ApiClient] PUT {endpoint} error: {ex.Message}");
            return default;
        }
    }
}
