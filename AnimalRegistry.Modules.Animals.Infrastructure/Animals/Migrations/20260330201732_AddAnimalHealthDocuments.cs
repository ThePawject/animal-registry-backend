using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnimalRegistry.Modules.Animals.Infrastructure.Animals.Migrations
{
    /// <inheritdoc />
    public partial class AddAnimalHealthDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnimalHealthRecords_AnimalHealthDocument_DocumentId",
                table: "AnimalHealthRecords");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AnimalHealthDocument",
                table: "AnimalHealthDocument");

            migrationBuilder.RenameTable(
                name: "AnimalHealthDocument",
                newName: "AnimalHealthDocuments");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AnimalHealthDocuments",
                table: "AnimalHealthDocuments",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AnimalHealthRecords_AnimalHealthDocuments_DocumentId",
                table: "AnimalHealthRecords",
                column: "DocumentId",
                principalTable: "AnimalHealthDocuments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnimalHealthRecords_AnimalHealthDocuments_DocumentId",
                table: "AnimalHealthRecords");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AnimalHealthDocuments",
                table: "AnimalHealthDocuments");

            migrationBuilder.RenameTable(
                name: "AnimalHealthDocuments",
                newName: "AnimalHealthDocument");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AnimalHealthDocument",
                table: "AnimalHealthDocument",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AnimalHealthRecords_AnimalHealthDocument_DocumentId",
                table: "AnimalHealthRecords",
                column: "DocumentId",
                principalTable: "AnimalHealthDocument",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
