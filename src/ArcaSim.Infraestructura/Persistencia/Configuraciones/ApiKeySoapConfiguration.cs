
using ArcaSim.Infraestructura.Persistencia.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArcaSim.Infraestructura.Persistencia.Configuraciones;

public sealed class ApiKeySoapConfiguration
    : IEntityTypeConfiguration<ApiKeySoap>
{
    public void Configure(EntityTypeBuilder<ApiKeySoap> builder)
    {
        builder.ToTable("api_keys_soap");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Hash)
            .HasColumnName("hash")
            .HasMaxLength(64)
            .IsRequired();

        builder.HasIndex(x => x.Hash)
            .IsUnique()
            .HasDatabaseName("ux_api_keys_soap_hash");

        builder.Property(x => x.Cuit)
            .HasColumnName("cuit")
            .HasMaxLength(11)
            .IsRequired();

        builder.HasIndex(x => x.Cuit)
            .HasDatabaseName("ix_api_keys_soap_cuit");

        builder.Property(x => x.Revocada)
            .HasColumnName("revocada")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.FechaCreacion)
            .HasColumnName("fecha_creacion")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.FechaRevocacion)
            .HasColumnName("fecha_revocacion")
            .HasColumnType("timestamp with time zone");
    }
}
