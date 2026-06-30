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
        var inscripciones = await _pencaService.ObtenerInscripcionesAsync();
        var inscriptas = lista.Where(p => inscripciones.Contains(p.Id)).ToList();
        Pencas = new ObservableCollection<Penca>(inscriptas);
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
    private readonly PencaService _pencaService;
    private IDispatcherTimer? _timer;
    private List<Prediccion> _todasLasPredicciones = new();

    [ObservableProperty] string pencaId = string.Empty;
    [ObservableProperty] string nombrePenca = string.Empty;
    [ObservableProperty] ObservableCollection<Prediccion> predicciones = new();
    [ObservableProperty] bool isBusy;
    [ObservableProperty] string mensajeEstado = string.Empty;
    [ObservableProperty] bool pencaAbierta;
    [ObservableProperty] bool pencaEnCurso;
    [ObservableProperty] bool pencaFinalizada;
    [ObservableProperty] int filtroEstado = -1;  // -1=todos, 0=Abiertos, 1=Finalizados
    [ObservableProperty] bool filtroJugados;

    public bool AbiertosActivo => FiltroEstado == 0;
    public bool FinalizadosActivo => FiltroEstado == 1;

    partial void OnFiltroEstadoChanged(int value)
    {
        OnPropertyChanged(nameof(AbiertosActivo));
        OnPropertyChanged(nameof(FinalizadosActivo));
        AplicarFiltro();
    }

    partial void OnFiltroJugadosChanged(bool value) => AplicarFiltro();

    void AplicarFiltro()
    {
        var resultado = _todasLasPredicciones.AsEnumerable();
        if (FiltroEstado == 0) resultado = resultado.Where(p => !p.PartidoJugado);
        else if (FiltroEstado == 1) resultado = resultado.Where(p => p.PartidoJugado);
        if (FiltroJugados) resultado = resultado.Where(p => p.Guardado);
        Predicciones = new ObservableCollection<Prediccion>(resultado);
    }

    [RelayCommand]
    void FiltrarPor(string? filtro)
    {
        var valor = int.TryParse(filtro, out int v) ? v : -1;
        FiltroEstado = FiltroEstado == valor ? -1 : valor;
    }

    [RelayCommand]
    void ToggleJugados() => FiltroJugados = !FiltroJugados;

    public PrediccionViewModel(PrediccionService prediccionService, PencaService pencaService)
    {
        _prediccionService = prediccionService;
        _pencaService = pencaService;
    }

    [RelayCommand]
    async Task CargarAsync()
    {
        if (string.IsNullOrEmpty(PencaId)) return;
        IsBusy = true;

        var pencaTask = _pencaService.ObtenerPencaAsync(PencaId);
        var historialTask = _prediccionService.ObtenerHistorialAsync(PencaId);
        var margenTask = _pencaService.ObtenerTiempoLimitePrediccionAsync(PencaId);
        await Task.WhenAll(pencaTask, historialTask, margenTask);

        PencaAbierta = pencaTask.Result?.Estado == 0;
        PencaEnCurso = pencaTask.Result?.Estado == 1;
        PencaFinalizada = pencaTask.Result?.Estado == 2;

        _todasLasPredicciones = historialTask.Result?.Partidos
            .Select(p => p.ToPrediccion(PencaId, margenTask.Result))
            .ToList() ?? new List<Prediccion>();
        FiltroEstado = -1;
        FiltroJugados = false;
        AplicarFiltro();
        IsBusy = false;

        IniciarTimer();
    }

    void IniciarTimer()
    {
        if (_timer != null) return;

        _timer = Application.Current!.Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(30);
        _timer.Tick += (_, _) =>
        {
            foreach (var p in Predicciones)
                p.RefrescarTiempo();
        };
        _timer.Start();
    }

    public void DetenerTimer()
    {
        _timer?.Stop();
        _timer = null;
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
    private bool _cargando;

    [ObservableProperty] string nombre = string.Empty;
    [ObservableProperty] string email = string.Empty;
    [ObservableProperty] string iniciales = string.Empty;
    [ObservableProperty] bool isBusy;

    // Notificaciones
    [ObservableProperty] bool notifRecordatorio;
    [ObservableProperty] bool notifResultado;
    [ObservableProperty] bool notifResumenSemanal;
    [ObservableProperty] bool confirmandoCierre;
    [ObservableProperty] bool confirmacionGuardadoVisible;
    [ObservableProperty] string mensajeGuardado = string.Empty;

    public PerfilViewModel(AuthService auth, UsuarioService usuarioService)
    {
        _auth = auth;
        _usuarioService = usuarioService;
    }

    [RelayCommand]
    async Task CargarAsync()
    {
        IsBusy = true;
        _cargando = true;
        try
        {
            Nombre = await _auth.GetUserNombreAsync() ?? "";
            Email = await _auth.GetUserEmailAsync() ?? "";
            Iniciales = Nombre.Length >= 2 ? Nombre[..2].ToUpper() : Nombre.ToUpper();

            var userId = await _auth.GetUserIdAsync();
            if (!string.IsNullOrEmpty(userId))
            {
                var prefs = await _usuarioService.ObtenerPreferenciasNotificacionAsync(userId);
                if (prefs != null)
                {
                    NotifRecordatorio = prefs.NotifRecordatorioPrediccion;
                    NotifResultado = prefs.NotifResultadoPartido;
                    NotifResumenSemanal = prefs.NotifResumenSemanal;
                }
            }
        }
        finally
        {
            _cargando = false;
            IsBusy = false;
        }
    }

    partial void OnNotifRecordatorioChanged(bool value)
    {
        if (!_cargando) _ = _usuarioService.ActualizarPreferenciasNotificacionAsync(
            value, NotifResultado, NotifResumenSemanal);
    }

    partial void OnNotifResultadoChanged(bool value)
    {
        if (!_cargando) _ = _usuarioService.ActualizarPreferenciasNotificacionAsync(
            NotifRecordatorio, value, NotifResumenSemanal);
    }

    partial void OnNotifResumenSemanalChanged(bool value)
    {
        if (!_cargando) _ = _usuarioService.ActualizarPreferenciasNotificacionAsync(
            NotifRecordatorio, NotifResultado, value);
    }

    [RelayCommand]
    void CerrarConfirmacionGuardado() => ConfirmacionGuardadoVisible = false;

    [RelayCommand]
    void CerrarSesion() => ConfirmandoCierre = true;

    [RelayCommand]
    void CancelarCierre() => ConfirmandoCierre = false;

    [RelayCommand]
    async Task ConfirmarCierreAsync()
    {
        await _auth.LogoutAsync();

        foreach (var item in Shell.Current.Items)
            foreach (var section in item.Items)
                if (section.Navigation.NavigationStack.Count > 1)
                    await section.Navigation.PopToRootAsync(false);

        await Shell.Current.GoToAsync("//login");
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
            MensajeGuardado = "Cambios guardados";
        }
        else
            MensajeGuardado = "No se pudo guardar";

        ConfirmacionGuardadoVisible = true;
    }
}
