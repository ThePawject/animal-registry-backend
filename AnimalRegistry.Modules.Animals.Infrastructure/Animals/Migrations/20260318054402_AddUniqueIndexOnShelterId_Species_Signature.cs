using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnimalRegistry.Modules.Animals.Infrastructure.Animals.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexOnShelterId_Species_Signature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Animals_Signature_ShelterId",
                table: "Animals");

            migrationBuilder.AlterColumn<string>(
                name: "Signature",
                table: "Animals",
                type: "nvarchar(9)",
                maxLength: 9,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(11)",
                oldMaxLength: 11);

            migrationBuilder.CreateIndex(
                name: "IX_Animals_ShelterId_Species_Signature",
                table: "Animals",
                columns: new[] { "ShelterId", "Species", "Signature" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Animals_ShelterId_Species_Signature",
                table: "Animals");

            migrationBuilder.AlterColumn<string>(
                name: "Signature",
                table: "Animals",
                type: "nvarchar(11)",
                maxLength: 11,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(9)",
                oldMaxLength: 9);

            migrationBuilder.CreateIndex(
                name: "IX_Animals_Signature_ShelterId",
                table: "Animals",
                columns: new[] { "Signature", "ShelterId" },
                unique: true);
        }
    }
}
