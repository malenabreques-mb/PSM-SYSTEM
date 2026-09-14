using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PSMSystem.Migrations
{
    /// <inheritdoc />
    public partial class PrepararFacturacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Hora",
                table: "Turno",
                newName: "hora");

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "hora",
                table: "Turno",
                type: "time(0)",
                nullable: false,
                oldClrType: typeof(TimeOnly),
                oldType: "time");

            migrationBuilder.AddColumn<string>(
                name: "tipo_item",
                table: "DetalleFactura",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "EstadoFactura",
                keyColumn: "id_estado_factura",
                keyValue: 1,
                column: "nombre",
                value: "Pendiente de emisión");

            migrationBuilder.InsertData(
                table: "EstadoFactura",
                columns: new[] { "id_estado_factura", "nombre" },
                values: new object[] { 4, "Pendiente de pago" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_DetalleFactura_TipoItem",
                table: "DetalleFactura",
                sql: "[tipo_item] IN ('Repuesto', 'Mano de obra')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_DetalleFactura_TipoItem",
                table: "DetalleFactura");

            migrationBuilder.DeleteData(
                table: "EstadoFactura",
                keyColumn: "id_estado_factura",
                keyValue: 4);

            migrationBuilder.DropColumn(
                name: "tipo_item",
                table: "DetalleFactura");

            migrationBuilder.RenameColumn(
                name: "hora",
                table: "Turno",
                newName: "Hora");

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "Hora",
                table: "Turno",
                type: "time",
                nullable: false,
                oldClrType: typeof(TimeOnly),
                oldType: "time(0)");

            migrationBuilder.UpdateData(
                table: "EstadoFactura",
                keyColumn: "id_estado_factura",
                keyValue: 1,
                column: "nombre",
                value: "Pendiente");
        }
    }
}
