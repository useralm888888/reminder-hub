using Api.Domain.Entities;
using Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Data.Configurations;

public class ReminderConfiguration : IEntityTypeConfiguration<Reminder>
{
    public void Configure(EntityTypeBuilder<Reminder> builder)
    {
        builder.ToTable("reminders");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Message)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(r => r.Email)
            .HasMaxLength(320);

        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(r => r.SendAt)
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.Version)
            .IsConcurrencyToken()
            .HasDefaultValue(0);

        builder.HasIndex(r => new { r.Status, r.SendAt });
    }
}
