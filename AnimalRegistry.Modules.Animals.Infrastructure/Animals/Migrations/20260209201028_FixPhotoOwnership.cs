using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnimalRegistry.Modules.Animals.Infrastructure.Animals.Migrations;

/// <inheritdoc />
public partial class FixPhotoOwnership : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropPrimaryKey(
            name: "PK_AnimalPhotos",
            table: "AnimalPhotos");

        migrationBuilder.DropIndex(
            name: "IX_AnimalPhotos_AnimalId",
            table: "AnimalPhotos");

        migrationBuilder.AddPrimaryKey(
            name: "PK_AnimalPhotos",
            table: "AnimalPhotos",
            columns: new[] { "AnimalId", "Id" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropPrimaryKey(
            name: "PK_AnimalPhotos",
            table: "AnimalPhotos");

        migrationBuilder.AddPrimaryKey(
            name: "PK_AnimalPhotos",
            table: "AnimalPhotos",
            column: "Id");

        migrationBuilder.CreateIndex(
            name: "IX_AnimalPhotos_AnimalId",
            table: "AnimalPhotos",
            column: "AnimalId");
    }
}