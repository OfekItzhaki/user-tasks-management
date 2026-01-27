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
END
GO
