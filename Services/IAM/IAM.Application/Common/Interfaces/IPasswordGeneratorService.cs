namespace IAM.Application.Common.Interfaces;

public interface IPasswordGeneratorService
{
    string GenerateSecureTemporaryPassword(int length = 12);
}