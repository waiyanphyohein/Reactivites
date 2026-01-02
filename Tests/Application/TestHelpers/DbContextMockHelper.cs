using Microsoft.EntityFrameworkCore;
using Persistence;
using Domain;

namespace Tests.Application.TestHelpers;

public static class DbContextMockHelper
{
    /// <summary>
    /// Creates an in-memory database context for testing
    /// </summary>
    public static AppDbContext CreateInMemoryContext(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: databaseName ?? Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        return new AppDbContext(options);
    }

    /// <summary>
    /// Creates an in-memory database context pre-populated with test data
    /// </summary>
    public static AppDbContext CreateInMemoryContextWithData(
        List<Activity> activities,
        string? databaseName = null)
    {
        var context = CreateInMemoryContext(databaseName);
        context.Activities.AddRange(activities);
        context.SaveChanges();

        // Detach entities to avoid tracking issues in tests
        foreach (var entity in context.ChangeTracker.Entries().ToList())
        {
            entity.State = EntityState.Detached;
        }

        return context;
    }
}
