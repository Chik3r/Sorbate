using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Sorbate.Migrations
{
    /// <inheritdoc />
    public partial class AddSteamRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PublishedFileId",
                table: "ModRecords",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SteamUpdateRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PublishedFileId = table.Column<string>(type: "text", nullable: false),
                    TimeUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SteamUpdateRecords", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SteamUpdateRecords_PublishedFileId",
                table: "SteamUpdateRecords",
                column: "PublishedFileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SteamUpdateRecords");

            migrationBuilder.DropColumn(
                name: "PublishedFileId",
                table: "ModRecords");
        }
    }
}
