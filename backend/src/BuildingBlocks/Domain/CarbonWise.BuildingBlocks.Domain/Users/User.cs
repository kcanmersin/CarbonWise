using System;
using System.Collections.Generic;

namespace CarbonWise.BuildingBlocks.Domain.Users
{
    public class User : Entity, IAggregateRoot
    {
        public UserId Id { get; private set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public bool IsInInstitution { get; set; }
        public bool IsStudent { get; set; }
        public bool IsAcademicPersonal { get; set; }
        public bool IsAdministrativeStaff { get; set; }
        public string UniqueId { get; set; }
        public int? SustainabilityPoint { get; set; }
        public string apiKey { get; set; }
        public string PasswordHash { get; private set; }
        public UserRole Role { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? LastLoginAt { get; private set; }

        protected User() { }

        public User(
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
            string apiKey)
        {
            Id = new UserId(Guid.NewGuid());
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
            this.apiKey = apiKey;
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

            var user = new User
            {
                Id = new UserId(Guid.NewGuid()),
                Username = username,
                Email = email,
                PasswordHash = passwordHash,
                Role = role,
                CreatedAt = DateTime.UtcNow
            };

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
    }
}