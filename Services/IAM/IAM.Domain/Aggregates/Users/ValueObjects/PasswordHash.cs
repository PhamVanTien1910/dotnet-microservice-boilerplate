using BuildingBlocks.Domain.Exceptions;

namespace IAM.Domain.Aggregates.Users.ValueObjects
{
    public record PasswordHash
    {
        public string Value { get; init; }

        private PasswordHash(string value) => Value = value;

        public static PasswordHash Create(string hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
            {
                throw new BadRequestException("Password hash cannot be empty.");
            }

            return new PasswordHash(hash);
        }
    }
}
