using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecretSanta.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "santa_event",
                columns: table => new
                {
                    uid = table.Column<string>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_santa_event", x => x.uid);
                });

            migrationBuilder.CreateTable(
                name: "santa_event_membership",
                columns: table => new
                {
                    event_uid = table.Column<string>(type: "TEXT", nullable: false),
                    telegram_login = table.Column<string>(type: "TEXT", nullable: false),
                    opponent_telegram_login = table.Column<string>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_santa_event_membership", x => new { x.event_uid, x.telegram_login });
                });

            migrationBuilder.CreateIndex(
                name: "IX_santa_event_membership_telegram_login_event_uid",
                table: "santa_event_membership",
                columns: new[] { "telegram_login", "event_uid" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "santa_event");

            migrationBuilder.DropTable(
                name: "santa_event_membership");
        }
    }
}
