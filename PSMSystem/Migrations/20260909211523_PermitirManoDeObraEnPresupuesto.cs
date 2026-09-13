using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PSMSystem.Migrations
{
    /// <inheritdoc />
    public partial class PermitirManoDeObraEnPresupuesto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "id_repuesto",
                table: "DetallePresupuesto",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "descripcion",
                table: "DetallePresupuesto",
                type: "varchar(300)",
                unicode: false,
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tipo_item",
                table: "DetallePresupuesto",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "EstadoPresupuesto",
                keyColumn: "id_estado_presupuesto",
                keyValue: 1,
                column: "nombre",
                value: "En elaboración");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DetallePresupuesto_TipoItem",
                table: "DetallePresupuesto",
                sql: "[tipo_item] IN ('Repuesto', 'Mano de obra')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_DetallePresupuesto_TipoItem",
                table: "DetallePresupuesto");

            migrationBuilder.DropColumn(
                name: "descripcion",
                table: "DetallePresupuesto");

            migrationBuilder.DropColumn(
                name: "tipo_item",
                table: "DetallePresupuesto");

            migrationBuilder.AlterColumn<int>(
                name: "id_repuesto",
                table: "DetallePresupuesto",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "EstadoPresupuesto",
                keyColumn: "id_estado_presupuesto",
                keyValue: 1,
                column: "nombre",
                value: "Pendiente");
        }
    }
}
