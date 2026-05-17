using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnimalRegistry.Modules.Animals.Infrastructure.Animals.Migrations;

/// <inheritdoc />
public partial class AddBreedAndDistinguishingMarksToAnimal : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Breed",
            table: "Animals",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "DistinguishingMarks",
            table: "Animals",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: false,
            defaultValue: "");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Breed",
            table: "Animals");

        migrationBuilder.DropColumn(
            name: "DistinguishingMarks",
            table: "Animals");
    }
}