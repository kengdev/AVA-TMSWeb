namespace TMSWeb.Models
{
    public class AuditLogEntry
    {
        public string ModuleName { get; set; } = string.Empty;

        public string ActionName { get; set; } = string.Empty;

        public string? EntityName { get; set; }

        public string? EntityId { get; set; }

        public string? Description { get; set; }

        public object? OldValues { get; set; }

        public object? NewValues { get; set; }

        public string Result { get; set; } = AuditResults.Success;

        public string? ErrorMessage { get; set; }
    }

    public static class AuditActions
    {
        public const string Create = "CREATE";
        public const string Update = "UPDATE";
        public const string Delete = "DELETE";

        public const string Active = "ACTIVE";
        public const string Inactive = "INACTIVE";

        public const string Approve = "APPROVE";
        public const string Cancel = "CANCEL";

        public const string Login = "LOGIN";
        public const string Logout = "LOGOUT";

        public const string View = "VIEW";
        public const string Print = "PRINT";
        public const string Export = "EXPORT";
    }

    public static class AuditResults
    {
        public const string Success = "SUCCESS";
        public const string Failed = "FAILED";
    }
}
