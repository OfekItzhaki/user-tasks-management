-- Stored Procedure: GetTasksWithMultipleTags
-- Description: Returns tasks with at least the specified number of tags, including tag names,
--              sorted by number of tags in descending order
-- Parameters:
--   @MinTagCount: Minimum number of tags required (default: 2)

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetTasksWithMultipleTags]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[GetTasksWithMultipleTags]
GO

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
        STUFF((
            SELECT ', ' + tag2.Name
            FROM TaskTags tt2
            INNER JOIN Tags tag2 ON tt2.TagId = tag2.Id
            WHERE tt2.TaskId = t.Id
            ORDER BY tag2.Name
            FOR XML PATH(''), TYPE
        ).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS TagNames,
        COUNT(DISTINCT ut.UserId) AS UserCount,
        STUFF((
            SELECT ', ' + u2.FullName
            FROM UserTasks ut2
            INNER JOIN Users u2 ON ut2.UserId = u2.Id
            WHERE ut2.TaskId = t.Id
            ORDER BY u2.FullName
            FOR XML PATH(''), TYPE
        ).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS AssignedUsers
    FROM Tasks t
    INNER JOIN TaskTags tt ON t.Id = tt.TaskId
    LEFT JOIN UserTasks ut ON t.Id = ut.TaskId
    GROUP BY t.Id, t.Title, t.Description, t.DueDate, t.Priority
    HAVING COUNT(DISTINCT tt.TagId) >= @MinTagCount
    ORDER BY TagCount DESC;
END
GO
