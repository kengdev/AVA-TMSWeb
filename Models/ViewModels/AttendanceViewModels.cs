using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TMSWeb.Models.ViewModels;
/*public class AttendanceViewModel
{
    public int id { get; set; }
    public string title { get; set; }
    public DateTime start { get; set; }
    public DateTime end { get; set; }
    public bool allDay { get; set; }
    public string color { get; set; }
}

public class AttendanceResourceViewModel
{
    public string id { get; set; }
    public string title { get; set; }
}*/

public class AttendanceTimelineViewModel
{
    public string resourceId { get; set; }
    public string title { get; set; }
    public DateTime start { get; set; }
    public DateTime end { get; set; }
    public string color { get; set; }
}

public class AttendanceSummaryViewModel
{
    public int total_days { get; set; }
    public decimal leave_days { get; set; }
    public int absent_days { get; set; }
    public int late { get; set; }
    public decimal secure_balance { get; set; }
}

public class resource
{
    public string Id { get; set; }
    public string Name { get; set; }
}

public class events
{
    public string Id { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string Text { get; set; }
    public string? Color { get; set; }
    public string? CssClass { get; set; }
    public bool is_pass { get; set; }
    [JsonPropertyName("resource")]
    public string ResourceId { get; set; }
}