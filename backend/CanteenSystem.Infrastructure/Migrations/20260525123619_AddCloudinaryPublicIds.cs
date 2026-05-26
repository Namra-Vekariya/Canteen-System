using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CanteenSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCloudinaryPublicIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfileImagePublicId",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImagePublicId",
                table: "MenuItems",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfileImagePublicId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ImagePublicId",
                table: "MenuItems");
        }
    }
}
