using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnimalRegistry.Modules.Animals.Infrastructure.Animals.Migrations
{
    /// <inheritdoc />
    public partial class RenameAnimalIsActiveToIsInShelter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Animals",
                newName: "IsInShelter");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsInShelter",
                table: "Animals",
                newName: "IsActive");
        }
    }
}
