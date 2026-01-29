using System;

namespace Domain;

public class Activity
{
    // Activity
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string Title { get; set; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool IsCancelled { get; set; }

    // Location
    public required string City { get; set; }
    public required string Venue { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}