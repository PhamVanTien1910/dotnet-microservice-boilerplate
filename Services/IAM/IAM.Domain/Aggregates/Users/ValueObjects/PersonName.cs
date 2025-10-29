using BuildingBlocks.Domain.Exceptions;

namespace IAM.Domain.Aggregates.Users.ValueObjects;

public record PersonName
{
    public string FirstName { get; }
    public string LastName { get; }

    private PersonName(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public static PersonName Create(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
        {
            throw new BadRequestException("Invalid person name. First and last names cannot be empty.");
        }
        return new PersonName(firstName.Trim(), lastName.Trim());
    }

    public string GetFullName() => $"{FirstName} {LastName}";
}