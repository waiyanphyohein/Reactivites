using System;

namespace Domain;

public class Person
{
    public Guid PersonId { get; set; } = Guid.NewGuid();
    public required string FirstName;
    public string? MiddleName;
    public required string LastName;
    public int Age;
    public DateTime DateOfBirth;
    public string? Address;
    public string? Interests;
}