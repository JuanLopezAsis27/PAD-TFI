using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ArcaSim.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Contribuyentes",
                columns: table => new
                {
                    CUIT = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    NombreORazonSocial = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TipoPersona = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CondicionIva = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contribuyentes", x => x.CUIT);
                });

            migrationBuilder.CreateTable(
                name: "PuntosVenta",
                columns: table => new
                {
                    CUITContribuyente = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    Numero = table.Column<int>(type: "integer", nullable: false),
                    Habilitado = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PuntosVenta", x => new { x.CUITContribuyente, x.Numero });
                    table.ForeignKey(
                        name: "FK_PuntosVenta_Contribuyentes_CUITContribuyente",
                        column: x => x.CUITContribuyente,
                        principalTable: "Contribuyentes",
                        principalColumn: "CUIT",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Comprobantes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CUITEmisor = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    Concepto = table.Column<int>(type: "integer", nullable: false),
                    PuntoVentaNumero = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Numero = table.Column<long>(type: "bigint", nullable: false),
                    Fecha = table.Column<DateTime>(type: "date", nullable: false),
                    FechaServicioDesde = table.Column<DateTime>(type: "date", nullable: true),
                    FechaServicioHasta = table.Column<DateTime>(type: "date", nullable: true),
                    FechaVencimientoPago = table.Column<DateTime>(type: "date", nullable: true),
                    TipoDocumentoReceptor = table.Column<int>(type: "integer", nullable: false),
                    NumeroDocumentoReceptor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CondicionIvaReceptor = table.Column<int>(type: "integer", nullable: true),
                    ImporteNeto = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    ImporteNoGravado = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    ImporteExento = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    ImporteTributos = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    ImporteIVA = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    ImporteTotal = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    Moneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    CancelaEnMismaMonedaExtranjera = table.Column<bool>(type: "boolean", nullable: false),
                    Cotizacion = table.Column<decimal>(type: "numeric(19,6)", precision: 19, scale: 6, nullable: true),
                    CAE = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    FechaVencimientoCAE = table.Column<DateTime>(type: "date", nullable: true),
                    EstadoAutorizacion = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ObservacionesAutorizacion = table.Column<string[]>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comprobantes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comprobantes_PuntosVenta_CUITEmisor_PuntoVentaNumero",
                        columns: x => new { x.CUITEmisor, x.PuntoVentaNumero },
                        principalTable: "PuntosVenta",
                        principalColumns: new[] { "CUITContribuyente", "Numero" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ComprobantesIva",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Alicuota = table.Column<int>(type: "integer", nullable: false),
                    BaseImponible = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    ImporteIVA = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    ComprobanteId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComprobantesIva", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComprobantesIva_Comprobantes_ComprobanteId",
                        column: x => x.ComprobanteId,
                        principalTable: "Comprobantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comprobantes_CUITEmisor_PuntoVentaNumero_Tipo_Numero",
                table: "Comprobantes",
                columns: new[] { "CUITEmisor", "PuntoVentaNumero", "Tipo", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComprobantesIva_ComprobanteId",
                table: "ComprobantesIva",
                column: "ComprobanteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComprobantesIva");

            migrationBuilder.DropTable(
                name: "Comprobantes");

            migrationBuilder.DropTable(
                name: "PuntosVenta");

            migrationBuilder.DropTable(
                name: "Contribuyentes");
        }
    }
}
