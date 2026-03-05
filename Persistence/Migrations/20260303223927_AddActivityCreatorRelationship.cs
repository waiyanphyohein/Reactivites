using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddActivityCreatorRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatorDisplayName",
                table: "Activities",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorPersonId",
                table: "Activities",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Activities_CreatorPersonId",
                table: "Activities",
                column: "CreatorPersonId");

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_People_CreatorPersonId",
                table: "Activities",
                column: "CreatorPersonId",
                principalTable: "People",
                principalColumn: "PersonId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activities_People_CreatorPersonId",
                table: "Activities");

            migrationBuilder.DropIndex(
                name: "IX_Activities_CreatorPersonId",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "CreatorDisplayName",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "CreatorPersonId",
                table: "Activities");
        }
    }
}
