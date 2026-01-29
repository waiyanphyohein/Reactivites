using System;

namespace Domain;

public class Tag {
    public Guid TagId { get; set; } = Guid.NewGuid();
    public required string TagName { get; set; }
}