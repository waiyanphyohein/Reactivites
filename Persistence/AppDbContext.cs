using Microsoft.EntityFrameworkCore;
using Domain;

namespace Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Activity> Activities { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Person> People { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Group relationships
        modelBuilder.Entity<Group>()
            .HasMany(g => g.Organizers)
            .WithMany()
            .UsingEntity(j => j.ToTable("GroupOrganizers"));

        modelBuilder.Entity<Group>()
            .HasMany(g => g.GroupTags!)
            .WithMany()
            .UsingEntity(j => j.ToTable("GroupTags"));

        // Configure Event relationships (Event inherits from Group)
        modelBuilder.Entity<Event>()
            .HasMany(e => e.Registration!)
            .WithMany()
            .UsingEntity(j => j.ToTable("EventRegistration"));

        modelBuilder.Entity<Event>()
            .HasMany(e => e.Tags!)
            .WithMany()
            .UsingEntity(j => j.ToTable("EventTags"));

        // Configure Person properties
        modelBuilder.Entity<Person>()
            .Property(p => p.FirstName)
            .IsRequired();

        modelBuilder.Entity<Person>()
            .Property(p => p.LastName)
            .IsRequired();

        // Configure Tag properties
        modelBuilder.Entity<Tag>()
            .Property(t => t.TagName)
            .IsRequired();
    }
}
