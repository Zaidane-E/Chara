using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddHabitSortOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Frequency",
                table: "Habits",
                newName: "SortOrder");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SortOrder",
                table: "Habits",
                newName: "Frequency");
        }
    }
}
