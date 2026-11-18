using ArcaSim.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArcaSim.Infraestructura.Persistencia.Configuraciones;

public class UsuarioConfiguracion : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");

        builder.HasKey(u => u.Cuit);
        builder.Property(u => u.Cuit).HasMaxLength(11);

        builder.Property(u => u.Nombre).HasMaxLength(100).IsRequired();
        builder.Property(u => u.Apellido).HasMaxLength(100).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(254).IsRequired();
        builder.Property(u => u.HashClave).HasMaxLength(200).IsRequired();

        builder.Property(u => u.FechaAlta).HasColumnType("timestamp with time zone");
        builder.Property(u => u.UltimoIngreso).HasColumnType("timestamp with time zone");
        builder.Property(u => u.BloqueadoHasta).HasColumnType("timestamp with time zone");

        builder.Ignore(u => u.NombreCompleto);

        builder.HasMany(u => u.Representaciones)
            .WithOne(r => r.Usuario)
            .HasForeignKey(r => r.CuitUsuario)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
