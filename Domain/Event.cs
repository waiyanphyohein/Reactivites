using System;

namespace Domain;

public class Event : Group
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public required string EventName { get; set; }
    public string? EventDescription { get; set; }
    public string? Location { get; set; }
    public List<Tag>? Tags { get; set; }
    public List<Person>? Registration { get; set; }
}