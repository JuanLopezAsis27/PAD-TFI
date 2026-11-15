using ArcaSim.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArcaSim.Infraestructura.Persistencia.Configuraciones;

public class RepresentacionConfiguracion : IEntityTypeConfiguration<Representacion>
{
    public void Configure(EntityTypeBuilder<Representacion> builder)
    {
        builder.ToTable("Representaciones");

        builder.HasKey(r => new { r.CuitUsuario, r.CuitRepresentado });
        builder.Property(r => r.CuitUsuario).HasMaxLength(11);
        builder.Property(r => r.CuitRepresentado).HasMaxLength(11);

        builder.Property(r => r.FechaAlta).HasColumnType("timestamp with time zone");

        // Para listar quiénes representan a un contribuyente.
        builder.HasIndex(r => r.CuitRepresentado);

        builder.HasOne(r => r.Representado)
            .WithMany()
            .HasForeignKey(r => r.CuitRepresentado)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
