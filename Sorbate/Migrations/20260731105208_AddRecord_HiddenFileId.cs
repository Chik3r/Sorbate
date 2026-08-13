using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sorbate.Migrations
{
    /// <inheritdoc />
    public partial class AddRecord_HiddenFileId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Source",
                table: "ModRecords",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "FileId",
                table: "ModRecords",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Hidden",
                table: "ModRecords",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_ModRecords_Hash",
                table: "ModRecords",
                column: "Hash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ModRecords_Hash",
                table: "ModRecords");

            migrationBuilder.DropColumn(
                name: "FileId",
                table: "ModRecords");

            migrationBuilder.DropColumn(
                name: "Hidden",
                table: "ModRecords");

            migrationBuilder.AlterColumn<string>(
                name: "Source",
                table: "ModRecords",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
