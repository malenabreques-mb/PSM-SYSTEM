using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PSMSystem.Migrations
{
    /// <inheritdoc />
    public partial class BusquedaSinTildes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "modelo",
                table: "Vehiculo",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: false,
                collation: "Latin1_General_CI_AI",
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldUnicode: false,
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "marca",
                table: "Vehiculo",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: false,
                collation: "Latin1_General_CI_AI",
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldUnicode: false,
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "motivo",
                table: "Turno",
                type: "varchar(300)",
                unicode: false,
                maxLength: 300,
                nullable: false,
                collation: "Latin1_General_CI_AI",
                oldClrType: typeof(string),
                oldType: "varchar(300)",
                oldUnicode: false,
                oldMaxLength: 300);

            migrationBuilder.AlterColumn<string>(
                name: "proveedor",
                table: "Repuesto",
                type: "varchar(150)",
                unicode: false,
                maxLength: 150,
                nullable: true,
                collation: "Latin1_General_CI_AI",
                oldClrType: typeof(string),
                oldType: "varchar(150)",
                oldUnicode: false,
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "nombre",
                table: "Repuesto",
                type: "varchar(150)",
                unicode: false,
                maxLength: 150,
                nullable: false,
                collation: "Latin1_General_CI_AI",
                oldClrType: typeof(string),
                oldType: "varchar(150)",
                oldUnicode: false,
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "categoria",
                table: "Repuesto",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: true,
                collation: "Latin1_General_CI_AI",
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldUnicode: false,
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "motivo_ingreso",
                table: "OrdenTrabajo",
                type: "varchar(500)",
                unicode: false,
                maxLength: 500,
                nullable: false,
                collation: "Latin1_General_CI_AI",
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldUnicode: false,
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "nombre",
                table: "Cliente",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: false,
                collation: "Latin1_General_CI_AI",
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldUnicode: false,
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "direccion",
                table: "Cliente",
                type: "varchar(200)",
                unicode: false,
                maxLength: 200,
                nullable: true,
                collation: "Latin1_General_CI_AI",
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldUnicode: false,
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "apellido",
                table: "Cliente",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: false,
                collation: "Latin1_General_CI_AI",
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldUnicode: false,
                oldMaxLength: 100);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "modelo",
                table: "Vehiculo",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldUnicode: false,
                oldMaxLength: 100,
                oldCollation: "Latin1_General_CI_AI");

            migrationBuilder.AlterColumn<string>(
                name: "marca",
                table: "Vehiculo",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldUnicode: false,
                oldMaxLength: 100,
                oldCollation: "Latin1_General_CI_AI");

            migrationBuilder.AlterColumn<string>(
                name: "motivo",
                table: "Turno",
                type: "varchar(300)",
                unicode: false,
                maxLength: 300,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(300)",
                oldUnicode: false,
                oldMaxLength: 300,
                oldCollation: "Latin1_General_CI_AI");

            migrationBuilder.AlterColumn<string>(
                name: "proveedor",
                table: "Repuesto",
                type: "varchar(150)",
                unicode: false,
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(150)",
                oldUnicode: false,
                oldMaxLength: 150,
                oldNullable: true,
                oldCollation: "Latin1_General_CI_AI");

            migrationBuilder.AlterColumn<string>(
                name: "nombre",
                table: "Repuesto",
                type: "varchar(150)",
                unicode: false,
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(150)",
                oldUnicode: false,
                oldMaxLength: 150,
                oldCollation: "Latin1_General_CI_AI");

            migrationBuilder.AlterColumn<string>(
                name: "categoria",
                table: "Repuesto",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldUnicode: false,
                oldMaxLength: 100,
                oldNullable: true,
                oldCollation: "Latin1_General_CI_AI");

            migrationBuilder.AlterColumn<string>(
                name: "motivo_ingreso",
                table: "OrdenTrabajo",
                type: "varchar(500)",
                unicode: false,
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldUnicode: false,
                oldMaxLength: 500,
                oldCollation: "Latin1_General_CI_AI");

            migrationBuilder.AlterColumn<string>(
                name: "nombre",
                table: "Cliente",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldUnicode: false,
                oldMaxLength: 100,
                oldCollation: "Latin1_General_CI_AI");

            migrationBuilder.AlterColumn<string>(
                name: "direccion",
                table: "Cliente",
                type: "varchar(200)",
                unicode: false,
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldUnicode: false,
                oldMaxLength: 200,
                oldNullable: true,
                oldCollation: "Latin1_General_CI_AI");

            migrationBuilder.AlterColumn<string>(
                name: "apellido",
                table: "Cliente",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldUnicode: false,
                oldMaxLength: 100,
                oldCollation: "Latin1_General_CI_AI");
        }
    }
}
