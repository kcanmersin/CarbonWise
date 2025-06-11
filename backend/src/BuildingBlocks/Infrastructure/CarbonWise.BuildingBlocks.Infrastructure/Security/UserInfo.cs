using CarbonWise.BuildingBlocks.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonWise.BuildingBlocks.Infrastructure.Security
{
    public class UserInfo
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public UserRole Role { get; set; }
        public bool IsInInstitution { get; set; }
        public bool IsStudent { get; set; }
        public bool IsAcademicPersonal { get; set; }
        public bool IsAdministrativeStaff { get; set; }
        public string UniqueId { get; set; }
        public int? SustainabilityPoint { get; set; }
    }
}
