using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnimalRegistry.Modules.Animals.Infrastructure.Animals.Migrations;

/// <inheritdoc />
public partial class AddUniqueSignatureIndex : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "Signature",
            table: "Animals",
            type: "nvarchar(9)",
            maxLength: 9,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(100)",
            oldMaxLength: 100);

        migrationBuilder.CreateIndex(
            name: "IX_Animals_Signature_ShelterId",
            table: "Animals",
            columns: new[] { "Signature", "ShelterId" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Animals_Signature_ShelterId",
            table: "Animals");

        migrationBuilder.AlterColumn<string>(
            name: "Signature",
            table: "Animals",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(9)",
            oldMaxLength: 9);
    }
}