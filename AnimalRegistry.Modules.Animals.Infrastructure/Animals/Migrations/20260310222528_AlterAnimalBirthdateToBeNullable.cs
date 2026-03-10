using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
#pragma warning disable

namespace AnimalRegistry.Modules.Animals.Infrastructure.Animals.Migrations;

/// <inheritdoc />
public partial class AlterAnimalBirthdateToBeNullable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "BirthDate",
            table: "Animals",
            type: "datetimeoffset",
            nullable: true,
            oldClrType: typeof(DateTimeOffset),
            oldType: "datetimeoffset");

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "BirthDate",
            table: "Animals",
            type: "datetimeoffset",
            nullable: false,
            defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
            oldClrType: typeof(DateTimeOffset),
            oldType: "datetimeoffset",
            oldNullable: true);
}