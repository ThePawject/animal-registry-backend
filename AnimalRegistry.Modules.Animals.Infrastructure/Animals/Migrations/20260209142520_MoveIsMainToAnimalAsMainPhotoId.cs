using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnimalRegistry.Modules.Animals.Infrastructure.Animals.Migrations;

/// <inheritdoc />
public partial class MoveIsMainToAnimalAsMainPhotoId : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_AnimalPhotos_IsMain",
            table: "AnimalPhotos");

        migrationBuilder.DropColumn(
            name: "IsMain",
            table: "AnimalPhotos");

        migrationBuilder.AddColumn<Guid>(
            name: "MainPhotoId",
            table: "Animals",
            type: "uniqueidentifier",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "MainPhotoId",
            table: "Animals");

        migrationBuilder.AddColumn<bool>(
            name: "IsMain",
            table: "AnimalPhotos",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.CreateIndex(
            name: "IX_AnimalPhotos_IsMain",
            table: "AnimalPhotos",
            column: "IsMain");
    }
}