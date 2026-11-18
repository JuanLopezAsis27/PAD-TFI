using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ArcaSim.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AgregarApiKeysSoap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "api_keys_soap",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    cuit = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    revocada = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_revocacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_api_keys_soap", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_api_keys_soap_cuit",
                table: "api_keys_soap",
                column: "cuit");

            migrationBuilder.CreateIndex(
                name: "ux_api_keys_soap_hash",
                table: "api_keys_soap",
                column: "hash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "api_keys_soap");
        }
    }
}
