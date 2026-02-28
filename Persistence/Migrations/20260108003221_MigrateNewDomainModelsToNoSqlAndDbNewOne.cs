using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MigrateNewDomainModelsToNoSqlAndDbNewOne : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Intentionally left as a no-op migration: no schema changes required.
            migrationBuilder.Sql("/* No-op migration: no schema changes. */");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally left as a no-op down migration: no schema changes to revert.
            migrationBuilder.Sql("/* No-op down migration: no schema changes to revert. */");
        }
    }
}
