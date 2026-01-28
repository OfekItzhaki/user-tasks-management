using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.DTOs;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Application.Queries.Tasks;

public class GetTasksWithMultipleTagsQueryHandler : IRequestHandler<GetTasksWithMultipleTagsQuery, List<TasksWithTagsDto>>
{
    private readonly TaskManagementDbContext _context;
    private readonly ILogger<GetTasksWithMultipleTagsQueryHandler> _logger;

    public GetTasksWithMultipleTagsQueryHandler(
        TaskManagementDbContext context,
        ILogger<GetTasksWithMultipleTagsQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<TasksWithTagsDto>> Handle(GetTasksWithMultipleTagsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Execute stored procedure: GetTasksWithMultipleTags
            // This stored procedure returns tasks with at least the specified number of tags,
            // including tag names, sorted by number of tags in descending order
            var minTagCountParam = new SqlParameter("@MinTagCount", request.MinTagCount);
            
            _logger.LogInformation("Executing stored procedure GetTasksWithMultipleTags with MinTagCount={MinTagCount}", request.MinTagCount);
            
            var results = await _context.Database
                .SqlQueryRaw<TasksWithTagsDto>(
                    "EXEC [dbo].[GetTasksWithMultipleTags] @MinTagCount",
                    minTagCountParam)
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Stored procedure returned {Count} tasks", results.Count);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing stored procedure GetTasksWithMultipleTags: {Message}", ex.Message);
            
            // If stored procedure doesn't exist, fall back to raw SQL query
            _logger.LogWarning("Falling back to raw SQL query");
            return await ExecuteRawSqlQuery(request.MinTagCount, cancellationToken);
        }
    }

    private async Task<List<TasksWithTagsDto>> ExecuteRawSqlQuery(int minTagCount, CancellationToken cancellationToken)
    {
        // Fallback: Execute the SQL query directly if stored procedure doesn't exist
        var sql = @"
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
            ORDER BY TagCount DESC";

        var minTagCountParam = new SqlParameter("@MinTagCount", minTagCount);
        var results = await _context.Database
            .SqlQueryRaw<TasksWithTagsDto>(sql, minTagCountParam)
            .ToListAsync(cancellationToken);

        return results;
    }
}
