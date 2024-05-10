using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sorbate.Storage.Migrations
{
    /// <inheritdoc />
    public partial class ModLoaderVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ModLoaderVersion",
                table: "Files",
                type: "TEXT",
                maxLength: 256,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModLoaderVersion",
                table: "Files");
        }
    }
}
