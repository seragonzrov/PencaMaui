using PencaMaui.Models;
using System.Text.Json;
#if ANDROID
using Plugin.Firebase.CloudMessaging;
#endif

namespace PencaMaui.Services;

public class AuthService
{
    private readonly ApiClient _api;
    private const string TokenKey = "auth_token";
    private const string UserIdKey = "user_id";
    private const string UserNombreKey = "user_nombre";
    private const string UserEmailKey = "user_email";

    public AuthService(ApiClient api)
    {
        _api = api;

#if ANDROID
        if (CrossFirebaseCloudMessaging.IsSupported)
        {
            CrossFirebaseCloudMessaging.Current.TokenChanged += async (_, e) =>
            {
                if (await EstaLogueadoAsync())
                    await RegistrarFcmTokenAsync(e.Token);
            };
        }
#endif
    }

    public async Task<(bool success, string error)> LoginAsync(string email, string password)
    {
        var dto = new LoginRequestDto { Email = email, Password = password };
        var (result, statusCode, errorMsg) = await _api.PostDetailedAsync<AuthResponse>("/api/Auth/login", dto);

        if (result == null || string.IsNullOrEmpty(result.Token))
        {
            return statusCode switch
            {
                400 or 401 => (false, "Email o contraseña incorrectos"),
                0           => (false, "No se pudo conectar al servidor. Verificá tu conexión a internet."),
                _           => (false, "El servidor tardó en responder. Intentá de nuevo en unos segundos.")
            };
        }

        await GuardarSesionAsync(result, email);
        return (true, string.Empty);
    }

    public async Task<(bool success, string error)> LoginFirebaseAsync(string idToken)
    {
        var dto = new FirebaseLoginRequest { IdToken = idToken };
        var (result, statusCode, error) = await _api.PostDetailedAsync<AuthResponse>("/api/Auth/firebase", dto);

        if (result == null || string.IsNullOrEmpty(result.Token))
            return (false, $"Error al autenticar con red social ({statusCode}: {error})");

        await GuardarSesionAsync(result);
        return (true, string.Empty);
    }

    public async Task<(bool success, string error)> RegistrarAsync(
        string nombre, string email, string password, string? codigoInvitacion = null)
    {
        var dto = new RegistroUsuarioRequestDto
        {
            Nombre = nombre,
            Email = email,
            Password = password,
            Rol = 0,
            CodigoInvitacion = codigoInvitacion
        };

        var result = await _api.PostAsync<AuthResponse>("/api/Auth/registro/usuario", dto);

        if (result == null || string.IsNullOrEmpty(result.Token))
            return (false, "Error al registrar usuario");

        await GuardarSesionAsync(result, email);
        return (true, string.Empty);
    }

    private async Task GuardarSesionAsync(AuthResponse auth, string? emailOverride = null)
    {
        var (id, emailFromJwt) = ParseJwt(auth.Token);
        var email = emailOverride ?? emailFromJwt;

        await SecureStorage.SetAsync(TokenKey, auth.Token);
        await SecureStorage.SetAsync(UserIdKey, id);
        await SecureStorage.SetAsync(UserNombreKey, auth.Nombre);
        await SecureStorage.SetAsync(UserEmailKey, email);
        _api.SetToken(auth.Token);

#if ANDROID
        await RegistrarFcmTokenActualAsync();
#endif
    }

#if ANDROID
    private async Task RegistrarFcmTokenActualAsync()
    {
        try
        {
            var jwt = await SecureStorage.GetAsync(TokenKey);
            if (!string.IsNullOrEmpty(jwt))
            {
                var parts = jwt.Split('.');
                var payload = parts[1].PadRight(parts[1].Length + (4 - parts[1].Length % 4) % 4, '=');
                var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(payload));
                Console.WriteLine($"[FCM] JWT claims: {json}");
            }

            Console.WriteLine($"[FCM] IsSupported = {CrossFirebaseCloudMessaging.IsSupported}");
            if (!CrossFirebaseCloudMessaging.IsSupported) return;

            var token = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();
            Console.WriteLine($"[FCM] Token obtenido: {token}");
            await RegistrarFcmTokenAsync(token);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FCM] Error al obtener el token: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private async Task RegistrarFcmTokenAsync(string fcmToken)
    {
        if (string.IsNullOrWhiteSpace(fcmToken)) return;

        try
        {
            var dto = new RegistrarFcmTokenRequestDto { FcmToken = fcmToken };
            var ok = await _api.PostAsync("/api/Usuario/registrar/fcm-token", dto);
            Console.WriteLine($"[FCM] Registro en backend: {(ok ? "OK" : "FALLÓ")}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FCM] Error al registrar el token: {ex.GetType().Name}: {ex.Message}");
        }
    }
#endif

    private (string id, string email) ParseJwt(string token)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length != 3) return ("", "");
            var payload = parts[1];
            payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
            var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(payload));
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var id = root.TryGetProperty("sub", out var sub) ? sub.GetString() ?? "" : "";
            var email = root.TryGetProperty("email", out var em) ? em.GetString() ?? "" : "";
            return (id, email);
        }
        catch
        {
            return ("", "");
        }
    }

    public async Task<bool> CargarSesionAsync()
    {
        var token = await SecureStorage.GetAsync(TokenKey);
        if (string.IsNullOrEmpty(token)) return false;

        var (id, _) = ParseJwt(token);
        if (string.IsNullOrEmpty(id)) return false;

        // Verificar expiración
        try
        {
            var parts = token.Split('.');
            var payload = parts[1].PadRight(parts[1].Length + (4 - parts[1].Length % 4) % 4, '=');
            var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(payload));
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("exp", out var exp))
            {
                var expTime = DateTimeOffset.FromUnixTimeSeconds(exp.GetInt64());
                if (expTime < DateTimeOffset.UtcNow)
                {
                    await LogoutAsync();
                    return false;
                }
            }
        }
        catch { }

        _api.SetToken(token);

#if ANDROID
        await RegistrarFcmTokenActualAsync();
#endif

        return true;
    }

    public async Task LogoutAsync()
    {
        SecureStorage.Remove(TokenKey);
        SecureStorage.Remove(UserIdKey);
        SecureStorage.Remove(UserNombreKey);
        SecureStorage.Remove(UserEmailKey);
        _api.ClearToken();
    }

    public async Task<string?> GetTokenAsync() =>
        await SecureStorage.GetAsync(TokenKey);

    public async Task<string?> GetUserIdAsync() =>
        await SecureStorage.GetAsync(UserIdKey);

    public async Task<string?> GetUserNombreAsync() =>
        await SecureStorage.GetAsync(UserNombreKey);

    public async Task<string?> GetUserEmailAsync() =>
        await SecureStorage.GetAsync(UserEmailKey);

    public async Task<bool> EstaLogueadoAsync()
    {
        var token = await SecureStorage.GetAsync(TokenKey);
        return !string.IsNullOrEmpty(token);
    }
    public async Task ActualizarNombreAsync(string nombre)
    {
        await SecureStorage.SetAsync(UserNombreKey, nombre);
    }
}