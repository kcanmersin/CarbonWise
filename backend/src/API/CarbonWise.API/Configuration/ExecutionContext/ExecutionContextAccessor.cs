using CarbonWise.BuildingBlocks.Application;
using System.Security.Claims;

namespace CarbonWise.API.Configuration.ExecutionContext
{
    public class ExecutionContextAccessor : IExecutionContextAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ExecutionContextAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid UserId
        {
            get
            {
                if (_httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true)
                {
                    var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                    if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid userId))
                    {
                        return userId;
                    }
                }

                return Guid.Empty;
            }
        }

        public string UserName
        {
            get
            {
                if (_httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true)
                {
                    return _httpContextAccessor.HttpContext.User.Identity.Name;
                }

                return string.Empty;
            }
        }

        public bool IsAuthenticated
        {
            get
            {
                return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;
            }
        }

        public string CorrelationId
        {
            get
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
                    {
                        return correlationId.First();
                    }
                }

                return Guid.NewGuid().ToString();
            }
        }
    }
}