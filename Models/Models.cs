namespace PencaMaui.Models;

// ─── Auth ───────────────────────────────────────────────────────────────────

public class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class FirebaseLoginRequest
{
    public string IdToken { get; set; } = string.Empty;
}

public class RegistrarFcmTokenRequestDto
{
    public string FcmToken { get; set; } = string.Empty;
}

public class RegistroUsuarioRequestDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int Rol { get; set; } = 0;
    public string? CodigoInvitacion { get; set; }
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public DateTime Expira { get; set; }
}

// ─── Sitio ──────────────────────────────────────────────────────────────────

public class SitioDto
{
    public string Id { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string UrlPropia { get; set; } = string.Empty;
}

// ─── Penca ──────────────────────────────────────────────────────────────────

public class TiempoLimitePrediccionDto
{
    public int TiempoLimitePrevioMinutos { get; set; }
}

public class Penca
{
    public string Id { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int Estado { get; set; }        // EstadoPenca: 0=Abierta,1=Cerrada,2=Finalizada,3=?
    public int MontoEntrada { get; set; }
    public int PorcentajePremio1 { get; set; }
    public int PorcentajePremio2 { get; set; }
    public int PorcentajePremio3 { get; set; }
    public PlantillaPenca? Plantilla { get; set; }

    public string EstadoTexto => Estado switch
    {
        0 => "Inscripto",
        1 => "En curso",
        2 => "Finalizada",
        _ => "Desconocido"
    };

    public Color EstadoColor => Estado switch
    {
        0 => Color.FromArgb("#1D9E75"),
        1 => Color.FromArgb("#EF9F27"),
        2 => Color.FromArgb("#888888"),
        _ => Color.FromArgb("#888888")
    };

    public bool PosicionesDisponible => Estado != 0;
}

public class PlantillaPenca
{
    public string Id { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}

// ─── Posiciones ─────────────────────────────────────────────────────────────

public class PosicionEntry
{
    public int Posicion { get; set; }
    public string UsuarioId { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public int Puntos { get; set; }
    public int Aciertos { get; set; }
    public bool EsUsuarioActual { get; set; }

    public string Iniciales => NombreUsuario.Length >= 2
        ? NombreUsuario[..2].ToUpper()
        : NombreUsuario.ToUpper();

    public Color ColorAvatar => EsUsuarioActual
        ? Color.FromArgb("#9FE1CB")
        : Color.FromArgb("#E0E0E0");

    public Color ColorFila => EsUsuarioActual
        ? Color.FromArgb("#E1F5EE")
        : Colors.Transparent;

    public Color ColorPuntos => EsUsuarioActual
        ? Color.FromArgb("#0F6E56")
        : Color.FromArgb("#333333");
}

public class TablaPosicionesResponse
{
    public string PencaId { get; set; } = string.Empty;
    public string NombrePenca { get; set; } = string.Empty;
    public List<PosicionResponseDto> Posiciones { get; set; } = new();
}

public class PosicionResponseDto
{
    public int Posicion { get; set; }
    public string UsuarioId { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public int PuntosTotales { get; set; }
    public int PartidosPredichos { get; set; }

    public PosicionEntry ToPosicionEntry() => new PosicionEntry
    {
        Posicion = Posicion,
        UsuarioId = UsuarioId,
        NombreUsuario = NombreUsuario,
        Puntos = PuntosTotales,
        Aciertos = PartidosPredichos
    };
}

// ─── Predicción ─────────────────────────────────────────────────────────────

public class PrediccionRequestDto
{
    public string PencaId { get; set; } = string.Empty;
    public string PartidoId { get; set; } = string.Empty;
    public int GolesLocal { get; set; }
    public int GolesVisitante { get; set; }
    public string? EquipoGanadorPredichoId { get; set; }
}

public class Prediccion
{
    public string Id { get; set; } = string.Empty;
    public string PencaId { get; set; } = string.Empty;
    public Partido? Partido { get; set; }
    public int GolesLocal { get; set; }
    public int GolesVisitante { get; set; }
    public bool Guardado { get; set; }
    public bool Cerrado { get; set; }
    public bool PartidoJugado { get; set; }
    public int? ResultadoLocal { get; set; }
    public int? ResultadoVisitante { get; set; }
    public int PuntosObtenidos { get; set; }
    public DateTime? CierrePrediccion { get; set; }

    public string ResultadoTexto => PartidoJugado
        ? $"{ResultadoLocal} - {ResultadoVisitante}"
        : string.Empty;

    public bool MostrarGuardado => Guardado && !Cerrado;

    public bool MostrarPuntos => PartidoJugado && Guardado;

