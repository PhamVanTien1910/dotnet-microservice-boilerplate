using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BuildingBlocks.Domain.Repositories;
using IAM.Application.Common.Interfaces;
using IAM.Domain.Aggregates.Users.Entities;
using IAM.Domain.Aggregates.Users.ValueObjects;
using IAM.Infrastructure.Configurations;

namespace IAM.Infrastructure.Data;

public class DatabaseSeeder
{
    private readonly IAMDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DatabaseSeeder> _logger;
    private readonly SeedUsersSettings _seedUsers;

    public DatabaseSeeder(
        IAMDbContext context,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        ILogger<DatabaseSeeder> logger,
        IOptions<SeedUsersSettings> seedUsers)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _seedUsers = seedUsers.Value;
    }

    public async Task MigrateAndSeedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[SEEDING] Starting database migration and seeding...");
        await _context.Database.MigrateAsync(cancellationToken);

        if (await _context.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        await SeedDataAsync(cancellationToken);
    }

    private async Task SeedDataAsync(CancellationToken ct)
    {
        if (_seedUsers.Users.Count > 0)
        {
            await SeedMultipleUsersAsync(ct);
        }
    }

    private async Task SeedMultipleUsersAsync(CancellationToken ct)
    {
        foreach (var userData in _seedUsers.Users)
        {
            try
            {
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.Value == userData.Email, ct);

                if (existingUser != null)
                {
                    continue;
                }

                var name = PersonName.Create(userData.FirstName, userData.LastName);
                var email = Email.Create(userData.Email);
                var passwordHash = PasswordHash.Create(_passwordHasher.HashPassword(userData.Password));
                var phoneNumber = PhoneNumber.Create(userData.PhoneNumber);

                var user = User.Create(
                    name,
                    email,
                    phoneNumber,
                    passwordHash,
                    userData.Role
                );

                user.ConfirmEmail();

                _context.Users.Add(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SEEDING] Failed to seed user '{Email}'.", userData.Email);
            }
        }

        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("[SEEDING] Database seeding completed.");
    }
}