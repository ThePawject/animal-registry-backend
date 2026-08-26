using AnimalRegistry.Modules.Contact.Domain;
using Microsoft.EntityFrameworkCore;

namespace AnimalRegistry.Modules.Contact.Infrastructure;

internal sealed class ContactDbContext(DbContextOptions<ContactDbContext> options) : DbContext(options)
{
    public DbSet<ContactRequest> ContactRequests => Set<ContactRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ContactDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}