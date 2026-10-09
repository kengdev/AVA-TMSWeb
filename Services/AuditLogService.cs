using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Text.Json;
using TMSWeb.Models;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Services;

public class AuditLogService : IAuditLogService
{
    private readonly DbavalogContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditLogService(
        DbavalogContext db,
        IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task AddAsync(
        AuditLogEntry entry,
        CancellationToken cancellationToken = default)
    {
        var context = _httpContextAccessor.HttpContext;
        var user = context?.User;

        var log = new SystemAuditLog
        {
            EventId = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,

            UserId = user?.FindFirstValue(ClaimTypes.NameIdentifier),
            Username = user?.Identity?.Name,

            ModuleName = entry.ModuleName,
            ActionName = entry.ActionName,

            EntityName = entry.EntityName,
            EntityId = entry.EntityId,

            Description = entry.Description,

            OldValues = entry.OldValues is null
                ? null
                : JsonSerializer.Serialize(entry.OldValues),

            NewValues = entry.NewValues is null
                ? null
                : JsonSerializer.Serialize(entry.NewValues),

            IpAddress = context?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = context?.Request.Headers.UserAgent.ToString(),

            RequestMethod = context?.Request.Method,
            RequestPath = context?.Request.Path.Value,

            Result = entry.Result,
            ErrorMessage = entry.ErrorMessage,

            CorrelationId = context?.TraceIdentifier
        };

        await _db.SystemAuditLogs.AddAsync(log, cancellationToken);
    }
}