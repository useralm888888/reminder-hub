using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations;

public partial class AddReminderVersion : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "Version",
            table: "reminders",
            type: "integer",
            nullable: false,
            defaultValue: 0);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Version",
            table: "reminders");
    }
}
