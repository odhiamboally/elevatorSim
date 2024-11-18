using Microsoft.EntityFrameworkCore;

using ES.Domain.Entities;

namespace ES.Persistence.DataContext;

public class DBContext : DbContext
{
    public DBContext(DbContextOptions<DBContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DBContext).Assembly);
    }

    public DbSet<Log>? Logs { get; set; }


}
