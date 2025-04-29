using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capitol_Theatre.Migrations
{
    /// <inheritdoc />
    public partial class AddCardBackgroundColorToSiteSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SiteSettingsId",
                table: "SocialMediaLinks",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CardBackgroundColor",
                table: "SiteSettings",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "CardBackgroundColor",
                value: "#ffffff");

            migrationBuilder.CreateIndex(
                name: "IX_SocialMediaLinks_SiteSettingsId",
                table: "SocialMediaLinks",
                column: "SiteSettingsId");

            migrationBuilder.AddForeignKey(
                name: "FK_SocialMediaLinks_SiteSettings_SiteSettingsId",
                table: "SocialMediaLinks",
                column: "SiteSettingsId",
                principalTable: "SiteSettings",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SocialMediaLinks_SiteSettings_SiteSettingsId",
                table: "SocialMediaLinks");

            migrationBuilder.DropIndex(
                name: "IX_SocialMediaLinks_SiteSettingsId",
                table: "SocialMediaLinks");

            migrationBuilder.DropColumn(
                name: "SiteSettingsId",
                table: "SocialMediaLinks");

            migrationBuilder.DropColumn(
                name: "CardBackgroundColor",
                table: "SiteSettings");
        }
    }
}
