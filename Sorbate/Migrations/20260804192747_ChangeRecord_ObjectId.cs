using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sorbate.Migrations
{
    /// <inheritdoc />
    public partial class ChangeRecord_ObjectId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IconObjectId",
                table: "ModRecords",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModObjectId",
                table: "ModRecords",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IconObjectId",
                table: "ModRecords");

            migrationBuilder.DropColumn(
                name: "ModObjectId",
                table: "ModRecords");
        }
    }
}
