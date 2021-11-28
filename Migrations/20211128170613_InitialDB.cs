﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

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
                    Subreddit = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    YoutubeLinkId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Score = table.Column<int>(type: "int", nullable: false),
                    CreatedUTC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RetrievedUTC = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RedditComments", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RedditComments_Subreddit_YoutubeLinkId",
                table: "RedditComments",
                columns: new[] { "Subreddit", "YoutubeLinkId" },
                unique: true,
                filter: "[Subreddit] IS NOT NULL AND [YoutubeLinkId] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RedditComments");
        }
    }
}