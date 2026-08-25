using System;

namespace PSMSystem.Models;

public class AvanceTrabajo
{
    public int IdAvance { get; set; }
    public int IdOrdenTrabajo { get; set; }
    public required string Etapa { get; set; }
    public required string Descripcion { get; set; }
    public DateOnly Fecha { get; set; }

    public OrdenTrabajo? OrdenTrabajo { get; set; }
}
