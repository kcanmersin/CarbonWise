using System;

namespace CarbonWise.BuildingBlocks.Application.Users
{
    public class UserAlreadyExistsException : Exception
    {
        public UserAlreadyExistsException(string message) : base(message)
        {
        }
    }
}