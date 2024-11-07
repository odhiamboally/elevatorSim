using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ES.Persistence.EntityConfigurations;
internal sealed class AccountEntityTypeConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder
            .Property(e => e.AccountName)
            .IsRequired();

        builder
            .Property(e => e.AccountNumber)
            .IsRequired();

        builder
            .Property(e => e.AccountBic)
            .IsRequired();

    }
}
