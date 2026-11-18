using ArcaSim.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArcaSim.Infraestructura.Persistencia.Configuraciones;

public class ComprobanteConfiguracion : IEntityTypeConfiguration<Comprobante>
{
    // ARCA admite importes de hasta 13 enteros y 2 decimales, y cotizaciones con 6 decimales.
    private const int PrecisionImporte = 15;
    private const int EscalaImporte = 2;

    public void Configure(EntityTypeBuilder<Comprobante> builder)
    {
        builder.ToTable("Comprobantes");

        // Clave interna: el dominio identifica al comprobante por emisor, punto de venta, tipo y número.
        builder.Property<long>("Id");
        builder.HasKey("Id");
        builder.HasIndex(c => new { c.CUITEmisor, c.PuntoVentaNumero, c.Tipo, c.Numero }).IsUnique();

        builder.Property(c => c.CUITEmisor).HasMaxLength(11);
        builder.Property(c => c.NumeroDocumentoReceptor).HasMaxLength(20);
        builder.Property(c => c.Moneda).HasMaxLength(3);
        builder.Property(c => c.CAE).HasMaxLength(14);

        builder.Property(c => c.Fecha).HasColumnType("date");
        builder.Property(c => c.FechaServicioDesde).HasColumnType("date");
        builder.Property(c => c.FechaServicioHasta).HasColumnType("date");
        builder.Property(c => c.FechaVencimientoPago).HasColumnType("date");
        builder.Property(c => c.FechaVencimientoCAE).HasColumnType("date");

        builder.Property(c => c.ImporteNeto).HasPrecision(PrecisionImporte, EscalaImporte);
        builder.Property(c => c.ImporteNoGravado).HasPrecision(PrecisionImporte, EscalaImporte);
        builder.Property(c => c.ImporteExento).HasPrecision(PrecisionImporte, EscalaImporte);
        builder.Property(c => c.ImporteTributos).HasPrecision(PrecisionImporte, EscalaImporte);
        builder.Property(c => c.ImporteIVA).HasPrecision(PrecisionImporte, EscalaImporte);
        builder.Property(c => c.ImporteTotal).HasPrecision(PrecisionImporte, EscalaImporte);
        builder.Property(c => c.Cotizacion).HasPrecision(19, 6);

        builder.Property(c => c.EstadoAutorizacion).HasConversion<string>().HasMaxLength(30);
        builder.PrimitiveCollection(c => c.ObservacionesAutorizacion);

        builder.OwnsMany(c => c.AlicuotasIva, iva =>
        {
            iva.ToTable("ComprobantesIva");
            iva.WithOwner().HasForeignKey("ComprobanteId");
            iva.Property<long>("Id");
            iva.HasKey("Id");
            iva.Property(i => i.BaseImponible).HasPrecision(PrecisionImporte, EscalaImporte);
            iva.Property(i => i.ImporteIVA).HasPrecision(PrecisionImporte, EscalaImporte);
        });
    }
}
