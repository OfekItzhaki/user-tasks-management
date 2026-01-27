using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStoredProcedure_GetTasksWithMultipleTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop procedure if exists
            migrationBuilder.Sql(@"
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetTasksWithMultipleTags]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[GetTasksWithMultipleTags]");

            // Create stored procedure
            migrationBuilder.Sql(@"
CREATE PROCEDURE [dbo].[GetTasksWithMultipleTags]
    @MinTagCount INT = 2
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        t.Id,
        t.Title,
        t.Description,
        t.DueDate,
        t.Priority,
        COUNT(DISTINCT tt.TagId) AS TagCount,
        STRING_AGG(tag.Name, ', ') WITHIN GROUP (ORDER BY tag.Name) AS TagNames,
        COUNT(DISTINCT ut.UserId) AS UserCount,
        STRING_AGG(u.FullName, ', ') WITHIN GROUP (ORDER BY u.FullName) AS AssignedUsers
    FROM Tasks t
    INNER JOIN TaskTags tt ON t.Id = tt.TaskId
    INNER JOIN Tags tag ON tt.TagId = tag.Id
    LEFT JOIN UserTasks ut ON t.Id = ut.TaskId
    LEFT JOIN Users u ON ut.UserId = u.Id
    GROUP BY t.Id, t.Title, t.Description, t.DueDate, t.Priority
    HAVING COUNT(DISTINCT tt.TagId) >= @MinTagCount
    ORDER BY TagCount DESC;
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetTasksWithMultipleTags]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[GetTasksWithMultipleTags]");
        }
    }
}
