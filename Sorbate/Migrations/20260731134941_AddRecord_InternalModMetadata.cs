using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sorbate.Migrations
{
    /// <inheritdoc />
    public partial class AddRecord_InternalModMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Author",
                table: "ModRecords",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "ModRecords",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InternalName",
                table: "ModRecords",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ModLoaderVersion",
                table: "ModRecords",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Version",
                table: "ModRecords",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Author",
                table: "ModRecords");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "ModRecords");

            migrationBuilder.DropColumn(
                name: "InternalName",
                table: "ModRecords");

            migrationBuilder.DropColumn(
                name: "ModLoaderVersion",
                table: "ModRecords");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "ModRecords");
        }
    }
}
