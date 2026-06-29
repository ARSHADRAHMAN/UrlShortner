using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrlShortner.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IsActive",
                table: "UrlEntries");

            migrationBuilder.DropIndex(
                name: "IX_Owner_Active_Created",
                table: "UrlEntries");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "UrlEntries");

            migrationBuilder.CreateIndex(
                name: "IX_Owner_Created",
                table: "UrlEntries",
                columns: new[] { "OwnerId", "CreatedAt" },
                descending: new[] { false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Owner_Created",
                table: "UrlEntries");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "UrlEntries",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_IsActive",
                table: "UrlEntries",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Owner_Active_Created",
                table: "UrlEntries",
                columns: new[] { "OwnerId", "IsActive", "CreatedAt" },
                descending: new[] { false, false, true });
        }
    }
}
