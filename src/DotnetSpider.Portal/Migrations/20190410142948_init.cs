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
                name: "docker_image",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    repository_id = table.Column<int>(nullable: false),
                    image = table.Column<string>(maxLength: 255, nullable: false),
                    creation_time = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_docker_image", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "docker_repository",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(maxLength: 255, nullable: false),
                    registry = table.Column<string>(maxLength: 255, nullable: false),
                    repository = table.Column<string>(maxLength: 255, nullable: false),
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
                    cron = table.Column<string>(maxLength: 255, nullable: false),
                    arguments = table.Column<string>(maxLength: 255, nullable: true),
                    image = table.Column<string>(maxLength: 255, nullable: false),
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
                    container_id = table.Column<Guid>(nullable: false),
                    creation_time = table.Column<DateTime>(nullable: false),
                    exit_time = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_spider_container", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_docker_image_image",
                table: "docker_image",
                column: "image",
                unique: true);

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
                name: "IX_spider_image",
                table: "spider",
                column: "image");

            migrationBuilder.CreateIndex(
                name: "IX_spider_name",
                table: "spider",
                column: "name");

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
                name: "docker_image");

            migrationBuilder.DropTable(
                name: "docker_repository");

            migrationBuilder.DropTable(
                name: "spider");

            migrationBuilder.DropTable(
                name: "spider_container");
        }
    }
}
