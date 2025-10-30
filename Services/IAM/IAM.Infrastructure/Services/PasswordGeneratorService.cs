using System.Security.Cryptography;
using System.Text;
using IAM.Application.Common.Interfaces;

namespace IAM.Infrastructure.Services;

public class PasswordGeneratorService : IPasswordGeneratorService
{
    private const string LowerCaseChars = "abcdefghijklmnopqrstuvwxyz";
    private const string UpperCaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string NumberChars = "0123456789";
    private const string SpecialChars = "!@#$%^&*";

    public string GenerateSecureTemporaryPassword(int length = 12)
    {
        if (length < 8)
            throw new ArgumentException("Password length must be at least 8 characters", nameof(length));

        var password = new StringBuilder();

        // Ensure at least one character from each category
        password.Append(GetRandomChar(UpperCaseChars));
        password.Append(GetRandomChar(LowerCaseChars));
        password.Append(GetRandomChar(NumberChars));
        password.Append(GetRandomChar(SpecialChars));

        // Fill the rest with random characters from all categories
        var allChars = LowerCaseChars + UpperCaseChars + NumberChars + SpecialChars;
        for (int i = 4; i < length; i++)
        {
            password.Append(GetRandomChar(allChars));
        }

        // Shuffle the password to avoid predictable patterns
        return ShuffleString(password.ToString());
    }

    private static char GetRandomChar(string chars)
    {
        using var rng = RandomNumberGenerator.Create();
        var buffer = new byte[4];
        rng.GetBytes(buffer);
        var randomNumber = BitConverter.ToUInt32(buffer, 0);
        return chars[(int)(randomNumber % chars.Length)];
    }

    private static string ShuffleString(string input)
    {
        var array = input.ToCharArray();
        using var rng = RandomNumberGenerator.Create();

        for (int i = array.Length - 1; i > 0; i--)
        {
            var buffer = new byte[4];
            rng.GetBytes(buffer);
            var randomIndex = (int)(BitConverter.ToUInt32(buffer, 0) % (i + 1));

            // Swap
            (array[i], array[randomIndex]) = (array[randomIndex], array[i]);
        }

        return new string(array);
    }
}