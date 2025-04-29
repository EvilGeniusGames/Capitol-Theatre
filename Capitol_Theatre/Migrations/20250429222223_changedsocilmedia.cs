using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capitol_Theatre.Migrations
{
    /// <inheritdoc />
    public partial class changedsocilmedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SocialMediaTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "FontAwesomeClass",
                value: "fab fa-facebook-square");

            migrationBuilder.UpdateData(
                table: "SocialMediaTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "FontAwesomeClass",
                value: "fab fa-youtube-square");

            migrationBuilder.UpdateData(
                table: "SocialMediaTypes",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "FontAwesomeClass", "Name" },
                values: new object[] { "fab fa-twitter-square", "Twitter/X" });

            migrationBuilder.UpdateData(
                table: "SocialMediaTypes",
                keyColumn: "Id",
                keyValue: 7,
                column: "FontAwesomeClass",
                value: "fab fa-pinterest-square");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SocialMediaTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "FontAwesomeClass",
                value: "fab fa-facebook");

            migrationBuilder.UpdateData(
                table: "SocialMediaTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "FontAwesomeClass",
                value: "fab fa-youtube");

            migrationBuilder.UpdateData(
                table: "SocialMediaTypes",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "FontAwesomeClass", "Name" },
                values: new object[] { "fab fa-x-twitter", "Twitter" });

            migrationBuilder.UpdateData(
                table: "SocialMediaTypes",
                keyColumn: "Id",
                keyValue: 7,
                column: "FontAwesomeClass",
                value: "fab fa-pinterest");
        }
    }
}
