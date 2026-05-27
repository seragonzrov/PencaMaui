/*using PencaMaui.Models;

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

    public async Task<Penca?> ObtenerPencaAsync(string id)
    {
        return await _api.GetAsync<Penca>($"/api/Penca/{id}");
    }

    public async Task<List<PosicionEntry>> ObtenerTablaPosicionesAsync(string pencaId)
    {
        var result = await _api.GetAsync<List<PosicionEntry>>($"/api/Penca/{pencaId}/tabla-posiciones");
        return result ?? new List<PosicionEntry>();
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
        var result = await _api.GetAsync<List<Prediccion>>($"/api/Prediccion/{pencaId}");
        return result ?? new List<Prediccion>();
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
}*/

using PencaMaui.Models;

namespace PencaMaui.Services;

public class PencaService
{
    public async Task<List<Penca>> ObtenerPencasAsync()
    {
        await Task.Delay(500);
        return new List<Penca>
        {
            new Penca { Id = "1", Nombre = "Penca Mundial 2026", Estado = 0, MontoEntrada = 150 },
            new Penca { Id = "2", Nombre = "Copa Bimbo", Estado = 0, MontoEntrada = 50 },
            new Penca { Id = "3", Nombre = "Finaaal", Estado = 2, MontoEntrada = 200 }
        };
    }

    public async Task<Penca?> ObtenerPencaAsync(string id)
    {
        var pencas = await ObtenerPencasAsync();
        return pencas.FirstOrDefault(p => p.Id == id);
    }

    public async Task<List<PosicionEntry>> ObtenerTablaPosicionesAsync(string pencaId)
    {
        await Task.Delay(300);
        return new List<PosicionEntry>
        {
            new PosicionEntry { Posicion = 1, NombreUsuario = "María R.", Puntos = 178, Aciertos = 18, Exactos = 6 },
            new PosicionEntry { Posicion = 2, NombreUsuario = "Juan S.", Puntos = 161, Aciertos = 17, Exactos = 5 },
            new PosicionEntry { Posicion = 3, NombreUsuario = "Jose G.", Puntos = 142, Aciertos = 16, Exactos = 4, EsUsuarioActual = true },
            new PosicionEntry { Posicion = 4, NombreUsuario = "Andrés G.", Puntos = 130, Aciertos = 15, Exactos = 3 },
            new PosicionEntry { Posicion = 5, NombreUsuario = "Laura C.", Puntos = 105, Aciertos = 13, Exactos = 2 },
        };
    }
}

public class PrediccionService
{
    public async Task<List<Prediccion>> ObtenerPrediccionesAsync(string pencaId)
    {
        await Task.Delay(300);
        return new List<Prediccion>
        {
            new Prediccion
            {
                Id = "1", PencaId = pencaId, Guardado = true,
                GolesLocal = 2, GolesVisitante = 1,
                CierrePrediccion = DateTime.Now.AddHours(2),
                Partido = new Partido
                {
                    Id = "p1", Fase = "Grupo A · Fecha 1",
                    EquipoLocal = new Equipo { Id = "e1", Nombre = "Uruguay" },
                    EquipoVisitante = new Equipo { Id = "e2", Nombre = "Portugal" }
                }
            },
            new Prediccion
            {
                Id = "2", PencaId = pencaId, Guardado = false,
                GolesLocal = 0, GolesVisitante = 0,
                CierrePrediccion = DateTime.Now.AddMinutes(45),
                Partido = new Partido
                {
                    Id = "p2", Fase = "Grupo B · Fecha 1",
                    EquipoLocal = new Equipo { Id = "e3", Nombre = "Argentina" },
                    EquipoVisitante = new Equipo { Id = "e4", Nombre = "Francia" }
                }
            },
            new Prediccion
            {
                Id = "3", PencaId = pencaId, Cerrado = true,
                GolesLocal = 3, GolesVisitante = 0,
                Partido = new Partido
                {
                    Id = "p3", Fase = "Grupo C · Fecha 1",
                    EquipoLocal = new Equipo { Id = "e5", Nombre = "Brasil" },
                    EquipoVisitante = new Equipo { Id = "e6", Nombre = "México" }
                }
            }
        };
    }

    public async Task<bool> GuardarPrediccionAsync(PrediccionRequestDto dto)
    {
        await Task.Delay(300);
        return true;
    }
}

public class UsuarioService
{
    public async Task<Usuario?> ObtenerUsuarioAsync(string usuarioId)
    {
        await Task.Delay(200);
        return new Usuario { Id = usuarioId, Nombre = "Jose Garcia", Email = "josen@gmail.com" };
    }

    public async Task<bool> ActualizarUsuarioAsync(string id, string nombre, string? foto = null)
    {
        await Task.Delay(200);
        return true;
    }
}
