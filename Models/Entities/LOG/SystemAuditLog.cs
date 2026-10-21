using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class SystemAuditLog
{
    public long AuditLogId { get; set; }

    public Guid EventId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public string? UserId { get; set; }

    public string? Username { get; set; }

    public string ModuleName { get; set; } = null!;

    public string ActionName { get; set; } = null!;

    public string? EntityName { get; set; }

    public string? EntityId { get; set; }

    public string? Description { get; set; }

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public string? RequestMethod { get; set; }

    public string? RequestPath { get; set; }

    public string Result { get; set; } = null!;

    public string? ErrorMessage { get; set; }

    public string? CorrelationId { get; set; }
}
