using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MigrateNewDomainModelsToNoSqlAndDbTrack : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // This migration intentionally does not introduce any schema changes.
            migrationBuilder.Sql("-- No schema changes required for this migration (MigrateNewDomainModelsToNoSqlAndDbTrack).");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // This migration intentionally does not revert any schema changes.
            migrationBuilder.Sql("-- No schema changes to revert for this migration (MigrateNewDomainModelsToNoSqlAndDbTrack).");
        }
    }
}
