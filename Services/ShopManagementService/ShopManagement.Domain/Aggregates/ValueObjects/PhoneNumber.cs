using System.Text.RegularExpressions;
using BuildingBlocks.Domain.Exceptions;

namespace ShopManagement.Domain.Aggregates.ValueObjects
{
    public record PhoneNumber
    {
        public string Value { get; }

        private PhoneNumber(string value) => Value = value;

        public static PhoneNumber Create(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                throw new BadRequestException("The field 'phone_number' cannot be empty.");
            }

            var normalized = NormalizePhoneNumber(phoneNumber);

            if (!IsValidPhoneNumber(normalized))
            {
                throw new BadRequestException("Invalid phone number format.");
            }

            return new PhoneNumber(normalized);
        }

        private static string NormalizePhoneNumber(string phoneNumber)
        {
            return Regex.Replace(phoneNumber, @"[\s\-\(\)\.]", "");
        }

        private static bool IsValidPhoneNumber(string phoneNumber)
        {
            // International phone number validation
            // Supports formats like: +1234567890, +0123456789, 1234567890
            // Minimum 7 digits, maximum 15 digits
            // Optional + at the beginning, allows 0 as first digit for country codes
            return Regex.IsMatch(phoneNumber, @"^\+?[0-9]\d{6,14}$");
        }

        public static implicit operator string(PhoneNumber phoneNumber) => phoneNumber.Value;
    }
}