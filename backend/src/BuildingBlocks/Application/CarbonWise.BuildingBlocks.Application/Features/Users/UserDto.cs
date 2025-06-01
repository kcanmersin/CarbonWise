using System;
using CarbonWise.BuildingBlocks.Domain.Users;

namespace CarbonWise.BuildingBlocks.Application.Users
{
    public class UserDto
    {
        public Guid Id { get; set; }
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
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }
}