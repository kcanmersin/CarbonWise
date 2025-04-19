using CarbonWise.BuildingBlocks.Application;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CarbonWise.API.Configuration.Validation
{
    public class InvalidCommandProblemDetails : ProblemDetails
    {
        public InvalidCommandProblemDetails(InvalidCommandException exception)
        {
            Title = "Command validation error";
            Status = StatusCodes.Status400BadRequest;
            Detail = exception.Message;
            Type = "https://somedomain/validation-error";
            Errors = exception.Errors;
        }

        public IDictionary<string, string[]> Errors { get; }
    }
}