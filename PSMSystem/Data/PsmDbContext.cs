using System;
using Microsoft.EntityFrameworkCore;
using PSMSystem.Models;

namespace PSMSystem.Data;

public class PsmDbContext : DbContext
{
    public PsmDbContext(DbContextOptions<PsmDbContext> options) : base(options)
    {
    }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Vehiculo> Vehiculos => Set<Vehiculo>();
    public DbSet<EstadoTurno> EstadosTurno => Set<EstadoTurno>();
    public DbSet<EstadoOrdenTrabajo> EstadosOrdenTrabajo => Set<EstadoOrdenTrabajo>();
    public DbSet<EstadoPresupuesto> EstadosPresupuesto => Set<EstadoPresupuesto>();
    public DbSet<EstadoFactura> EstadosFactura => Set<EstadoFactura>();
    public DbSet<TipoMovimientoStock> TiposMovimientoStock => Set<TipoMovimientoStock>();
    public DbSet<Repuesto> Repuestos => Set<Repuesto>();
    public DbSet<MovimientoStock> MovimientosStock => Set<MovimientoStock>();
    public DbSet<Turno> Turnos => Set<Turno>();
    public DbSet<OrdenTrabajo> OrdenesTrabajo => Set<OrdenTrabajo>();
    public DbSet<AvanceTrabajo> AvancesTrabajo => Set<AvanceTrabajo>();
    public DbSet<Presupuesto> Presupuestos => Set<Presupuesto>();
    public DbSet<DetallePresupuesto> DetallesPresupuesto => Set<DetallePresupuesto>();
    public DbSet<Factura> Facturas => Set<Factura>();
    public DbSet<DetalleFactura> DetallesFactura => Set<DetalleFactura>();
    public DbSet<Pago> Pagos => Set<Pago>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---------- Cliente ----------
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.ToTable("Cliente");
            entity.HasKey(e => e.IdCliente);
            entity.Property(e => e.IdCliente).HasColumnName("id_cliente");
            entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(100).IsUnicode(false).IsRequired();
            entity.Property(e => e.Apellido).HasColumnName("apellido").HasMaxLength(100).IsUnicode(false).IsRequired();
            entity.Property(e => e.Telefono).HasColumnName("telefono").HasMaxLength(30).IsUnicode(false);
            entity.Property(e => e.Direccion).HasColumnName("direccion").HasMaxLength(200).IsUnicode(false);
            entity.Property(e => e.Activo).HasColumnName("activo").IsRequired();
            entity.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion").HasColumnType("date");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion").HasColumnType("date");
        });

        // ---------- Vehiculo ----------
        modelBuilder.Entity<Vehiculo>(entity =>
        {
            entity.ToTable("Vehiculo");
            entity.HasKey(e => e.IdVehiculo);
            entity.Property(e => e.IdVehiculo).HasColumnName("id_vehiculo");
            entity.Property(e => e.IdCliente).HasColumnName("id_cliente");
            entity.Property(e => e.Marca).HasColumnName("marca").HasMaxLength(100).IsUnicode(false).IsRequired();
            entity.Property(e => e.Modelo).HasColumnName("modelo").HasMaxLength(100).IsUnicode(false).IsRequired();
            entity.Property(e => e.Anio).HasColumnName("anio");
            entity.Property(e => e.Patente).HasColumnName("patente").HasMaxLength(20).IsUnicode(false).IsRequired();
            entity.Property(e => e.Activo).HasColumnName("activo").IsRequired();
            entity.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion").HasColumnType("date");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion").HasColumnType("date");

            entity.HasIndex(e => e.Patente).IsUnique();

            entity.HasOne(e => e.Cliente)
                  .WithMany(c => c.Vehiculos)
                  .HasForeignKey(e => e.IdCliente)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- EstadoTurno ----------
        modelBuilder.Entity<EstadoTurno>(entity =>
        {
            entity.ToTable("EstadoTurno");
            entity.HasKey(e => e.IdEstadoTurno);
            entity.Property(e => e.IdEstadoTurno).HasColumnName("id_estado_turno");
            entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(50).IsUnicode(false).IsRequired();
            entity.HasIndex(e => e.Nombre).IsUnique();

            entity.HasData(
                new EstadoTurno { IdEstadoTurno = 1, Nombre = "Pendiente" },
                new EstadoTurno { IdEstadoTurno = 2, Nombre = "En progreso" },
                new EstadoTurno { IdEstadoTurno = 3, Nombre = "Finalizado" }
            );
        });

        // ---------- EstadoOrdenTrabajo ----------
        modelBuilder.Entity<EstadoOrdenTrabajo>(entity =>
        {
            entity.ToTable("EstadoOrdenTrabajo");
            entity.HasKey(e => e.IdEstadoOrden);
            entity.Property(e => e.IdEstadoOrden).HasColumnName("id_estado_orden");
            entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(50).IsUnicode(false).IsRequired();
            entity.HasIndex(e => e.Nombre).IsUnique();

            // Inferidos del prototipo (Diagnóstico/Avances/Repuestos/Presupuesto → Finalizada);
            // no estaban enumerados explícitamente en tu modelo. Avisame si los querés cambiar.
            entity.HasData(
                new EstadoOrdenTrabajo { IdEstadoOrden = 1, Nombre = "Pendiente" },
                new EstadoOrdenTrabajo { IdEstadoOrden = 2, Nombre = "En proceso" },
                new EstadoOrdenTrabajo { IdEstadoOrden = 3, Nombre = "Esperando repuestos" },
                new EstadoOrdenTrabajo { IdEstadoOrden = 4, Nombre = "Finalizada" }
            );
        });

        // ---------- EstadoPresupuesto ----------
        modelBuilder.Entity<EstadoPresupuesto>(entity =>
        {
            entity.ToTable("EstadoPresupuesto");
            entity.HasKey(e => e.IdEstadoPresupuesto);
            entity.Property(e => e.IdEstadoPresupuesto).HasColumnName("id_estado_presupuesto");
            entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(50).IsUnicode(false).IsRequired();
            entity.HasIndex(e => e.Nombre).IsUnique();

            // También inferidos (CU18 no los enumera). Avisame si los querés cambiar.
            entity.HasData(
                new EstadoPresupuesto { IdEstadoPresupuesto = 1, Nombre = "Pendiente" },
                new EstadoPresupuesto { IdEstadoPresupuesto = 2, Nombre = "Aprobado" },
                new EstadoPresupuesto { IdEstadoPresupuesto = 3, Nombre = "Rechazado" },
                new EstadoPresupuesto { IdEstadoPresupuesto = 4, Nombre = "Facturado" }
            );
        });

        // ---------- EstadoFactura ----------
        modelBuilder.Entity<EstadoFactura>(entity =>
        {
            entity.ToTable("EstadoFactura");
            entity.HasKey(e => e.IdEstadoFactura);
            entity.Property(e => e.IdEstadoFactura).HasColumnName("id_estado_factura");
            entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(50).IsUnicode(false).IsRequired();
            entity.HasIndex(e => e.Nombre).IsUnique();

            // Estos 3 sí están tal cual en el prototipo de Facturación (Pendiente/Pagada/Vencida).
            entity.HasData(
                new EstadoFactura { IdEstadoFactura = 1, Nombre = "Pendiente" },
                new EstadoFactura { IdEstadoFactura = 2, Nombre = "Pagada" },
                new EstadoFactura { IdEstadoFactura = 3, Nombre = "Vencida" }
            );
        });

        // ---------- TipoMovimientoStock ----------
        modelBuilder.Entity<TipoMovimientoStock>(entity =>
        {
            entity.ToTable("TipoMovimientoStock");
            entity.HasKey(e => e.IdTipoMovimiento);
            entity.Property(e => e.IdTipoMovimiento).HasColumnName("id_tipo_movimiento");
            entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(50).IsUnicode(false).IsRequired();
            entity.HasIndex(e => e.Nombre).IsUnique();

            // Estos 4 sí están enumerados literalmente en la sección 5 de tu prompt.
            entity.HasData(
                new TipoMovimientoStock { IdTipoMovimiento = 1, Nombre = "Ingreso" },
                new TipoMovimientoStock { IdTipoMovimiento = 2, Nombre = "Egreso" },
                new TipoMovimientoStock { IdTipoMovimiento = 3, Nombre = "Devolución" },
                new TipoMovimientoStock { IdTipoMovimiento = 4, Nombre = "Compra" }
            );
        });

        // ---------- Repuesto ----------
        modelBuilder.Entity<Repuesto>(entity =>
        {
            entity.ToTable("Repuesto");
            entity.HasKey(e => e.IdRepuesto);
            entity.Property(e => e.IdRepuesto).HasColumnName("id_repuesto");
            entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(150).IsUnicode(false).IsRequired();
            entity.Property(e => e.Categoria).HasColumnName("categoria").HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.Proveedor).HasColumnName("proveedor").HasMaxLength(150).IsUnicode(false);
            entity.Property(e => e.PrecioUnitario).HasColumnName("precio_unitario").HasPrecision(18, 2);
            entity.Property(e => e.StockActual).HasColumnName("stock_actual").IsRequired();
            entity.Property(e => e.StockMinimo).HasColumnName("stock_minimo");
            entity.Property(e => e.Activo).HasColumnName("activo");
            entity.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion").HasColumnType("date");
        });

        
        modelBuilder.Entity<MovimientoStock>(entity =>
        {
            entity.ToTable("MovimientoStock");
            entity.HasKey(e => e.IdMovimientoStock);
            entity.Property(e => e.IdMovimientoStock).HasColumnName("id_movimientostock");
            entity.Property(e => e.IdRepuesto).HasColumnName("id_repuesto");
            entity.Property(e => e.IdTipoMovimiento).HasColumnName("id_tipo_movimiento");
            entity.Property(e => e.Cantidad).HasColumnName("cantidad").IsRequired();
            entity.Property(e => e.Fecha).HasColumnName("fecha").HasColumnType("date").IsRequired();
            entity.Property(e => e.Observacion).HasColumnName("observacion").HasMaxLength(500).IsUnicode(false);

            entity.HasOne(e => e.Repuesto)
                  .WithMany(r => r.MovimientosStock)
                  .HasForeignKey(e => e.IdRepuesto)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.TipoMovimiento)
                  .WithMany(tm => tm.MovimientosStock)
                  .HasForeignKey(e => e.IdTipoMovimiento)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        
        modelBuilder.Entity<Turno>(entity =>
        {
            entity.ToTable("Turno");
            entity.HasKey(e => e.IdTurno);
            entity.Property(e => e.IdTurno).HasColumnName("id_turno");
            entity.Property(e => e.IdCliente).HasColumnName("id_cliente");
            entity.Property(e => e.IdVehiculo).HasColumnName("id_vehiculo");
            entity.Property(e => e.IdEstadoTurno).HasColumnName("id_estado_turno");
            entity.Property(e => e.Fecha).HasColumnName("fecha").HasColumnType("date").IsRequired();
            entity.Property(e => e.Motivo).HasColumnName("motivo").HasMaxLength(300).IsUnicode(false).IsRequired();
            entity.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion").HasColumnType("date");

            entity.HasOne(e => e.Cliente)
                  .WithMany(c => c.Turnos)
                  .HasForeignKey(e => e.IdCliente)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Vehiculo)
                  .WithMany(v => v.Turnos)
                  .HasForeignKey(e => e.IdVehiculo)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.EstadoTurno)
                  .WithMany(et => et.Turnos)
                  .HasForeignKey(e => e.IdEstadoTurno)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        
        modelBuilder.Entity<OrdenTrabajo>(entity =>
        {
            entity.ToTable("OrdenTrabajo");
            entity.HasKey(e => e.IdOrdenTrabajo);
            entity.Property(e => e.IdOrdenTrabajo).HasColumnName("id_ordentrabajo");
            entity.Property(e => e.IdCliente).HasColumnName("id_cliente");
            entity.Property(e => e.IdVehiculo).HasColumnName("id_vehiculo");
            entity.Property(e => e.IdTurno).HasColumnName("id_turno");
            entity.Property(e => e.IdEstadoOrden).HasColumnName("id_estado_orden");
            entity.Property(e => e.MotivoIngreso).HasColumnName("motivo_ingreso").HasMaxLength(500).IsUnicode(false).IsRequired();
            entity.Property(e => e.DiagnosticoInicial).HasColumnName("diagnosticoinicial").HasMaxLength(1000).IsUnicode(false).IsRequired();
            entity.Property(e => e.Observaciones).HasColumnName("observaciones").HasMaxLength(1000).IsUnicode(false);
            entity.Property(e => e.FechaIngreso).HasColumnName("fecha_ingreso").HasColumnType("date").IsRequired();
            entity.Property(e => e.FechaEntregaEstimada).HasColumnName("fecha_entregaestimada").HasColumnType("date");
            entity.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion").HasColumnType("date");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion").HasColumnType("date");

            entity.HasOne(e => e.Cliente)
                  .WithMany(c => c.OrdenesTrabajo)
                  .HasForeignKey(e => e.IdCliente)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Vehiculo)
                  .WithMany(v => v.OrdenesTrabajo)
                  .HasForeignKey(e => e.IdVehiculo)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Turno)
                  .WithMany(t => t.OrdenesTrabajo)
                  .HasForeignKey(e => e.IdTurno)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.EstadoOrden)
                  .WithMany(eo => eo.OrdenesTrabajo)
                  .HasForeignKey(e => e.IdEstadoOrden)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        
        modelBuilder.Entity<AvanceTrabajo>(entity =>
        {
            entity.ToTable("AvanceTrabajo", tb => tb.HasCheckConstraint(
                "CK_AvanceTrabajo_Etapa",
                "[etapa] IN ('Diagnóstico', 'Reparación', 'Espera de repuestos', 'Finalización')"));
            entity.HasKey(e => e.IdAvance);
            entity.Property(e => e.IdAvance).HasColumnName("id_avance");
            entity.Property(e => e.IdOrdenTrabajo).HasColumnName("id_orden_trabajo");
            entity.Property(e => e.Etapa).HasColumnName("etapa").HasMaxLength(100).IsUnicode(false).IsRequired();
            entity.Property(e => e.Descripcion).HasColumnName("descripcion").HasMaxLength(1000).IsUnicode(false).IsRequired();
            entity.Property(e => e.Fecha).HasColumnName("fecha").HasColumnType("date").IsRequired();

            entity.HasOne(e => e.OrdenTrabajo)
                  .WithMany(ot => ot.AvancesTrabajo)
                  .HasForeignKey(e => e.IdOrdenTrabajo)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        
        modelBuilder.Entity<Presupuesto>(entity =>
        {
            entity.ToTable("Presupuesto");
            entity.HasKey(e => e.IdPresupuesto);
            entity.Property(e => e.IdPresupuesto).HasColumnName("id_presupuesto");
            entity.Property(e => e.IdOrdenTrabajo).HasColumnName("id_orden_trabajo");
            entity.Property(e => e.IdEstadoPresupuesto).HasColumnName("id_estado_presupuesto");
            entity.Property(e => e.Subtotal).HasColumnName("subtotal").HasPrecision(18, 2);
            entity.Property(e => e.Total).HasColumnName("total").HasPrecision(18, 2);
            entity.Property(e => e.Fecha).HasColumnName("fecha").HasColumnType("date").IsRequired();
            entity.Property(e => e.Observaciones).HasColumnName("observaciones").HasMaxLength(500).IsUnicode(false);

            entity.HasOne(e => e.OrdenTrabajo)
                  .WithMany(ot => ot.Presupuestos)
                  .HasForeignKey(e => e.IdOrdenTrabajo)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.EstadoPresupuesto)
                  .WithMany(ep => ep.Presupuestos)
                  .HasForeignKey(e => e.IdEstadoPresupuesto)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        
        modelBuilder.Entity<DetallePresupuesto>(entity =>
        {
            entity.ToTable("DetallePresupuesto");
            entity.HasKey(e => e.IdDetallePresupuesto);
            entity.Property(e => e.IdDetallePresupuesto).HasColumnName("id_detallepresupuesto");
            entity.Property(e => e.IdPresupuesto).HasColumnName("id_presupuesto");
            entity.Property(e => e.IdRepuesto).HasColumnName("id_repuesto");
            entity.Property(e => e.Cantidad).HasColumnName("cantidad").IsRequired();
            entity.Property(e => e.PrecioUnitario).HasColumnName("precio_unitario").HasPrecision(18, 2).IsRequired();
            entity.Property(e => e.Subtotal).HasColumnName("subtotal").HasPrecision(18, 2).IsRequired();

            entity.HasOne(e => e.Presupuesto)
                  .WithMany(p => p.Detalles)
                  .HasForeignKey(e => e.IdPresupuesto)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Repuesto)
                  .WithMany(r => r.DetallesPresupuesto)
                  .HasForeignKey(e => e.IdRepuesto)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        
        modelBuilder.Entity<Factura>(entity =>
        {
            entity.ToTable("Factura");
            entity.HasKey(e => e.IdFactura);
            entity.Property(e => e.IdFactura).HasColumnName("id_factura");
            entity.Property(e => e.IdPresupuesto).HasColumnName("id_presupuesto");
            entity.Property(e => e.IdEstadoFactura).HasColumnName("id_estado_factura");
            entity.Property(e => e.NumeroFactura).HasColumnName("numero_factura").HasMaxLength(50).IsUnicode(false).IsRequired();
            entity.Property(e => e.Subtotal).HasColumnName("subtotal").HasPrecision(18, 2);
            entity.Property(e => e.Iva).HasColumnName("iva").HasPrecision(18, 2);
            entity.Property(e => e.Total).HasColumnName("total").HasPrecision(18, 2);
            entity.Property(e => e.FechaEmision).HasColumnName("fecha_emision").HasColumnType("date");

            entity.HasIndex(e => e.NumeroFactura).IsUnique();

            entity.HasOne(e => e.Presupuesto)
                  .WithMany(p => p.Facturas)
                  .HasForeignKey(e => e.IdPresupuesto)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.EstadoFactura)
                  .WithMany(ef => ef.Facturas)
                  .HasForeignKey(e => e.IdEstadoFactura)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        
        modelBuilder.Entity<DetalleFactura>(entity =>
        {
            entity.ToTable("DetalleFactura");
            entity.HasKey(e => e.IdDetalleFactura);
            entity.Property(e => e.IdDetalleFactura).HasColumnName("id_detallefactura");
            entity.Property(e => e.IdFactura).HasColumnName("id_factura");
            entity.Property(e => e.IdRepuesto).HasColumnName("id_repuesto");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion").HasMaxLength(300).IsUnicode(false);
            entity.Property(e => e.Cantidad).HasColumnName("cantidad");
            entity.Property(e => e.PrecioUnitario).HasColumnName("precio_unitario").HasPrecision(18, 2);
            entity.Property(e => e.Subtotal).HasColumnName("subtotal").HasPrecision(18, 2);

            entity.HasOne(e => e.Factura)
                  .WithMany(f => f.Detalles)
                  .HasForeignKey(e => e.IdFactura)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Repuesto)
                  .WithMany(r => r.DetallesFactura)
                  .HasForeignKey(e => e.IdRepuesto)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        
        modelBuilder.Entity<Pago>(entity =>
        {
            entity.ToTable("Pago");
            entity.HasKey(e => e.IdPago);
            entity.Property(e => e.IdPago).HasColumnName("id_pago");
            entity.Property(e => e.IdFactura).HasColumnName("id_factura");
            entity.Property(e => e.Fecha).HasColumnName("fecha").HasColumnType("date").IsRequired();
            entity.Property(e => e.Monto).HasColumnName("monto").HasPrecision(18, 2).IsRequired();
            entity.Property(e => e.MedioPago).HasColumnName("medio_pago").HasMaxLength(50).IsUnicode(false).IsRequired();

            entity.HasOne(e => e.Factura)
                  .WithMany(f => f.Pagos)
                  .HasForeignKey(e => e.IdFactura)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}