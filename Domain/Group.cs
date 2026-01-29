using System;

namespace Domain;

public class Group
{
    public Guid GroupId { get; set; } = Guid.NewGuid();
    public required string GroupName;
    public string? GroupDescription;
    public required List<Person> Organizers;
    public List<Tag>? GroupTags;

    // TODO: Add Group Stats with Data Visualization
}