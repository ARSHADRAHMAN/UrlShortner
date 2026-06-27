using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrlShortner.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAnalyticsAndOwnershipFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ExpiresAt",
                table: "UrlEntries");

            migrationBuilder.RenameColumn(
                name: "AccessCount",
                table: "UrlEntries",
                newName: "ClickCount");

            migrationBuilder.AlterTable(
                name: "UrlEntries",
                comment: "Shortened URL entries with analytics");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "UrlEntries",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastVisited",
                table: "UrlEntries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "UrlEntries",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Active_Expiration",
                table: "UrlEntries",
                columns: new[] { "IsActive", "ExpiresAt" },
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ClickCount",
                table: "UrlEntries",
                column: "ClickCount",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_ExpiresAt",
                table: "UrlEntries",
                column: "ExpiresAt",
                filter: "[ExpiresAt] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LastVisited",
                table: "UrlEntries",
                column: "LastVisited",
                filter: "[LastVisited] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Owner_Active_Created",
                table: "UrlEntries",
                columns: new[] { "OwnerId", "IsActive", "CreatedAt" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_OwnerId",
                table: "UrlEntries",
                column: "OwnerId",
                filter: "[OwnerId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Active_Expiration",
                table: "UrlEntries");

            migrationBuilder.DropIndex(
                name: "IX_ClickCount",
                table: "UrlEntries");

            migrationBuilder.DropIndex(
                name: "IX_ExpiresAt",
                table: "UrlEntries");

            migrationBuilder.DropIndex(
                name: "IX_LastVisited",
                table: "UrlEntries");

            migrationBuilder.DropIndex(
                name: "IX_Owner_Active_Created",
                table: "UrlEntries");

            migrationBuilder.DropIndex(
                name: "IX_OwnerId",
                table: "UrlEntries");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "UrlEntries");

            migrationBuilder.DropColumn(
                name: "LastVisited",
                table: "UrlEntries");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "UrlEntries");

            migrationBuilder.RenameColumn(
                name: "ClickCount",
                table: "UrlEntries",
                newName: "AccessCount");

            migrationBuilder.AlterTable(
                name: "UrlEntries",
                oldComment: "Shortened URL entries with analytics");

            migrationBuilder.CreateIndex(
                name: "IX_ExpiresAt",
                table: "UrlEntries",
                column: "ExpiresAt");
        }
    }
}
