using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace slender_server.Infra.Migrations.Application
{
    /// <inheritdoc />
    public partial class AddReadAtMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "read_at_utc",
                schema: "slender",
                table: "messages",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "read_at_utc",
                schema: "slender",
                table: "messages");
        }
    }
}
