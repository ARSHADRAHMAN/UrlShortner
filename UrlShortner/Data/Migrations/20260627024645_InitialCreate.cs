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
                    AccessCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CustomAlias = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrlEntries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CreatedAt",
                table: "UrlEntries",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CustomAlias",
                table: "UrlEntries",
                column: "CustomAlias",
                unique: true,
                filter: "[CustomAlias] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ExpiresAt",
                table: "UrlEntries",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_IsActive",
                table: "UrlEntries",
                column: "IsActive");

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
