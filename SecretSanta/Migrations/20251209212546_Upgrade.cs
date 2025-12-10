using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecretSanta.Migrations
{
    /// <inheritdoc />
    public partial class Upgrade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_santa_event_membership_santa_event_event_uid",
                table: "santa_event_membership",
                column: "event_uid",
                principalTable: "santa_event",
                principalColumn: "uid",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_santa_event_membership_santa_event_event_uid",
                table: "santa_event_membership");
        }
    }
}
