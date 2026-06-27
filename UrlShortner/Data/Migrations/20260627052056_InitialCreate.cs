using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrlShortner.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UrlEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OriginalUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    ShortCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ClickCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LastVisited = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    OwnerId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrlEntries", x => x.Id);
                },
                comment: "Shortened URL entries with analytics");

            migrationBuilder.CreateIndex(
                name: "IX_ClickCount",
                table: "UrlEntries",
                column: "ClickCount",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_CreatedAt",
                table: "UrlEntries",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_IsActive",
                table: "UrlEntries",
                column: "IsActive");

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

            migrationBuilder.CreateIndex(
                name: "IX_ShortCode",
                table: "UrlEntries",
                column: "ShortCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UrlEntries");
        }
    }
}
