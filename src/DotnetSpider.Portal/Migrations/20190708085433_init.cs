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
                name: "docker_repository",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(maxLength: 255, nullable: false),
                    registry = table.Column<string>(maxLength: 255, nullable: false),
                    repository = table.Column<string>(maxLength: 255, nullable: false),
                    user_name = table.Column<string>(maxLength: 255, nullable: true),
                    password = table.Column<string>(maxLength: 255, nullable: true),
                    creation_time = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_docker_repository", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "spider",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(maxLength: 255, nullable: false),
                    type = table.Column<string>(maxLength: 400, nullable: false),
                    cron = table.Column<string>(maxLength: 255, nullable: false),
                    environment = table.Column<string>(maxLength: 255, nullable: true),
                    arguments = table.Column<string>(maxLength: 255, nullable: true),
                    registry = table.Column<string>(maxLength: 255, nullable: false),
                    repository = table.Column<string>(maxLength: 255, nullable: false),
                    tag = table.Column<string>(maxLength: 255, nullable: false),
                    creation_time = table.Column<DateTime>(nullable: false),
                    last_modification_time = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_spider", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "spider_container",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    spider_id = table.Column<int>(nullable: false),
                    container_id = table.Column<string>(maxLength: 100, nullable: true),
                    creation_time = table.Column<DateTime>(nullable: false),
                    status = table.Column<string>(maxLength: 20, nullable: false),
                    exit_time = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_spider_container", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_docker_repository_creation_time",
                table: "docker_repository",
                column: "creation_time");

            migrationBuilder.CreateIndex(
                name: "IX_docker_repository_name",
                table: "docker_repository",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_docker_repository_repository",
                table: "docker_repository",
                column: "repository",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_spider_creation_time",
                table: "spider",
                column: "creation_time");

            migrationBuilder.CreateIndex(
                name: "IX_spider_name",
                table: "spider",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_spider_repository",
                table: "spider",
                column: "repository");

            migrationBuilder.CreateIndex(
                name: "IX_spider_container_container_id",
                table: "spider_container",
                column: "container_id");

            migrationBuilder.CreateIndex(
                name: "IX_spider_container_creation_time",
                table: "spider_container",
                column: "creation_time");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "docker_repository");

            migrationBuilder.DropTable(
                name: "spider");

            migrationBuilder.DropTable(
                name: "spider_container");
        }
    }
}
