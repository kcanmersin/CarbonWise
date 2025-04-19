using CarbonWise.BuildingBlocks.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CarbonWise.API.Configuration.Validation
{
    public class BusinessRuleValidationExceptionProblemDetails : ProblemDetails
    {
        public BusinessRuleValidationExceptionProblemDetails(BusinessRuleValidationException exception)
        {
            Title = "Business rule validation error";
            Status = StatusCodes.Status409Conflict;
            Detail = exception.Message;
            Type = "https://somedomain/business-rule-validation-error";
            BrokenRule = exception.BrokenRule.GetType().Name;
        }

        public string BrokenRule { get; }
    }
}