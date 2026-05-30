using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PencaMaui.Models;
using PencaMaui.Services;
using System.Collections.ObjectModel;

namespace PencaMaui.ViewModels;

// ─── Home Pencas ────────────────────────────────────────────────────────────

public partial class HomePencasViewModel : ObservableObject
{
    private readonly PencaService _pencaService;
    private readonly AuthService _auth;

    [ObservableProperty] ObservableCollection<Penca> pencas = new();
    [ObservableProperty] bool isBusy;
    [ObservableProperty] string nombreUsuario = string.Empty;
    [ObservableProperty] string bienvenida = string.Empty;

    public HomePencasViewModel(PencaService pencaService, AuthService auth)
    {
        _pencaService = pencaService;
        _auth = auth;
    }

    [RelayCommand]
    async Task CargarAsync()
    {
        IsBusy = true;
        var nombre = await _auth.GetUserNombreAsync();
        NombreUsuario = nombre ?? "Usuario";
        Bienvenida = $"Hola, {NombreUsuario} 👋";

        var lista = await _pencaService.ObtenerPencasAsync();
        Pencas = new ObservableCollection<Penca>(lista);
        IsBusy = false;
    }

    [RelayCommand]
    async Task VerPosicionesAsync(Penca penca)
    {
        await Shell.Current.GoToAsync($"posiciones?pencaId={penca.Id}&nombre={Uri.EscapeDataString(penca.Nombre)}");
    }

    [RelayCommand]
    async Task PredecirAsync(Penca penca)
    {
        await Shell.Current.GoToAsync($"prediccion?pencaId={penca.Id}&nombre={Uri.EscapeDataString(penca.Nombre)}");
    }
}

// ─── Posiciones ─────────────────────────────────────────────────────────────

[QueryProperty("PencaId", "pencaId")]
[QueryProperty("NombrePenca", "nombre")]
public partial class PosicionesViewModel : ObservableObject
{
    private readonly PencaService _pencaService;
    private readonly AuthService _auth;

    [ObservableProperty] string pencaId = string.Empty;
    [ObservableProperty] string nombrePenca = string.Empty;
    [ObservableProperty] ObservableCollection<PosicionEntry> posiciones = new();
    [ObservableProperty] PosicionEntry? miPosicion;
    [ObservableProperty] bool isBusy;

    public PosicionesViewModel(PencaService pencaService, AuthService auth)
    {
        _pencaService = pencaService;
        _auth = auth;
    }

    [RelayCommand]
    async Task CargarAsync()
    {
        if (string.IsNullOrEmpty(PencaId)) return;
        IsBusy = true;

        var userId = await _auth.GetUserIdAsync();
        var lista = await _pencaService.ObtenerTablaPosicionesAsync(PencaId);

        foreach (var p in lista)
            p.EsUsuarioActual = p.UsuarioId == userId;

        MiPosicion = lista.FirstOrDefault(p => p.EsUsuarioActual);
        Posiciones = new ObservableCollection<PosicionEntry>(lista);
        IsBusy = false;
    }
}

// ─── Prediccion ─────────────────────────────────────────────────────────────

[QueryProperty("PencaId", "pencaId")]
[QueryProperty("NombrePenca", "nombre")]
public partial class PrediccionViewModel : ObservableObject
{
    private readonly PrediccionService _prediccionService;

    [ObservableProperty] string pencaId = string.Empty;
    [ObservableProperty] string nombrePenca = string.Empty;
    [ObservableProperty] ObservableCollection<Prediccion> predicciones = new();
    [ObservableProperty] bool isBusy;
    [ObservableProperty] string mensajeEstado = string.Empty;

    public PrediccionViewModel(PrediccionService prediccionService)
    {
        _prediccionService = prediccionService;
    }

    [RelayCommand]
    async Task CargarAsync()
    {
        if (string.IsNullOrEmpty(PencaId)) return;
        IsBusy = true;
        var lista = await _prediccionService.ObtenerPrediccionesAsync(PencaId);
        Predicciones = new ObservableCollection<Prediccion>(lista);
        IsBusy = false;
    }

    [RelayCommand]
    async Task GuardarPrediccionAsync(Prediccion prediccion)
    {
        if (prediccion.Partido == null) return;

        var dto = new PrediccionRequestDto
        {
            PencaId = PencaId,
            PartidoId = prediccion.Partido.Id,
            GolesLocal = prediccion.GolesLocal,
            GolesVisitante = prediccion.GolesVisitante
        };

        var ok = await _prediccionService.GuardarPrediccionAsync(dto);
        if (ok)
        {
            prediccion.Guardado = true;
            MensajeEstado = "Predicción guardada ✓";
        }
        else
        {
            MensajeEstado = "Error al guardar, intentá de nuevo";
        }
    }
}

// ─── Perfil ─────────────────────────────────────────────────────────────────

public partial class PerfilViewModel : ObservableObject
{
    private readonly AuthService _auth;
    private readonly UsuarioService _usuarioService;

    [ObservableProperty] string nombre = string.Empty;
    [ObservableProperty] string email = string.Empty;
    [ObservableProperty] string iniciales = string.Empty;
    [ObservableProperty] bool isBusy;

    // Notificaciones
    [ObservableProperty] bool notifRecordatorio = true;
    [ObservableProperty] bool notifResultado = true;
    [ObservableProperty] bool notifResumenSemanal = true;
    [ObservableProperty] bool notifPush = true;

    public PerfilViewModel(AuthService auth, UsuarioService usuarioService)
    {
        _auth = auth;
        _usuarioService = usuarioService;
    }

    [RelayCommand]
    async Task CargarAsync()
    {
        IsBusy = true;
        Nombre = await _auth.GetUserNombreAsync() ?? "";
        Email = await _auth.GetUserEmailAsync() ?? "";
        Iniciales = Nombre.Length >= 2 ? Nombre[..2].ToUpper() : Nombre.ToUpper();
        IsBusy = false;
    }

    [RelayCommand]
    async Task CerrarSesionAsync()
    {
        bool confirmar = await Shell.Current.DisplayAlertAsync(
            "Cerrar sesión", "¿Seguro que querés salir?", "Sí", "Cancelar");

        if (confirmar)
        {
            await _auth.LogoutAsync();
            await Shell.Current.GoToAsync("//login");
        }
    }

    [RelayCommand]
    async Task GuardarPerfilAsync()
    {
        var userId = await _auth.GetUserIdAsync();
        if (string.IsNullOrEmpty(userId)) return;

        IsBusy = true;
        var ok = await _usuarioService.ActualizarUsuarioAsync(userId, Nombre);
        IsBusy = false;

        if (ok)
        {
            await _auth.ActualizarNombreAsync(Nombre);
            await Shell.Current.DisplayAlertAsync("Perfil", "Cambios guardados", "OK");
        }
        else
            await Shell.Current.DisplayAlertAsync("Error", "No se pudo guardar", "OK");
    }
}
