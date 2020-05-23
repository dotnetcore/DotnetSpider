using Microsoft.EntityFrameworkCore.Migrations;

namespace DotnetSpider.Portal.Migrations
{
    public partial class AddDockerVolume : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VOLUME",
                table: "SPIDER",
                maxLength: 2000,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VOLUME",
                table: "SPIDER");
        }
    }
}
