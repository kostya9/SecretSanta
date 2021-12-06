using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecretSanta.Migrations
{
    public partial class AddArchived : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "archived",
                table: "santa_event",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "archived",
                table: "santa_event");
        }
    }
}
