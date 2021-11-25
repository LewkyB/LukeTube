using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace luke_site_mvc.Migrations
{
    public partial class InitialDB : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RedditComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Subreddit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    YoutubeLinkId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Score = table.Column<int>(type: "int", nullable: false),
                    CreatedUTC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RetrievedUTC = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RedditComments", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RedditComments");
        }
    }
}
