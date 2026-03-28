namespace SportsStore.Application.Common.Dtos;

public sealed class OrderStatusTimelineEntryDto
{
    public string EventType { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime OccurredAtUtc { get; set; }

    public string? Detail { get; set; }
}
