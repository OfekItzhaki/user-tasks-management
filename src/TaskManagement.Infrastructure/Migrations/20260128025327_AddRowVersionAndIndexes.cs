using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRowVersionAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add RowVersion column for optimistic concurrency control
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Tasks",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            // Add performance indexes
            migrationBuilder.CreateIndex(
                name: "IX_Tasks_DueDate",
                table: "Tasks",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Priority",
                table: "Tasks",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_CreatedAt",
                table: "Tasks",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Title",
                table: "Tasks",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Description",
                table: "Tasks",
                column: "Description");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop indexes
            migrationBuilder.DropIndex(
                name: "IX_Tasks_Description",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_Title",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_CreatedAt",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_Priority",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_DueDate",
                table: "Tasks");

            // Remove RowVersion column
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Tasks");
        }
    }
}
