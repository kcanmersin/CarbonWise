using CarbonWise.BuildingBlocks.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarbonWise.BuildingBlocks.Infrastructure.Users
{
    public class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Id)
                .HasConversion(
                    userId => userId.Value,
                    dbId => new UserId(dbId));

            builder.Property(u => u.Username)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(u => u.Name)
                .HasMaxLength(100)
                .IsRequired(false);

            builder.Property(u => u.Surname)
                .HasMaxLength(100)
                .IsRequired(false);

            builder.Property(u => u.Email)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(u => u.Gender)
                .HasMaxLength(20)
                .IsRequired(false);

            builder.Property(u => u.IsInInstitution)
                .IsRequired();

            builder.Property(u => u.IsStudent)
                .IsRequired();

            builder.Property(u => u.IsAcademicPersonal)
                .IsRequired();

            builder.Property(u => u.IsAdministrativeStaff)
                .IsRequired();

            builder.Property(u => u.UniqueId)
                .HasMaxLength(255)
                .IsRequired(false);

            builder.Property(u => u.SustainabilityPoint)
                .IsRequired(false);

            builder.Property(u => u.ApiKey)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(u => u.PasswordHash)
                .IsRequired();

            builder.Property(u => u.Role)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(u => u.CreatedAt)
                .IsRequired();

            builder.Property(u => u.LastLoginAt)
                .IsRequired(false);

            builder.HasIndex(u => u.Username)
                .IsUnique();

            builder.HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}