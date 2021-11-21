using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecretSanta.Migrations
{
    public partial class AddMetadata : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "metadata",
                table: "santa_event",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "metadata",
                table: "santa_event");
        }
    }
}
