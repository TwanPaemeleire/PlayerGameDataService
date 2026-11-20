using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlayerGameDataService.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerTokenHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TokenHash",
                table: "Players",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Players_TokenHash",
                table: "Players",
                column: "TokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Players_TokenHash",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "TokenHash",
                table: "Players");
        }
    }
}
