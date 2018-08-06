using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace DotnetSpider.Migrator.Migrations
{
    public partial class _01 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Block",
                columns: table => new
                {
                    BlockId = table.Column<string>(nullable: false),
                    Identity = table.Column<string>(nullable: false),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    Exception = table.Column<string>(nullable: true),
                    LastModificationTime = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Block", x => new { x.BlockId, x.Identity });
                });

            migrationBuilder.CreateTable(
                name: "Node",
                columns: table => new
                {
                    NodeId = table.Column<string>(maxLength: 32, nullable: false),
                    CpuCount = table.Column<int>(nullable: false),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    Group = table.Column<string>(maxLength: 32, nullable: false),
                    Ip = table.Column<string>(maxLength: 32, nullable: true),
                    IsEnable = table.Column<bool>(nullable: true),
                    LastModificationTime = table.Column<DateTime>(nullable: true),
                    Os = table.Column<string>(maxLength: 32, nullable: false),
                    TotalMemory = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Node", x => x.NodeId);
                });

            migrationBuilder.CreateTable(
                name: "NodeHeartbeat",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Cpu = table.Column<int>(nullable: false),
                    FreeMemory = table.Column<long>(nullable: false),
                    NodeId = table.Column<string>(nullable: true),
                    ProcessCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NodeHeartbeat", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RequestQueue",
                columns: table => new
                {
                    RequestId = table.Column<string>(maxLength: 32, nullable: false),
                    Identity = table.Column<string>(maxLength: 32, nullable: false),
                    BlockId = table.Column<string>(maxLength: 32, nullable: false),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    ProcessTime = table.Column<DateTime>(nullable: true),
                    Request = table.Column<string>(nullable: true),
                    Response = table.Column<string>(nullable: true),
                    ResponseTime = table.Column<DateTime>(nullable: true),
                    StatusCode = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestQueue", x => new { x.RequestId, x.Identity });
                    table.UniqueConstraint("AK_RequestQueue_Identity_RequestId", x => new { x.Identity, x.RequestId });
                });

            migrationBuilder.CreateTable(
                name: "Running",
                columns: table => new
                {
                    Identity = table.Column<string>(maxLength: 32, nullable: false),
                    BlockTimes = table.Column<int>(nullable: false),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    LastModificationTime = table.Column<DateTime>(nullable: true),
                    Priority = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Running", x => x.Identity);
                });

            migrationBuilder.CreateTable(
                name: "RunningHistory",
                columns: table => new
                {
                    Identity = table.Column<string>(maxLength: 32, nullable: false),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    JobId = table.Column<string>(maxLength: 32, nullable: false),
                    LastModificationTime = table.Column<DateTime>(nullable: true),
                    Status = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RunningHistory", x => x.Identity);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Block");

            migrationBuilder.DropTable(
                name: "Node");

            migrationBuilder.DropTable(
                name: "NodeHeartbeat");

            migrationBuilder.DropTable(
                name: "RequestQueue");

            migrationBuilder.DropTable(
                name: "Running");

            migrationBuilder.DropTable(
                name: "RunningHistory");
        }
    }
}
