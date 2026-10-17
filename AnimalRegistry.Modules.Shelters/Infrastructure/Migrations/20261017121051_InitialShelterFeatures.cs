using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnimalRegistry.Modules.Shelters.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialShelterFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShelterFeatureActivations",
                columns: table => new
                {
                    ShelterId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FeatureKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EnabledBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EnabledOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShelterFeatureActivations", x => new { x.ShelterId, x.FeatureKey });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShelterFeatureActivations");
        }
    }
}
