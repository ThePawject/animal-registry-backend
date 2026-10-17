using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnimalRegistry.Modules.Animals.Infrastructure.Animals.Migrations
{
    /// <inheritdoc />
    public partial class AddAnimalOriginModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Municipalities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShelterId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Municipalities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AnimalOrigins",
                columns: table => new
                {
                    AnimalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdmissionDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    AdmissionMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FoundStreet = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FoundMunicipalityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractorMunicipalityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnimalOrigins", x => x.AnimalId);
                    table.ForeignKey(
                        name: "FK_AnimalOrigins_Animals_AnimalId",
                        column: x => x.AnimalId,
                        principalTable: "Animals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnimalOrigins_Municipalities_ContractorMunicipalityId",
                        column: x => x.ContractorMunicipalityId,
                        principalTable: "Municipalities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AnimalOrigins_Municipalities_FoundMunicipalityId",
                        column: x => x.FoundMunicipalityId,
                        principalTable: "Municipalities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnimalOrigins_AdmissionDate",
                table: "AnimalOrigins",
                column: "AdmissionDate");

            migrationBuilder.CreateIndex(
                name: "IX_AnimalOrigins_ContractorMunicipalityId",
                table: "AnimalOrigins",
                column: "ContractorMunicipalityId");

            migrationBuilder.CreateIndex(
                name: "IX_AnimalOrigins_FoundMunicipalityId",
                table: "AnimalOrigins",
                column: "FoundMunicipalityId");

            migrationBuilder.CreateIndex(
                name: "IX_Municipalities_ShelterId_Name",
                table: "Municipalities",
                columns: new[] { "ShelterId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnimalOrigins");

            migrationBuilder.DropTable(
                name: "Municipalities");
        }
    }
}
