namespace CarbonWise.BuildingBlocks.Application.Users.Login
{
    public class AuthenticationResult
    {
        public UserDto User { get; set; }
        public string Token { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }

        public static AuthenticationResult SuccessResult(UserDto user, string token)
        {
            return new AuthenticationResult
            {
                User = user,
                Token = token,
                Success = true
            };
        }

        public static AuthenticationResult FailureResult(string errorMessage)
        {
            return new AuthenticationResult
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }
    }
}