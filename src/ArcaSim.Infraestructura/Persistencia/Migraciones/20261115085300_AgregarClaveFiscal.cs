using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArcaSim.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AgregarClaveFiscal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Cuit = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Apellido = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    HashClave = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FechaAlta = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UltimoIngreso = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IntentosFallidos = table.Column<int>(type: "integer", nullable: false),
                    BloqueadoHasta = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Cuit);
                });

            migrationBuilder.CreateTable(
                name: "Representaciones",
                columns: table => new
                {
                    CuitUsuario = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    CuitRepresentado = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    FechaAlta = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Representaciones", x => new { x.CuitUsuario, x.CuitRepresentado });
                    table.ForeignKey(
                        name: "FK_Representaciones_Contribuyentes_CuitRepresentado",
                        column: x => x.CuitRepresentado,
                        principalTable: "Contribuyentes",
                        principalColumn: "CUIT",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Representaciones_Usuarios_CuitUsuario",
                        column: x => x.CuitUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "Cuit",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Representaciones_CuitRepresentado",
                table: "Representaciones",
                column: "CuitRepresentado");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Representaciones");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
