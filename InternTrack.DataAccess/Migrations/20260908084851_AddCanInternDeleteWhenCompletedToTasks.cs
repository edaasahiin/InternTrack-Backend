using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternTrack.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddCanInternDeleteWhenCompletedToTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CanInternDeleteWhenCompleted",
                table: "Tasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanInternDeleteWhenCompleted",
                table: "Tasks");
        }
    }
}
