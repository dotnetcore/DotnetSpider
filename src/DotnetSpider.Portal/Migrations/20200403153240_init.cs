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
                name: "SPIDER",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ENABLED = table.Column<bool>(nullable: false),
                    NAME = table.Column<string>(maxLength: 255, nullable: false),
                    IMAGE = table.Column<string>(maxLength: 255, nullable: false),
                    TYPE = table.Column<string>(maxLength: 255, nullable: false),
                    CRON = table.Column<string>(maxLength: 255, nullable: false),
                    ENVIRONMENT = table.Column<string>(maxLength: 2000, nullable: true),
                    CREATION_TIME = table.Column<DateTimeOffset>(nullable: false),
                    LAST_MODIFICATION_TIME = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SPIDER", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "SPIDER_HISTORIES",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SPIDER_ID = table.Column<int>(nullable: false),
                    SPIDER_NAME = table.Column<string>(maxLength: 255, nullable: false),
                    CONTAINER_ID = table.Column<string>(maxLength: 100, nullable: true),
                    BATCH = table.Column<string>(maxLength: 36, nullable: true),
                    CREATION_TIME = table.Column<DateTimeOffset>(nullable: false),
                    STATUS = table.Column<string>(maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SPIDER_HISTORIES", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SPIDER_CREATION_TIME",
                table: "SPIDER",
                column: "CREATION_TIME");

            migrationBuilder.CreateIndex(
                name: "IX_SPIDER_NAME",
                table: "SPIDER",
                column: "NAME");

            migrationBuilder.CreateIndex(
                name: "IX_SPIDER_HISTORIES_BATCH",
                table: "SPIDER_HISTORIES",
                column: "BATCH");

            migrationBuilder.CreateIndex(
                name: "IX_SPIDER_HISTORIES_CREATION_TIME",
                table: "SPIDER_HISTORIES",
                column: "CREATION_TIME");

            migrationBuilder.CreateIndex(
                name: "IX_SPIDER_HISTORIES_SPIDER_ID",
                table: "SPIDER_HISTORIES",
                column: "SPIDER_ID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SPIDER");

            migrationBuilder.DropTable(
                name: "SPIDER_HISTORIES");
        }
    }
}
