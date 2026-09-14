using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kiwbi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDeveloperCompanyTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "developer_company_id",
                table: "AspNetUsers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "developer_companies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    logo_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    primary_color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    secondary_color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_developer_companies", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_developer_company_id",
                table: "AspNetUsers",
                column: "developer_company_id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_developer_companies_developer_company_id",
                table: "AspNetUsers",
                column: "developer_company_id",
                principalTable: "developer_companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_developer_companies_developer_company_id",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "developer_companies");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_developer_company_id",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "developer_company_id",
                table: "AspNetUsers");
        }
    }
}
