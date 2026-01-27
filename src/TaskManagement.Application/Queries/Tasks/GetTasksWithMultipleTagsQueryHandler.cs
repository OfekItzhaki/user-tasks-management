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
            ORDER BY TagCount DESC";

        var minTagCountParam = new SqlParameter("@MinTagCount", minTagCount);
        var results = await _context.Database
            .SqlQueryRaw<TasksWithTagsDto>(sql, minTagCountParam)
            .ToListAsync(cancellationToken);

        return results;
    }
}
