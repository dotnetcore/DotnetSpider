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
                    id = table.Column<int>()
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(maxLength: 255),
                    schema = table.Column<string>(maxLength: 10, nullable: true),
                    registry = table.Column<string>(maxLength: 255, nullable: true),
                    repository = table.Column<string>(maxLength: 255),
                    user_name = table.Column<string>(maxLength: 255, nullable: true),
                    password = table.Column<string>(maxLength: 255, nullable: true),
                    creation_time = table.Column<DateTime>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_docker_repository", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "spider",
                columns: table => new
                {
                    id = table.Column<int>()
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(maxLength: 255),
                    type = table.Column<string>(maxLength: 400),
                    cron = table.Column<string>(maxLength: 255),
                    environment = table.Column<string>(maxLength: 255, nullable: true),
                    registry = table.Column<string>(maxLength: 255, nullable: true),
                    repository = table.Column<string>(maxLength: 255),
                    tag = table.Column<string>(maxLength: 255),
                    creation_time = table.Column<DateTime>(),
                    last_modification_time = table.Column<DateTime>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_spider", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "spider_container",
                columns: table => new
                {
                    id = table.Column<int>()
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    spider_id = table.Column<int>(),
                    container_id = table.Column<string>(maxLength: 100, nullable: true),
                    batch = table.Column<string>(maxLength: 100, nullable: true),
                    creation_time = table.Column<DateTime>(),
                    status = table.Column<string>(maxLength: 20)
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
                name: "IX_docker_repository_repository_registry",
                table: "docker_repository",
                columns: new[] { "repository", "registry" },
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
                name: "IX_spider_container_batch",
                table: "spider_container",
                column: "batch");

            migrationBuilder.CreateIndex(
                name: "IX_spider_container_creation_time",
                table: "spider_container",
                column: "creation_time");

            migrationBuilder.CreateIndex(
                name: "IX_spider_container_spider_id",
                table: "spider_container",
                column: "spider_id");
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
