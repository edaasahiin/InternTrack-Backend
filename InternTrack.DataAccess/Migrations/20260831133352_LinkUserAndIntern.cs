using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternTrack.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class LinkUserAndIntern : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Interns",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Interns_UserId",
                table: "Interns",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Interns_Users_UserId",
                table: "Interns",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Interns_Users_UserId",
                table: "Interns");

            migrationBuilder.DropIndex(
                name: "IX_Interns_UserId",
                table: "Interns");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Interns");
        }
    }
}
