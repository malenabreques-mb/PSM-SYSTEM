using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PSMSystem.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cliente",
                columns: table => new
                {
                    id_cliente = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    apellido = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    telefono = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    direccion = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    activo = table.Column<bool>(type: "bit", nullable: false),
                    fecha_creacion = table.Column<DateOnly>(type: "date", nullable: true),
                    fecha_modificacion = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cliente", x => x.id_cliente);
                });

            migrationBuilder.CreateTable(
                name: "EstadoFactura",
                columns: table => new
                {
                    id_estado_factura = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadoFactura", x => x.id_estado_factura);
                });

            migrationBuilder.CreateTable(
                name: "EstadoOrdenTrabajo",
                columns: table => new
                {
                    id_estado_orden = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadoOrdenTrabajo", x => x.id_estado_orden);
                });

            migrationBuilder.CreateTable(
                name: "EstadoPresupuesto",
                columns: table => new
                {
                    id_estado_presupuesto = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadoPresupuesto", x => x.id_estado_presupuesto);
                });

            migrationBuilder.CreateTable(
                name: "EstadoTurno",
                columns: table => new
                {
                    id_estado_turno = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadoTurno", x => x.id_estado_turno);
                });

            migrationBuilder.CreateTable(
                name: "Repuesto",
                columns: table => new
                {
                    id_repuesto = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: false),
                    categoria = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    proveedor = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: true),
                    precio_unitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    stock_actual = table.Column<int>(type: "int", nullable: false),
                    stock_minimo = table.Column<int>(type: "int", nullable: true),
                    activo = table.Column<bool>(type: "bit", nullable: true),
                    fecha_creacion = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Repuesto", x => x.id_repuesto);
                });

            migrationBuilder.CreateTable(
                name: "TipoMovimientoStock",
                columns: table => new
                {
                    id_tipo_movimiento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoMovimientoStock", x => x.id_tipo_movimiento);
                });

            migrationBuilder.CreateTable(
                name: "Vehiculo",
                columns: table => new
                {
                    id_vehiculo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_cliente = table.Column<int>(type: "int", nullable: false),
                    marca = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    modelo = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    anio = table.Column<int>(type: "int", nullable: true),
                    patente = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    activo = table.Column<bool>(type: "bit", nullable: false),
                    fecha_creacion = table.Column<DateOnly>(type: "date", nullable: true),
                    fecha_modificacion = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehiculo", x => x.id_vehiculo);
                    table.ForeignKey(
                        name: "FK_Vehiculo_Cliente_id_cliente",
                        column: x => x.id_cliente,
                        principalTable: "Cliente",
                        principalColumn: "id_cliente",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MovimientoStock",
                columns: table => new
                {
                    id_movimientostock = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_repuesto = table.Column<int>(type: "int", nullable: false),
                    id_tipo_movimiento = table.Column<int>(type: "int", nullable: false),
                    cantidad = table.Column<int>(type: "int", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    observacion = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientoStock", x => x.id_movimientostock);
                    table.ForeignKey(
                        name: "FK_MovimientoStock_Repuesto_id_repuesto",
                        column: x => x.id_repuesto,
                        principalTable: "Repuesto",
                        principalColumn: "id_repuesto",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientoStock_TipoMovimientoStock_id_tipo_movimiento",
                        column: x => x.id_tipo_movimiento,
                        principalTable: "TipoMovimientoStock",
                        principalColumn: "id_tipo_movimiento",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Turno",
                columns: table => new
                {
                    id_turno = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_cliente = table.Column<int>(type: "int", nullable: false),
                    id_vehiculo = table.Column<int>(type: "int", nullable: false),
                    id_estado_turno = table.Column<int>(type: "int", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    motivo = table.Column<string>(type: "varchar(300)", unicode: false, maxLength: 300, nullable: false),
                    fecha_creacion = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Turno", x => x.id_turno);
                    table.ForeignKey(
                        name: "FK_Turno_Cliente_id_cliente",
                        column: x => x.id_cliente,
                        principalTable: "Cliente",
                        principalColumn: "id_cliente",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Turno_EstadoTurno_id_estado_turno",
                        column: x => x.id_estado_turno,
                        principalTable: "EstadoTurno",
                        principalColumn: "id_estado_turno",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Turno_Vehiculo_id_vehiculo",
                        column: x => x.id_vehiculo,
                        principalTable: "Vehiculo",
                        principalColumn: "id_vehiculo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrdenTrabajo",
                columns: table => new
                {
                    id_ordentrabajo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_cliente = table.Column<int>(type: "int", nullable: false),
                    id_vehiculo = table.Column<int>(type: "int", nullable: false),
                    id_turno = table.Column<int>(type: "int", nullable: true),
                    id_estado_orden = table.Column<int>(type: "int", nullable: false),
                    motivo_ingreso = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: false),
                    diagnosticoinicial = table.Column<string>(type: "varchar(1000)", unicode: false, maxLength: 1000, nullable: false),
                    observaciones = table.Column<string>(type: "varchar(1000)", unicode: false, maxLength: 1000, nullable: true),
                    fecha_ingreso = table.Column<DateOnly>(type: "date", nullable: false),
                    fecha_entregaestimada = table.Column<DateOnly>(type: "date", nullable: true),
                    fecha_creacion = table.Column<DateOnly>(type: "date", nullable: true),
                    fecha_modificacion = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenTrabajo", x => x.id_ordentrabajo);
                    table.ForeignKey(
                        name: "FK_OrdenTrabajo_Cliente_id_cliente",
                        column: x => x.id_cliente,
                        principalTable: "Cliente",
                        principalColumn: "id_cliente",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdenTrabajo_EstadoOrdenTrabajo_id_estado_orden",
                        column: x => x.id_estado_orden,
                        principalTable: "EstadoOrdenTrabajo",
                        principalColumn: "id_estado_orden",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdenTrabajo_Turno_id_turno",
                        column: x => x.id_turno,
                        principalTable: "Turno",
                        principalColumn: "id_turno",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdenTrabajo_Vehiculo_id_vehiculo",
                        column: x => x.id_vehiculo,
                        principalTable: "Vehiculo",
                        principalColumn: "id_vehiculo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AvanceTrabajo",
                columns: table => new
                {
                    id_avance = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_orden_trabajo = table.Column<int>(type: "int", nullable: false),
                    etapa = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    descripcion = table.Column<string>(type: "varchar(1000)", unicode: false, maxLength: 1000, nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvanceTrabajo", x => x.id_avance);
                    table.CheckConstraint("CK_AvanceTrabajo_Etapa", "[etapa] IN ('Diagnóstico', 'Reparación', 'Espera de repuestos', 'Finalización')");
                    table.ForeignKey(
                        name: "FK_AvanceTrabajo_OrdenTrabajo_id_orden_trabajo",
                        column: x => x.id_orden_trabajo,
                        principalTable: "OrdenTrabajo",
                        principalColumn: "id_ordentrabajo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Presupuesto",
                columns: table => new
                {
                    id_presupuesto = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_orden_trabajo = table.Column<int>(type: "int", nullable: false),
                    id_estado_presupuesto = table.Column<int>(type: "int", nullable: false),
                    subtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    observaciones = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Presupuesto", x => x.id_presupuesto);
                    table.ForeignKey(
                        name: "FK_Presupuesto_EstadoPresupuesto_id_estado_presupuesto",
                        column: x => x.id_estado_presupuesto,
                        principalTable: "EstadoPresupuesto",
                        principalColumn: "id_estado_presupuesto",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Presupuesto_OrdenTrabajo_id_orden_trabajo",
                        column: x => x.id_orden_trabajo,
                        principalTable: "OrdenTrabajo",
                        principalColumn: "id_ordentrabajo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetallePresupuesto",
                columns: table => new
                {
                    id_detallepresupuesto = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_presupuesto = table.Column<int>(type: "int", nullable: false),
                    id_repuesto = table.Column<int>(type: "int", nullable: false),
                    cantidad = table.Column<int>(type: "int", nullable: false),
                    precio_unitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    subtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallePresupuesto", x => x.id_detallepresupuesto);
                    table.ForeignKey(
                        name: "FK_DetallePresupuesto_Presupuesto_id_presupuesto",
                        column: x => x.id_presupuesto,
                        principalTable: "Presupuesto",
                        principalColumn: "id_presupuesto",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetallePresupuesto_Repuesto_id_repuesto",
                        column: x => x.id_repuesto,
                        principalTable: "Repuesto",
                        principalColumn: "id_repuesto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Factura",
                columns: table => new
                {
                    id_factura = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_presupuesto = table.Column<int>(type: "int", nullable: false),
                    id_estado_factura = table.Column<int>(type: "int", nullable: false),
                    numero_factura = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    subtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    iva = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    fecha_emision = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Factura", x => x.id_factura);
                    table.ForeignKey(
                        name: "FK_Factura_EstadoFactura_id_estado_factura",
                        column: x => x.id_estado_factura,
                        principalTable: "EstadoFactura",
                        principalColumn: "id_estado_factura",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Factura_Presupuesto_id_presupuesto",
                        column: x => x.id_presupuesto,
                        principalTable: "Presupuesto",
                        principalColumn: "id_presupuesto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetalleFactura",
                columns: table => new
                {
                    id_detallefactura = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_factura = table.Column<int>(type: "int", nullable: false),
                    id_repuesto = table.Column<int>(type: "int", nullable: true),
                    descripcion = table.Column<string>(type: "varchar(300)", unicode: false, maxLength: 300, nullable: true),
                    cantidad = table.Column<int>(type: "int", nullable: true),
                    precio_unitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    subtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetalleFactura", x => x.id_detallefactura);
                    table.ForeignKey(
                        name: "FK_DetalleFactura_Factura_id_factura",
                        column: x => x.id_factura,
                        principalTable: "Factura",
                        principalColumn: "id_factura",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetalleFactura_Repuesto_id_repuesto",
                        column: x => x.id_repuesto,
                        principalTable: "Repuesto",
                        principalColumn: "id_repuesto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pago",
                columns: table => new
                {
                    id_pago = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_factura = table.Column<int>(type: "int", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    monto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    medio_pago = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pago", x => x.id_pago);
                    table.ForeignKey(
                        name: "FK_Pago_Factura_id_factura",
                        column: x => x.id_factura,
                        principalTable: "Factura",
                        principalColumn: "id_factura",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "EstadoFactura",
                columns: new[] { "id_estado_factura", "nombre" },
                values: new object[,]
                {
                    { 1, "Pendiente" },
                    { 2, "Pagada" },
                    { 3, "Vencida" }
                });

            migrationBuilder.InsertData(
                table: "EstadoOrdenTrabajo",
                columns: new[] { "id_estado_orden", "nombre" },
                values: new object[,]
                {
                    { 1, "Pendiente" },
                    { 2, "En proceso" },
                    { 3, "Esperando repuestos" },
                    { 4, "Finalizada" }
                });

            migrationBuilder.InsertData(
                table: "EstadoPresupuesto",
                columns: new[] { "id_estado_presupuesto", "nombre" },
                values: new object[,]
                {
                    { 1, "Pendiente" },
                    { 2, "Aprobado" },
                    { 3, "Rechazado" },
                    { 4, "Facturado" }
                });

            migrationBuilder.InsertData(
                table: "EstadoTurno",
                columns: new[] { "id_estado_turno", "nombre" },
                values: new object[,]
                {
                    { 1, "Pendiente" },
                    { 2, "En progreso" },
                    { 3, "Finalizado" }
                });

            migrationBuilder.InsertData(
                table: "TipoMovimientoStock",
                columns: new[] { "id_tipo_movimiento", "nombre" },
                values: new object[,]
                {
                    { 1, "Ingreso" },
                    { 2, "Egreso" },
                    { 3, "Devolución" },
                    { 4, "Compra" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AvanceTrabajo_id_orden_trabajo",
                table: "AvanceTrabajo",
                column: "id_orden_trabajo");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleFactura_id_factura",
                table: "DetalleFactura",
                column: "id_factura");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleFactura_id_repuesto",
                table: "DetalleFactura",
                column: "id_repuesto");

            migrationBuilder.CreateIndex(
                name: "IX_DetallePresupuesto_id_presupuesto",
                table: "DetallePresupuesto",
                column: "id_presupuesto");

            migrationBuilder.CreateIndex(
                name: "IX_DetallePresupuesto_id_repuesto",
                table: "DetallePresupuesto",
                column: "id_repuesto");

            migrationBuilder.CreateIndex(
                name: "IX_EstadoFactura_nombre",
                table: "EstadoFactura",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EstadoOrdenTrabajo_nombre",
                table: "EstadoOrdenTrabajo",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EstadoPresupuesto_nombre",
                table: "EstadoPresupuesto",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EstadoTurno_nombre",
                table: "EstadoTurno",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Factura_id_estado_factura",
                table: "Factura",
                column: "id_estado_factura");

            migrationBuilder.CreateIndex(
                name: "IX_Factura_id_presupuesto",
                table: "Factura",
                column: "id_presupuesto");

            migrationBuilder.CreateIndex(
                name: "IX_Factura_numero_factura",
                table: "Factura",
                column: "numero_factura",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovimientoStock_id_repuesto",
                table: "MovimientoStock",
                column: "id_repuesto");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientoStock_id_tipo_movimiento",
                table: "MovimientoStock",
                column: "id_tipo_movimiento");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenTrabajo_id_cliente",
                table: "OrdenTrabajo",
                column: "id_cliente");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenTrabajo_id_estado_orden",
                table: "OrdenTrabajo",
                column: "id_estado_orden");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenTrabajo_id_turno",
                table: "OrdenTrabajo",
                column: "id_turno");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenTrabajo_id_vehiculo",
                table: "OrdenTrabajo",
                column: "id_vehiculo");

            migrationBuilder.CreateIndex(
                name: "IX_Pago_id_factura",
                table: "Pago",
                column: "id_factura");

            migrationBuilder.CreateIndex(
                name: "IX_Presupuesto_id_estado_presupuesto",
                table: "Presupuesto",
                column: "id_estado_presupuesto");

            migrationBuilder.CreateIndex(
                name: "IX_Presupuesto_id_orden_trabajo",
                table: "Presupuesto",
                column: "id_orden_trabajo");

            migrationBuilder.CreateIndex(
                name: "IX_TipoMovimientoStock_nombre",
                table: "TipoMovimientoStock",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Turno_id_cliente",
                table: "Turno",
                column: "id_cliente");

            migrationBuilder.CreateIndex(
                name: "IX_Turno_id_estado_turno",
                table: "Turno",
                column: "id_estado_turno");

            migrationBuilder.CreateIndex(
                name: "IX_Turno_id_vehiculo",
                table: "Turno",
                column: "id_vehiculo");

            migrationBuilder.CreateIndex(
                name: "IX_Vehiculo_id_cliente",
                table: "Vehiculo",
                column: "id_cliente");

            migrationBuilder.CreateIndex(
                name: "IX_Vehiculo_patente",
                table: "Vehiculo",
                column: "patente",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AvanceTrabajo");

            migrationBuilder.DropTable(
                name: "DetalleFactura");

            migrationBuilder.DropTable(
                name: "DetallePresupuesto");

            migrationBuilder.DropTable(
                name: "MovimientoStock");

            migrationBuilder.DropTable(
                name: "Pago");

            migrationBuilder.DropTable(
                name: "Repuesto");

            migrationBuilder.DropTable(
                name: "TipoMovimientoStock");

            migrationBuilder.DropTable(
                name: "Factura");

            migrationBuilder.DropTable(
                name: "EstadoFactura");

            migrationBuilder.DropTable(
                name: "Presupuesto");

            migrationBuilder.DropTable(
                name: "EstadoPresupuesto");

            migrationBuilder.DropTable(
                name: "OrdenTrabajo");

            migrationBuilder.DropTable(
                name: "EstadoOrdenTrabajo");

            migrationBuilder.DropTable(
                name: "Turno");

            migrationBuilder.DropTable(
                name: "EstadoTurno");

            migrationBuilder.DropTable(
                name: "Vehiculo");

            migrationBuilder.DropTable(
                name: "Cliente");
        }
    }
}
