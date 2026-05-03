using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Persistence;
using Xunit;

namespace Tests.Persistence.Migrations;

public class EventRelationshipMigrationTests
{
    [Fact]
    public async Task MigratingToManyToManyEventRelationships_PreservesExistingRegistrationsAndTags()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using (var context = new AppDbContext(options))
        {
            var migrator = context.GetService<IMigrator>();
            await migrator.MigrateAsync("20260107201304_MigrateNewDomainModelsToNoSqlAndDb");

            await context.Database.ExecuteSqlRawAsync(
                """
                INSERT INTO Groups (GroupId, Discriminator, EventId, EventName, EventDescription, Location)
                VALUES ('11111111-1111-1111-1111-111111111111', 'Event', '22222222-2222-2222-2222-222222222222', 'Existing Event', 'Description', 'Remote');

                INSERT INTO People (PersonId, EventGroupId)
                VALUES ('33333333-3333-3333-3333-333333333333', '11111111-1111-1111-1111-111111111111');

                INSERT INTO Tags (TagId, TagName, EventGroupId)
                VALUES ('44444444-4444-4444-4444-444444444444', 'Existing Tag', '11111111-1111-1111-1111-111111111111');
                """);

            await migrator.MigrateAsync("20260111182435_MigrateNewDomainModelsToNoSqlAndDbSecond");
        }

        await using (var verificationContext = new AppDbContext(options))
        {
            var registrationCount = await verificationContext.Database.SqlQueryRaw<int>(
                """
                SELECT COUNT(*) AS Value
                FROM EventRegistration
                WHERE EventGroupId = '11111111-1111-1111-1111-111111111111'
                  AND RegistrationPersonId = '33333333-3333-3333-3333-333333333333'
                """)
                .SingleAsync();

            var tagCount = await verificationContext.Database.SqlQueryRaw<int>(
                """
                SELECT COUNT(*) AS Value
                FROM EventTags
                WHERE EventGroupId = '11111111-1111-1111-1111-111111111111'
                  AND TagsTagId = '44444444-4444-4444-4444-444444444444'
                """)
                .SingleAsync();

            registrationCount.Should().Be(1);
            tagCount.Should().Be(1);
        }
    }
}
