using Domain.Entidades;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Fcg.Users.Api.Infra.Configs;

public sealed class UsuarioConfig : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> e)
    {
        var emailConv = new ValueConverter<EmailStruct, string>(
            v => v.Address,
            v => EmailStruct.Create(v)
        );

        var hashConv = new ValueConverter<SenhaHashed, string>(
            v => v.Hash,
            v => new SenhaHashed(v)
        );

        e.HasKey(x => x.Id);

        e.Property(x => x.Nome)
            .IsRequired()
            .HasMaxLength(120);

        e.Property(x => x.Email)
         .HasConversion(v => v.Address, s => EmailStruct.Create(s))
         .IsRequired()
         .HasMaxLength(160);

        e.Property(x => x.SenhaHashed)
            .HasConversion(hashConv)
            .IsRequired()
            .HasMaxLength(200);

        e.Property(x => x.NivelAcesso)
            .IsRequired();

        e.Property(x => x.DataCriacao)
            .IsRequired();

        e.HasIndex(x => x.Email).IsUnique();
    }
}
