using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Task_2EF.Migrations
{
    /// <inheritdoc />
    public partial class EmployeeDBV0IdentityInsertRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "7abb9d74-90e5-4070-a8fb-dcfc2054c0d4", null, "Visitor", "VISITOR" },
                    { "e19de7de-69d2-4bbf-9d1f-9467bf10edab", null, "Admin", "ADMIN" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "7abb9d74-90e5-4070-a8fb-dcfc2054c0d4");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "e19de7de-69d2-4bbf-9d1f-9467bf10edab");
        }
    }
}
