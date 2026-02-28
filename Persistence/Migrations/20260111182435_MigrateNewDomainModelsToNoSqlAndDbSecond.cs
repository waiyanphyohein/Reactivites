using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MigrateNewDomainModelsToNoSqlAndDbSecond : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_People_Groups_EventGroupId",
                table: "People");

            migrationBuilder.DropForeignKey(
                name: "FK_Tags_Groups_EventGroupId",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Tags_EventGroupId",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_People_EventGroupId",
                table: "People");

            migrationBuilder.DropColumn(
                name: "EventGroupId",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "EventGroupId",
                table: "People");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "People",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "People",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "EventRegistration",
                columns: table => new
                {
                    EventGroupId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RegistrationPersonId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventRegistration", x => new { x.EventGroupId, x.RegistrationPersonId });
                    table.ForeignKey(
                        name: "FK_EventRegistration_Groups_EventGroupId",
                        column: x => x.EventGroupId,
                        principalTable: "Groups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventRegistration_People_RegistrationPersonId",
                        column: x => x.RegistrationPersonId,
                        principalTable: "People",
                        principalColumn: "PersonId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventTags",
                columns: table => new
                {
                    EventGroupId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TagsTagId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventTags", x => new { x.EventGroupId, x.TagsTagId });
                    table.ForeignKey(
                        name: "FK_EventTags_Groups_EventGroupId",
                        column: x => x.EventGroupId,
                        principalTable: "Groups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventTags_Tags_TagsTagId",
                        column: x => x.TagsTagId,
                        principalTable: "Tags",
                        principalColumn: "TagId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupOrganizers",
                columns: table => new
                {
                    GroupId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrganizersPersonId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupOrganizers", x => new { x.GroupId, x.OrganizersPersonId });
                    table.ForeignKey(
                        name: "FK_GroupOrganizers_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupOrganizers_People_OrganizersPersonId",
                        column: x => x.OrganizersPersonId,
                        principalTable: "People",
                        principalColumn: "PersonId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupTags",
                columns: table => new
                {
                    GroupId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GroupTagsTagId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupTags", x => new { x.GroupId, x.GroupTagsTagId });
                    table.ForeignKey(
                        name: "FK_GroupTags_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupTags_Tags_GroupTagsTagId",
                        column: x => x.GroupTagsTagId,
                        principalTable: "Tags",
                        principalColumn: "TagId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventRegistration_RegistrationPersonId",
                table: "EventRegistration",
                column: "RegistrationPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_EventTags_TagsTagId",
                table: "EventTags",
                column: "TagsTagId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupOrganizers_OrganizersPersonId",
                table: "GroupOrganizers",
                column: "OrganizersPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupTags_GroupTagsTagId",
                table: "GroupTags",
                column: "GroupTagsTagId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventRegistration");

            migrationBuilder.DropTable(
                name: "EventTags");

            migrationBuilder.DropTable(
                name: "GroupOrganizers");

            migrationBuilder.DropTable(
                name: "GroupTags");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "People");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "People");

            migrationBuilder.AddColumn<Guid>(
                name: "EventGroupId",
                table: "Tags",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EventGroupId",
                table: "People",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_EventGroupId",
                table: "Tags",
                column: "EventGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_People_EventGroupId",
                table: "People",
                column: "EventGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_People_Groups_EventGroupId",
                table: "People",
                column: "EventGroupId",
                principalTable: "Groups",
                principalColumn: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_Groups_EventGroupId",
                table: "Tags",
                column: "EventGroupId",
                principalTable: "Groups",
                principalColumn: "GroupId");
        }
    }
}