    public string EquiposTexto => $"{Partido?.EquipoLocal?.Nombre} vs {Partido?.EquipoVisitante?.Nombre}";

    public string MiPrediccionTexto => Guardado
        ? $"{GolesLocal} - {GolesVisitante}"
        : string.Empty;

    public string FechaHoraTexto => CierrePrediccion?.ToString("d/M/yyyy HH:mm") ?? string.Empty;

    public string TiempoRestante
    {
        get
        {
            if (Cerrado) return "Plazo vencido";
            if (CierrePrediccion == null) return "";
            var diff = CierrePrediccion.Value - DateTime.Now;
            if (diff.TotalMinutes < 60)
                return $"Cierra en {(int)diff.TotalMinutes} min";
            return $"Cierra en {(int)diff.TotalHours}h {diff.Minutes}min";
        }
    }

    public Color ColorTiempo => Cerrado
        ? Color.FromArgb("#888888")
        : (CierrePrediccion.HasValue && (CierrePrediccion.Value - DateTime.Now).TotalMinutes < 60)
            ? Color.FromArgb("#993C1D")
            : Color.FromArgb("#555555");
}

public class HistorialPencaResponseDto
{
    public string PencaId { get; set; } = string.Empty;
    public string NombrePenca { get; set; } = string.Empty;
    public int PuntosTotales { get; set; }
    public int PartidosPredichos { get; set; }
    public int PartidosConResultado { get; set; }
    public List<HistorialPartidoDto> Partidos { get; set; } = new();
}

public class HistorialPartidoDto
{
    public string PartidoId { get; set; } = string.Empty;
    public DateTime FechaPartido { get; set; }
    public string? Fase { get; set; }
    public string EquipoLocal { get; set; } = string.Empty;
    public string EquipoVisitante { get; set; } = string.Empty;
    public bool Predijo { get; set; }
    public int? PrediccionLocal { get; set; }
    public int? PrediccionVisitante { get; set; }
    public int? ResultadoLocal { get; set; }
    public int? ResultadoVisitante { get; set; }
    public bool PartidoJugado { get; set; }
    public int PuntosObtenidos { get; set; }

    public Prediccion ToPrediccion(string pencaId, int margenMinutos = 0)
    {
        // El backend manda FechaPartido en UTC pero sin el sufijo "Z",
        // asi que .NET la deserializa como Kind=Unspecified. Hay que marcarla
        // explicitamente como UTC antes de convertirla a hora local, sino
        // se compara contra DateTime.Now con el desfasaje de la zona horaria.
        var fechaLocal = DateTime.SpecifyKind(FechaPartido, DateTimeKind.Utc).ToLocalTime();
        var cierre = fechaLocal.AddMinutes(-margenMinutos);

        return new Prediccion
        {
            Id = PartidoId,
            PencaId = pencaId,
            GolesLocal = PrediccionLocal ?? 0,
            GolesVisitante = PrediccionVisitante ?? 0,
            Guardado = Predijo,
            Cerrado = PartidoJugado || cierre <= DateTime.Now,
            PartidoJugado = PartidoJugado,
            ResultadoLocal = ResultadoLocal,
            ResultadoVisitante = ResultadoVisitante,
            PuntosObtenidos = PuntosObtenidos,
            CierrePrediccion = cierre,
            Partido = new Partido
            {
                Id = PartidoId,
                Fase = Fase,
                EquipoLocal = new Equipo { Nombre = EquipoLocal },
                EquipoVisitante = new Equipo { Nombre = EquipoVisitante }
            }
        };
    }
}

// ─── Partido ────────────────────────────────────────────────────────────────

public class Partido
{
    public string Id { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public string? Fase { get; set; }
    public Equipo? EquipoLocal { get; set; }
    public Equipo? EquipoVisitante { get; set; }
    public int? GolesLocal { get; set; }
    public int? GolesVisitante { get; set; }
}

public class Equipo
{
    public string Id { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
}

// ─── Usuario ────────────────────────────────────────────────────────────────

public class Usuario
{
    public string Id { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Foto { get; set; }
    public int Rol { get; set; }
    public bool NotifRecordatorioPrediccion { get; set; }
    public bool NotifResultadoPartido { get; set; }
    public bool NotifResumenSemanal { get; set; }

    public string Iniciales => Nombre.Length >= 2
        ? Nombre[..2].ToUpper()
        : Nombre.ToUpper();
}

public class UsuarioRequestDto
{
    public string Id { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Foto { get; set; }
}

public class ActualizarPreferenciasNotificacionRequestDto
{
    public bool NotifRecordatorioPrediccion { get; set; }
    public bool NotifResultadoPartido { get; set; }
    public bool NotifResumenSemanal { get; set; }
}
