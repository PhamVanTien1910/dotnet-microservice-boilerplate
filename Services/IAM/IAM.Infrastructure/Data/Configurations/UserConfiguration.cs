using IAM.Domain.Aggregates.Users.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAM.Infrastructure.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            // Primary key
            builder.HasKey(u => u.Id);
            builder.Property(u => u.Id)
                .HasColumnName("Id")
                .ValueGeneratedNever();

            // PersonName value object (required)
            builder.OwnsOne(u => u.Name, name =>
            {
                name.Property(n => n.FirstName)
                    .HasColumnName("FirstName")
                    .HasMaxLength(100)
                    .IsRequired();

                name.Property(n => n.LastName)
                    .HasColumnName("LastName")
                    .HasMaxLength(100)
                    .IsRequired();
            });

            // Email value object (required, unique)
            builder.OwnsOne(u => u.Email, email =>
            {
                email.Property(e => e.Value)
                    .HasColumnName("Email")
                    .HasMaxLength(255)
                    .IsRequired();
                email.HasIndex(e => e.Value)
                    .IsUnique()
                    .HasDatabaseName("IX_Users_Email");
            });

            // PhoneNumber value object (optional)
            builder.OwnsOne(u => u.PhoneNumber, phoneNumber =>
            {
                phoneNumber.Property(p => p.Value)
                    .HasColumnName("PhoneNumber")
                    .HasMaxLength(20);
            });

            // Avatar URL (optional)
            builder.Property(u => u.AvatarUrl)
                .HasColumnName("AvatarUrl")
                .HasMaxLength(500);

            // Role property (required)
            builder.Property(u => u.Role)
                .HasColumnName("Role")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            // Credentials
            builder.OwnsOne(u => u.PasswordHash, ph =>
            {
                ph.Property(p => p.Value)
                    .HasColumnName("PasswordHash")
                    .HasMaxLength(512)
                    .IsRequired();
            });

            // Email confirmation token (owned)
            builder.OwnsOne(u => u.EmailConfirmationToken, ect =>
            {
                ect.Ignore(t => t.PlainValue);
                ect.WithOwner();
                ect.Property(t => t.HashedValue)
                    .HasColumnName("EmailConfirmationTokenHash")
                    .HasMaxLength(512);
                ect.Property(t => t.ExpiresAt)
                    .HasColumnName("EmailConfirmationTokenExpiresAt");
                ect.Ignore(t => t.Type);
            });

            // Password reset token (owned)
            builder.OwnsOne(u => u.PasswordResetToken, prt =>
            {
                prt.Ignore(t => t.PlainValue);
                prt.WithOwner();
                prt.Property(t => t.HashedValue)
                    .HasColumnName("PasswordResetTokenHash")
                    .HasMaxLength(512);
                prt.Property(t => t.ExpiresAt)
                    .HasColumnName("PasswordResetTokenExpiresAt");
                prt.Ignore(t => t.Type);
            });

            builder.Property(u => u.IsEmailConfirmed)
                .HasColumnName("IsEmailConfirmed")
                .HasDefaultValue(false)
                .IsRequired();

            // Account status
            builder.Property(u => u.LastLoginAt)
                .HasColumnName("LastLoginAt");

            // Audit properties (ICreatedAuditable, IModifiedAuditable)
            builder.Property(u => u.CreatedAt)
                .HasColumnName("CreatedAt")
                .IsRequired();

            builder.Property(u => u.ModifiedAt)
                .HasColumnName("ModifiedAt");

            // Soft delete properties (ISoftDeletable)
            builder.Property(u => u.IsDeleted)
                .HasColumnName("IsDeleted")
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(u => u.DeletedAt)
                .HasColumnName("DeletedAt");

            builder.HasMany(provider => provider.Sessions)
                .WithOne(session => session.User)
                .HasForeignKey(session => session.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Performance indexes
            builder.HasIndex(u => u.IsDeleted)
                .HasDatabaseName("IX_Users_IsDeleted");

            builder.HasIndex(u => u.LastLoginAt)
                .HasDatabaseName("IX_Users_LastLoginAt");

            builder.HasIndex(u => u.Role)
                .HasDatabaseName("IX_Users_Role");

        }
    }
}
