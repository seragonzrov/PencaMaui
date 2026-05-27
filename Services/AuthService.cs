using PencaMaui.Models;

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
    }

   /* public async Task<(bool success, string error)> LoginAsync(string email, string password)
   {
        var dto = new LoginRequestDto { Email = email, Password = password };
        var result = await _api.PostAsync<AuthResponse>("/api/Auth/login", dto);

        if (result == null || string.IsNullOrEmpty(result.Token))
            return (false, "Email o contraseña incorrectos");

        await GuardarSesionAsync(result);
        return (true, string.Empty);
    }*/
    public async Task<(bool success, string error)> LoginAsync(string email, string password)
    {
        await Task.Delay(500);
        await SecureStorage.SetAsync(TokenKey, "mock_token");
        await SecureStorage.SetAsync(UserIdKey, "1");
        await SecureStorage.SetAsync(UserNombreKey, "Jose Garcia");
        await SecureStorage.SetAsync(UserEmailKey, email);
        return (true, string.Empty);
    }

    public async Task<(bool success, string error)> LoginFirebaseAsync(string idToken)
    {
        var dto = new FirebaseLoginRequest { IdToken = idToken };
        var result = await _api.PostAsync<AuthResponse>("/api/Auth/firebase", dto);

        if (result == null || string.IsNullOrEmpty(result.Token))
            return (false, "Error al autenticar con red social");

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

        await GuardarSesionAsync(result);
        return (true, string.Empty);
    }

    private async Task GuardarSesionAsync(AuthResponse auth)
    {
        await SecureStorage.SetAsync(TokenKey, auth.Token);
        await SecureStorage.SetAsync(UserIdKey, auth.Id);
        await SecureStorage.SetAsync(UserNombreKey, auth.Nombre);
        await SecureStorage.SetAsync(UserEmailKey, auth.Email);
        _api.SetToken(auth.Token);
    }

    public async Task<bool> CargarSesionAsync()
    {
        var token = await SecureStorage.GetAsync(TokenKey);
        if (string.IsNullOrEmpty(token)) return false;
        _api.SetToken(token);
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
}
