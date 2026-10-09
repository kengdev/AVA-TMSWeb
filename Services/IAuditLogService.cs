using TMSWeb.Models;

namespace TMSWeb.Services
{
    public interface IAuditLogService
    {
        Task AddAsync(
            AuditLogEntry entry,
            CancellationToken cancellationToken = default);
    }
}
