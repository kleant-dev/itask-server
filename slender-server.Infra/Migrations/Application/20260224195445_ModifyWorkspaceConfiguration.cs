using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace slender_server.Infra.Migrations.Application
{
    /// <inheritdoc />
    public partial class ModifyWorkspaceConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_workspaces_slug",
                schema: "slender",
                table: "workspaces");

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at_utc",
                schema: "slender",
                table: "workspaces",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_workspaces_slug",
                schema: "slender",
                table: "workspaces",
                column: "slug",
                unique: true,
                filter: "\"deleted_at_utc\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_workspaces_slug",
                schema: "slender",
                table: "workspaces");

            migrationBuilder.DropColumn(
                name: "deleted_at_utc",
                schema: "slender",
                table: "workspaces");

            migrationBuilder.CreateIndex(
                name: "ix_workspaces_slug",
                schema: "slender",
                table: "workspaces",
                column: "slug",
                unique: true);
        }
    }
}
