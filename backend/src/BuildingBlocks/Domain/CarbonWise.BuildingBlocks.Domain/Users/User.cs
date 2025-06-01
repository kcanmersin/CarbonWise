using System;
using System.Collections.Generic;

namespace CarbonWise.BuildingBlocks.Domain.Users
{
    public class User : Entity, IAggregateRoot
    {
        public UserId Id { get; private set; }
        public string Username { get; private set; }
        public string Name { get; private set; }
        public string Surname { get; private set; }
        public string Email { get; private set; }
        public string Gender { get; private set; }
        public bool IsInInstitution { get; private set; }
        public bool IsStudent { get; private set; }
        public bool IsAcademicPersonal { get; private set; }
        public bool IsAdministrativeStaff { get; private set; }
        public string UniqueId { get; private set; }
        public int? SustainabilityPoint { get; private set; }
        public string ApiKey { get; private set; }
        public string PasswordHash { get; private set; }
        public UserRole Role { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? LastLoginAt { get; private set; }

        protected User() { }

        private User(
            UserId id,
            string username,
            string name,
            string surname,
            string email,
            string gender,
            bool isInInstitution,
            bool isStudent,
            bool isAcademicPersonal,
            bool isAdministrativeStaff,
            string uniqueId,
            int? sustainabilityPoint,
            string apiKey,
            string passwordHash,
            UserRole role)
        {
            Id = id;
            Username = username;
            Name = name;
            Surname = surname;
            Email = email;
            Gender = gender;
            IsInInstitution = isInInstitution;
            IsStudent = isStudent;
            IsAcademicPersonal = isAcademicPersonal;
            IsAdministrativeStaff = isAdministrativeStaff;
            UniqueId = uniqueId;
            SustainabilityPoint = sustainabilityPoint;
            ApiKey = apiKey;
            PasswordHash = passwordHash;
            Role = role;
            CreatedAt = DateTime.UtcNow;
        }

        public static User Create(string username, string email, string passwordHash, UserRole role)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be empty", nameof(username));
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty", nameof(email));
            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("Password cannot be empty", nameof(passwordHash));

            var user = new User(
                new UserId(Guid.NewGuid()),
                username: username,
                name: "",
                surname: "",
                email: email,
                gender: "Other",
                isInInstitution: false,
                isStudent: false,
                isAcademicPersonal: false,
                isAdministrativeStaff: false,
                uniqueId: email,
                sustainabilityPoint: null,
                apiKey: GenerateApiKey(),
                passwordHash: passwordHash,
                role: role
            );

            user.AddDomainEvent(new UserCreatedDomainEvent(user.Id));
            return user;
        }

        public static User CreateFromOAuth(
            string username,
            string name,
            string surname,
            string email,
            string gender,
            bool isInInstitution,
            bool isStudent,
            bool isAcademicPersonal,
            bool isAdministrativeStaff,
            string uniqueId)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be empty", nameof(username));
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty", nameof(email));

            var user = new User(
                new UserId(Guid.NewGuid()),
                username: username,
                name: name ?? "",
                surname: surname ?? "",
                email: email,
                gender: gender ?? "Other",
                isInInstitution: isInInstitution,
                isStudent: isStudent,
                isAcademicPersonal: isAcademicPersonal,
                isAdministrativeStaff: isAdministrativeStaff,
                uniqueId: uniqueId ?? email,
                sustainabilityPoint: null,
                apiKey: GenerateApiKey(),
                passwordHash: GenerateRandomPasswordHash(),
                role: UserRole.User
            );

            user.AddDomainEvent(new UserCreatedDomainEvent(user.Id));
            return user;
        }

        public void UpdateLastLogin()
        {
            LastLoginAt = DateTime.UtcNow;
        }

        public void ChangeRole(UserRole newRole)
        {
            Role = newRole;
        }

        public void UpdateProfile(
            string name,
            string surname,
            string gender,
            bool isInInstitution,
            bool isStudent,
            bool isAcademicPersonal,
            bool isAdministrativeStaff)
        {
            Name = name ?? "";
            Surname = surname ?? "";
            Gender = gender ?? "Other";
            IsInInstitution = isInInstitution;
            IsStudent = isStudent;
            IsAcademicPersonal = isAcademicPersonal;
            IsAdministrativeStaff = isAdministrativeStaff;
        }

        public void UpdateSustainabilityPoint(int points)
        {
            if (points < 0)
                throw new ArgumentException("Sustainability points cannot be negative", nameof(points));

            SustainabilityPoint = points;
        }

        public void ClearSustainabilityPoint()
        {
            SustainabilityPoint = null;
        }

        private static string GenerateApiKey()
        {
            byte[] secretKeyBytes = new byte[32];
            System.Security.Cryptography.RandomNumberGenerator.Fill(secretKeyBytes);
            return Convert.ToBase64String(secretKeyBytes);
        }

        private static string GenerateRandomPasswordHash()
        {
            var bytes = new byte[32];
            System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes);
        }
    }
}