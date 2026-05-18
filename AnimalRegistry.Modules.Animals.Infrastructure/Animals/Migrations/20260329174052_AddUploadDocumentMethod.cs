using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnimalRegistry.Modules.Animals.Infrastructure.Animals.Migrations
{
    /// <inheritdoc />
    public partial class AddUploadDocumentMethod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DocumentId",
                table: "AnimalHealthRecords",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AnimalHealthDocument",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HealthRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BlobPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UploadedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnimalHealthDocument", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnimalHealthRecords_DocumentId",
                table: "AnimalHealthRecords",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_AnimalHealthDocuments_HealthRecordId",
                table: "AnimalHealthDocument",
                column: "HealthRecordId");

            migrationBuilder.AddForeignKey(
                name: "FK_AnimalHealthRecords_AnimalHealthDocument_DocumentId",
                table: "AnimalHealthRecords",
                column: "DocumentId",
                principalTable: "AnimalHealthDocument",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnimalHealthRecords_AnimalHealthDocument_DocumentId",
                table: "AnimalHealthRecords");

            migrationBuilder.DropTable(
                name: "AnimalHealthDocument");

            migrationBuilder.DropIndex(
                name: "IX_AnimalHealthRecords_DocumentId",
                table: "AnimalHealthRecords");

            migrationBuilder.DropColumn(
                name: "DocumentId",
                table: "AnimalHealthRecords");
        }
    }
}
