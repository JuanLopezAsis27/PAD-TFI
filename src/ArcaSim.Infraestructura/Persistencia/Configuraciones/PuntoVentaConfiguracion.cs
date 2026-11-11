using ArcaSim.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArcaSim.Infraestructura.Persistencia.Configuraciones;

public class PuntoVentaConfiguracion : IEntityTypeConfiguration<PuntoVenta>
{
    public void Configure(EntityTypeBuilder<PuntoVenta> builder)
    {
        builder.ToTable("PuntosVenta");

        // Cada contribuyente numera sus puntos de venta desde 1.
        builder.HasKey(p => new { p.CUITContribuyente, p.Numero });
        builder.Property(p => p.CUITContribuyente).HasMaxLength(11);

        builder.HasMany(p => p.Comprobantes)
            .WithOne(c => c.PuntoVenta)
            .HasForeignKey(c => new { c.CUITEmisor, c.PuntoVentaNumero })
            .OnDelete(DeleteBehavior.Restrict);
    }
}
