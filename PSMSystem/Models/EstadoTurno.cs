using System;

namespace PSMSystem.Models;

public class EstadoTurno
{
    public int IdEstadoTurno { get; set; }
    public required string Nombre { get; set; }

    public ICollection<Turno> Turnos { get; set; } = new List<Turno>();
}
