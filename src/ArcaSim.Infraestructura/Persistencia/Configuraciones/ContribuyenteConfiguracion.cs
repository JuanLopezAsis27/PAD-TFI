using ArcaSim.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArcaSim.Infraestructura.Persistencia.Configuraciones;

public class ContribuyenteConfiguracion : IEntityTypeConfiguration<Contribuyente>
{
    public void Configure(EntityTypeBuilder<Contribuyente> builder)
    {
        builder.ToTable("Contribuyentes");

        builder.HasKey(c => c.CUIT);
        builder.Property(c => c.CUIT).HasMaxLength(11);

        builder.Property(c => c.NombreORazonSocial).HasMaxLength(200).IsRequired();

        // Los enums sin ID de ARCA se guardan como texto; CondicionIva se guarda con su ID de ARCA.
        builder.Property(c => c.TipoPersona).HasConversion<string>().HasMaxLength(20);

        builder.HasMany(c => c.PuntosVenta)
            .WithOne(p => p.Contribuyente)
            .HasForeignKey(p => p.CUITContribuyente)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
