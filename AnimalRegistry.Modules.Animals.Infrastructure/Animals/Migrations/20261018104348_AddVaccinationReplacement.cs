using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnimalRegistry.Modules.Animals.Infrastructure.Animals.Migrations
{
    /// <inheritdoc />
    public partial class AddVaccinationReplacement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ReplacesVaccinationId",
                table: "AnimalVaccinations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AnimalVaccinations_ReplacesVaccinationId",
                table: "AnimalVaccinations",
                column: "ReplacesVaccinationId",
                unique: true,
                filter: "[ReplacesVaccinationId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AnimalVaccinations_ReplacesVaccinationId",
                table: "AnimalVaccinations");

            migrationBuilder.DropColumn(
                name: "ReplacesVaccinationId",
                table: "AnimalVaccinations");
        }
    }
}
