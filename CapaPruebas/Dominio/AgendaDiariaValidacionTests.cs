namespace ProyectoOdontologia.Tests.Dominio;

using CapaDominio.Entidades;
using CapaDominio.Excepciones;
using CapaDominio.ObjetosDeValor;

public class AgendaDiariaValidacionTests
{
    private static readonly DateTime FechaBase = new DateTime(2026, 6, 1);

    private static Turno CrearTurno(
        int id,
        int hora,
        int minuto,
        int duracionMinutos,
        int odontologoId = 1,
        EstadoTurno estado = EstadoTurno.Pendiente)
    {
        DateTime inicio = FechaBase.AddHours(hora).AddMinutes(minuto);
        DateTime fin = inicio.AddMinutes(duracionMinutos);

        return new Turno(id, inicio, fin, 1, odontologoId, 1, estado);
    }

    [Fact]
    public void ValidarDisponibilidad_TurnoQueSolapaConExistente_LanzaTurnoSolapado()
    {
        // Arrange
        List<Turno> turnos = new List<Turno>();
        turnos.Add(CrearTurno(1, 9, 0, 60));
        Turno nuevo = CrearTurno(2, 9, 30, 60);

        // Act & Assert
        Assert.Throws<TurnoSolapadoException>(
            delegate { AgendaDiaria.ValidarDisponibilidad(turnos, nuevo); });
    }

    [Fact]
    public void ValidarDisponibilidad_TurnoAdyacente_NoLanza()
    {
        // Arrange
        List<Turno> turnos = new List<Turno>();
        turnos.Add(CrearTurno(1, 9, 0, 60));
        Turno nuevo = CrearTurno(2, 10, 0, 60);

        // Act
        AgendaDiaria.ValidarDisponibilidad(turnos, nuevo);

        // Assert
        Assert.Equal(2, nuevo.Id);
    }

    [Fact]
    public void ValidarDisponibilidad_TurnoQueComienzaAntesDelFinAnterior_LanzaTurnoSolapado()
    {
        // Arrange
        List<Turno> turnos = new List<Turno>();
        turnos.Add(CrearTurno(1, 9, 0, 60));
        turnos.Add(CrearTurno(2, 11, 0, 60));
        Turno nuevo = CrearTurno(3, 9, 30, 60);

        // Act & Assert
        Assert.Throws<TurnoSolapadoException>(
            delegate { AgendaDiaria.ValidarDisponibilidad(turnos, nuevo); });
    }

    [Fact]
    public void ValidarDisponibilidad_TurnoCancelado_NoBloquea()
    {
        // Arrange
        List<Turno> turnos = new List<Turno>();
        turnos.Add(CrearTurno(1, 9, 0, 60, 1, EstadoTurno.Cancelado));
        Turno nuevo = CrearTurno(2, 9, 30, 60);

        // Act
        AgendaDiaria.ValidarDisponibilidad(turnos, nuevo);

        // Assert
        Assert.Equal(EstadoTurno.Pendiente, nuevo.Estado);
    }

    [Fact]
    public void ValidarDisponibilidad_AgendaConTurnosSolapados_LanzaTurnoSolapado()
    {
        // Arrange
        List<Turno> turnos = new List<Turno>();
        turnos.Add(CrearTurno(1, 9, 0, 60));
        turnos.Add(CrearTurno(2, 9, 30, 60));
        Turno nuevo = CrearTurno(3, 12, 0, 30);

        // Act & Assert
        Assert.Throws<TurnoSolapadoException>(
            delegate { AgendaDiaria.ValidarDisponibilidad(turnos, nuevo); });
    }

    [Fact]
    public void ValidarDisponibilidad_CandidatoAntesDeAgendaSolapada_LanzaTurnoSolapado()
    {
        // Arrange
        List<Turno> turnos = new List<Turno>();
        turnos.Add(CrearTurno(1, 9, 0, 60));
        turnos.Add(CrearTurno(2, 9, 30, 60));
        Turno nuevo = CrearTurno(3, 8, 0, 30);

        // Act & Assert
        Assert.Throws<TurnoSolapadoException>(
            delegate { AgendaDiaria.ValidarDisponibilidad(turnos, nuevo); });
    }

    [Fact]
    public void AgregarTurno_TurnoSolapado_LanzaYNoModificaLaAgenda()
    {
        // Arrange
        Turno existente = CrearTurno(1, 9, 0, 60);
        List<Turno> turnos = new List<Turno>();
        turnos.Add(existente);
        AgendaDiaria agenda = new AgendaDiaria(1, FechaBase, turnos);
        Turno nuevo = CrearTurno(2, 9, 30, 60);

        // Act & Assert
        Assert.Throws<TurnoSolapadoException>(
            delegate { agenda.AgregarTurno(nuevo); });
        Assert.Single(agenda.Turnos);
    }
}
