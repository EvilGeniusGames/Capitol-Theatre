using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capitol_Theatre.Migrations
{
    /// <inheritdoc />
    public partial class NRAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Ratings",
                columns: new[] { "Id", "Code", "Description" },
                values: new object[] { 6, "NR", "Not Rated. Content may not have been submitted for rating or is not suitable for classification." });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Ratings",
                keyColumn: "Id",
                keyValue: 6);
        }
    }
}
