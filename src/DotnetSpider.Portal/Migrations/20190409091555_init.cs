using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DotnetSpider.Portal.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DockerImageRepositories",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(maxLength: 255, nullable: true),
                    Registry = table.Column<string>(maxLength: 255, nullable: true),
                    Repository = table.Column<string>(maxLength: 255, nullable: true),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    LastModificationTime = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DockerImageRepositories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DockerImages",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DockerImageRepositoryId = table.Column<int>(nullable: false),
                    Repository = table.Column<string>(maxLength: 255, nullable: true),
                    CreationTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DockerImages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DockerImageRepositories_CreationTime",
                table: "DockerImageRepositories",
                column: "CreationTime");

            migrationBuilder.CreateIndex(
                name: "IX_DockerImageRepositories_LastModificationTime",
                table: "DockerImageRepositories",
                column: "LastModificationTime");

            migrationBuilder.CreateIndex(
                name: "IX_DockerImageRepositories_Name",
                table: "DockerImageRepositories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DockerImageRepositories_Repository",
                table: "DockerImageRepositories",
                column: "Repository",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DockerImages_Repository",
                table: "DockerImages",
                column: "Repository",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DockerImageRepositories");

            migrationBuilder.DropTable(
                name: "DockerImages");
        }
    }
}
