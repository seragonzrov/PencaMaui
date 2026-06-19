using PencaMaui.Models;

namespace PencaMaui.Services;

public class PencaService
{
    private readonly ApiClient _api;

    public PencaService(ApiClient api)
    {
        _api = api;
    }

    public async Task<List<Penca>> ObtenerPencasAsync()
    {
        var result = await _api.GetAsync<List<Penca>>("/api/Penca");
        return result ?? new List<Penca>();
    }

    public async Task<List<string>> ObtenerInscripcionesAsync()
    {
        var result = await _api.GetAsync<List<string>>("/api/Pago/mis-inscripciones");
        return result ?? new List<string>();
    }

    public async Task<Penca?> ObtenerPencaAsync(string id)
    {
        return await _api.GetAsync<Penca>($"/api/Penca/{id}");
    }

    public async Task<List<PosicionEntry>> ObtenerTablaPosicionesAsync(string pencaId)
    {
        var result = await _api.GetAsync<TablaPosicionesResponse>($"/api/Penca/{pencaId}/tabla-posiciones");
        return result?.Posiciones.Select(p => p.ToPosicionEntry()).ToList() ?? new List<PosicionEntry>();
    }
}

public class PrediccionService
{
    private readonly ApiClient _api;

    public PrediccionService(ApiClient api)
    {
        _api = api;
    }

    public async Task<List<Prediccion>> ObtenerPrediccionesAsync(string pencaId)
    {
        var result = await _api.GetAsync<HistorialPencaResponseDto>($"/api/Prediccion/{pencaId}/historial");
        return result?.Partidos.Select(p => p.ToPrediccion(pencaId)).ToList() ?? new List<Prediccion>();
    }

    public async Task<bool> GuardarPrediccionAsync(PrediccionRequestDto dto)
    {
        return await _api.PostAsync("/api/Prediccion", dto);
    }
}

public class UsuarioService
{
    private readonly ApiClient _api;

    public UsuarioService(ApiClient api)
    {
        _api = api;
    }

    public async Task<Usuario?> ObtenerUsuarioAsync(string usuarioId)
    {
        return await _api.GetAsync<Usuario>($"/api/Usuario/obtener/{usuarioId}");
    }

    public async Task<bool> ActualizarUsuarioAsync(string id, string nombre, string? foto = null)
    {
        var dto = new UsuarioRequestDto { Id = id, Nombre = nombre, Foto = foto };
        return await _api.PostAsync("/api/Usuario/actualizar", dto);
    }

    public async Task<ActualizarPreferenciasNotificacionRequestDto?> ObtenerPreferenciasNotificacionAsync(string usuarioId)
    {
        return await _api.GetAsync<ActualizarPreferenciasNotificacionRequestDto>(
            $"/api/Usuario/{usuarioId}/notificaciones/preferencias");
    }

    public async Task<bool> ActualizarPreferenciasNotificacionAsync(
        bool recordatorio, bool resultado, bool resumenSemanal)
    {
        var dto = new ActualizarPreferenciasNotificacionRequestDto
        {
            NotifRecordatorioPrediccion = recordatorio,
            NotifResultadoPartido = resultado,
            NotifResumenSemanal = resumenSemanal
        };
        return await _api.PostAsync("/api/Usuario/notificaciones/preferencias", dto);
    }
}