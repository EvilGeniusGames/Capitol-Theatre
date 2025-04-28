using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Capitol_Theatre.Migrations
{
    /// <inheritdoc />
    public partial class SiteSettingsAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SiteSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IconUrl = table.Column<string>(type: "TEXT", nullable: false),
                    BackgroundImageUrl = table.Column<string>(type: "TEXT", nullable: false),
                    BackgroundImageAlignment = table.Column<string>(type: "TEXT", nullable: false),
                    BackgroundImageTiled = table.Column<bool>(type: "INTEGER", nullable: false),
                    BackgroundColor = table.Column<string>(type: "TEXT", nullable: false),
                    FontColor = table.Column<string>(type: "TEXT", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SocialMediaTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    FontAwesomeClass = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocialMediaTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SocialMediaLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SocialMediaTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    IconColor = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocialMediaLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SocialMediaLinks_SocialMediaTypes_SocialMediaTypeId",
                        column: x => x.SocialMediaTypeId,
                        principalTable: "SocialMediaTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "SiteSettings",
                columns: new[] { "Id", "BackgroundColor", "BackgroundImageAlignment", "BackgroundImageTiled", "BackgroundImageUrl", "FontColor", "IconUrl", "LastUpdated" },
                values: new object[] { 1, "#ffffff", "left", false, "", "#000000", "", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "SocialMediaTypes",
                columns: new[] { "Id", "FontAwesomeClass", "Name" },
                values: new object[,]
                {
                    { 1, "fab fa-facebook", "Facebook" },
                    { 2, "fab fa-instagram", "Instagram" },
                    { 3, "fab fa-youtube", "YouTube" },
                    { 4, "fab fa-x-twitter", "Twitter" },
                    { 5, "fab fa-linkedin", "LinkedIn" },
                    { 6, "fab fa-tiktok", "TikTok" },
                    { 7, "fab fa-pinterest", "Pinterest" },
                    { 8, "fas fa-globe", "Bluesky" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_SocialMediaLinks_SocialMediaTypeId",
                table: "SocialMediaLinks",
                column: "SocialMediaTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SiteSettings");

            migrationBuilder.DropTable(
                name: "SocialMediaLinks");

            migrationBuilder.DropTable(
                name: "SocialMediaTypes");
        }
    }
}
