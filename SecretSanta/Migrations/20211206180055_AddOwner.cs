using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecretSanta.Migrations
{
    public partial class AddOwner : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "owner_id",
                table: "santa_event",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("update santa_event set owner_id = 'burningleg'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "owner_id",
                table: "santa_event");
        }
    }
}
