using Domain.Entidades;
using Domain.Shared;
using Fcg.Users.Api.Infra.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Fcg.Users.Api.Infra.Configs;

public sealed class EventEntityConfig : IEntityTypeConfiguration<EventEntity>
{
    public void Configure(EntityTypeBuilder<EventEntity> e)
    {
        e.HasKey(x => x.EventId);

        e.Property(x => x.AggregateId)
            .IsRequired();

        e.Property(x => x.EventType)
            .IsRequired()
            .HasMaxLength(200);

        e.Property(x => x.OccurredAt)
            .IsRequired();

        e.Property(x => x.Version)
            .IsRequired();

        e.Property(x => x.Payload)
            .IsRequired();

        e.HasIndex(x => new { x.AggregateId, x.Version })
            .IsUnique();
    }
}