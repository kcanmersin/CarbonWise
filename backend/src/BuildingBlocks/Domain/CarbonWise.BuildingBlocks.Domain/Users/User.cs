using System;
using System.Collections.Generic;

namespace CarbonWise.BuildingBlocks.Domain.Users
{
    public class User : Entity, IAggregateRoot
    {
        public UserId Id { get; private set; }
        public string Username { get; private set; }
        public string Email { get; private set; }
        public string PasswordHash { get; private set; }
        public UserRole Role { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? LastLoginAt { get; private set; }

        // Protected constructor for EF Core
        protected User() { }

        private User(UserId id, string username, string email, string passwordHash, UserRole role)
        {
            Id = id;
            Username = username;
            Email = email;
            PasswordHash = passwordHash;
            Role = role;
            CreatedAt = DateTime.UtcNow;
        }

        public static User Create(string username, string email, string passwordHash, UserRole role)
        {
            // Validation rules
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be empty", nameof(username));

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty", nameof(email));

            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("Password cannot be empty", nameof(passwordHash));

            var user = new User(new UserId(Guid.NewGuid()), username, email, passwordHash, role);

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