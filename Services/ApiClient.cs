using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace PencaMaui.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    private const string BaseUrl = "https://tupenca-api-production-ed1d.up.railway.app";
    private const string SitioPrefKey = "sitio_url_propia";
    private const string SitioPorDefecto = "penca.com";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public string SitioActual { get; private set; }

    public ApiClient()
    {
        _http = new HttpClient { BaseAddress = new Uri(BaseUrl) };
        SitioActual = Preferences.Get(SitioPrefKey, SitioPorDefecto);
        _http.DefaultRequestHeaders.Add("X-Sitio", SitioActual);
    }

    public void SetSitio(string urlPropia)
    {
        if (string.IsNullOrWhiteSpace(urlPropia) || urlPropia == SitioActual)
            return;

        SitioActual = urlPropia;
        Preferences.Set(SitioPrefKey, urlPropia);
        _http.DefaultRequestHeaders.Remove("X-Sitio");
        _http.DefaultRequestHeaders.Add("X-Sitio", urlPropia);
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
            // Evita que el dispositivo o algun proxy intermedio devuelva una
            // respuesta cacheada (ej. resultados/puntos de un partido que ya cerro).
            var separador = endpoint.Contains('?') ? "&" : "?";
            var url = $"{endpoint}{separador}_={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true, NoStore = true };

            var response = await _http.SendAsync(request);
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

    // Retries on network errors and 5xx (Railway cold start). Returns status code 0 on exception.
    public async Task<(T? data, int statusCode, string error)> PostDetailedAsync<T>(string endpoint, object body, int maxRetries = 2)
    {
        var json = JsonSerializer.Serialize(body);

        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            if (attempt > 0)
                await Task.Delay(TimeSpan.FromSeconds(3 * attempt));

            try
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _http.PostAsync(endpoint, content);
                var sc = (int)response.StatusCode;

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
                    return (data, sc, string.Empty);
                }

                // No reintentar errores del cliente (4xx)
                if (sc >= 400 && sc < 500)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[ApiClient] POST {endpoint} {sc}: {responseBody}");
                    return (default, sc, responseBody);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiClient] POST {endpoint} intento {attempt + 1} error: {ex.Message}");
                if (attempt == maxRetries)
                    return (default, 0, "Sin conexión");
                continue;
            }
        }

        return (default, 503, "El servidor no respondió");
    }

    public async Task<bool> PostAsync(string endpoint, object body)
    {
        try
        {
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync(endpoint, content);
            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[ApiClient] POST {endpoint} {(int)response.StatusCode}: {responseBody}");
            }
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
