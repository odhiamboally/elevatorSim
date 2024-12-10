using ES.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ES.Persistence.EntityConfigurations;
internal sealed class ElevatorEntityTypeConfiguration : IEntityTypeConfiguration<Elevator>
{
    public void Configure(EntityTypeBuilder<Elevator> builder)
    {
        builder
            .Property(e => e.Capacity)
            .IsRequired();

        builder
            .Property(e => e.Status)
            .IsRequired();


    }
}
