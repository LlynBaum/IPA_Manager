using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ipa.Manager.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddDbGeneratedIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "first_name",
                table: "users");

            migrationBuilder.DropColumn(
                name: "last_name",
                table: "users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "first_name",
                table: "users",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "last_name",
                table: "users",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
