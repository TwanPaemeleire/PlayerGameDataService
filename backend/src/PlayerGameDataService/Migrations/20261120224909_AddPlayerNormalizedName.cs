using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlayerGameDataService.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerNormalizedName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NormalizedName",
                table: "Players",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("UPDATE Players SET NormalizedName = UPPER(Name);");

            migrationBuilder.CreateIndex(
                name: "IX_Players_NormalizedName",
                table: "Players",
                column: "NormalizedName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Players_NormalizedName",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "NormalizedName",
                table: "Players");
        }
    }
}
