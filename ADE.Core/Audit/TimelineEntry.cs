namespace ADE.Core.Audit;

public class TimelineEntry
{
    public DateTime TimestampUtc { get; set; }

    public string EventType { get; set; } = "";

    public string Description { get; set; } = "";
}