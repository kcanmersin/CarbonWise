using CarbonWise.BuildingBlocks.Domain.Users;
using CarbonWise.BuildingBlocks.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CarbonWise.API.Services
{
    public interface IOAuthService
    {
        string GenerateLoginUrl(IMemoryCache cache);
        Task<OAuthResult> HandleOAuthRedirect(string state, string code, IMemoryCache cache);
    }

    public class OAuthService : IOAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly AppDbContext _dbContext;
        private readonly string clientId = "FB387A2ABDDD423586FFB6ED157762BB";
        private readonly string clientSecret = "3FF64BC099174847A5FB2EF22E1D0930";
        private readonly string redirectUri = "http://localhost:3000/auth";
        private readonly string authorizationEndpoint = "https://kampus.gtu.edu.tr/oauth/yetki";
        private readonly string tokenEndpoint = "https://kampus.gtu.edu.tr/oauth/dogrulama";
        private readonly string queryServerAddress = "https://kampus.gtu.edu.tr/oauth/sorgulama";

        public OAuthService(IUserRepository userRepository, AppDbContext dbContext)
        {
            _userRepository = userRepository;
            _dbContext = dbContext;
        }

        public string GenerateLoginUrl(IMemoryCache cache)
        {
            var state = Guid.NewGuid().ToString("N");
            var codeVerifier = GenerateCodeVerifier();
            var codeChallenge = GenerateCodeChallenge(codeVerifier);

            cache.Set(state, codeVerifier, TimeSpan.FromMinutes(10));

            return $"{authorizationEndpoint}?response_type=code" +
                   $"&client_id={clientId}" +
                   $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                   $"&state={state}" +
                   $"&code_challenge_method=s256" +
                   $"&code_challenge={codeChallenge}";
        }

        public async Task<OAuthResult> HandleOAuthRedirect(string state, string code, IMemoryCache cache)
        {
            if (string.IsNullOrEmpty(state) || string.IsNullOrEmpty(code))
                return OAuthResult.Fail(400, "Missing state or code.");

            if (!cache.TryGetValue(state, out string codeVerifier))
                return OAuthResult.Fail(400, "Invalid or expired state.");

            cache.Remove(state);

            try
            {
                var accessToken = await ExchangeCodeForToken(code, codeVerifier);

                using var httpClient = new HttpClient();
                var requestData = new Dictionary<string, string>
                {
                    { "client_id", clientId },
                    { "access_token", accessToken },
                    { "kapsam", "GENEL" }
                };

                var jsonData = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(queryServerAddress, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return OAuthResult.Fail((int)response.StatusCode, $"Error fetching user info: {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var userInfo = JToken.Parse(responseContent);

                var newUser = await ControlUser(userInfo);
                if (newUser == null)
                    return OAuthResult.Fail(400, "User creation or retrieval failed.");

                return OAuthResult.SuccessResult(newUser);
            }
            catch (Exception ex)
            {
                return OAuthResult.Fail(500, $"An error occurred: {ex.Message}");
            }
        }

        private async Task<User?> ControlUser(JToken userInfo)
        {
            var user = ConvertToUser(userInfo);
            var existingUser = await _userRepository.GetByEmailAsync(user.Email);

            if (existingUser != null)
                return existingUser;

            user.apiKey = ApiKeyGenerator.GenerateApiKey();
            user.SustainabilityPoint = 0;
            user.UniqueId = user.Email;
            await _userRepository.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            return await _userRepository.GetByEmailAsync(user.Email);
        }

        private User ConvertToUser(JToken userInfo)
        {
            return User.Create(
                userInfo["kullanici_adi"]?.ToString() ?? "",
                userInfo["kurumsal_email_adresi"]?.ToString() ?? "",
                "oauth", 
                UserRole.User
            );
        }

        private async Task<string> ExchangeCodeForToken(string code, string codeVerifier)
        {
            using var httpClient = new HttpClient();
            var requestData = new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "code", code },
                { "code_verifier", codeVerifier }
            };

            var jsonData = JsonConvert.SerializeObject(requestData);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(tokenEndpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Token request failed: {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            dynamic responseJson = JsonConvert.DeserializeObject(responseContent);
            return responseJson.access_token;
        }

        private static string GenerateCodeVerifier()
        {
            var bytes = new byte[64];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private static string GenerateCodeChallenge(string codeVerifier)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
            return Convert.ToBase64String(hashBytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }
    }
    public class OAuthResult
    {
        public bool Success { get; set; }
        public User User { get; set; }
        public int StatusCode { get; set; }
        public string ErrorMessage { get; set; }

        public static OAuthResult SuccessResult(User user) => new OAuthResult { Success = true, User = user, StatusCode = 200 };
        public static OAuthResult Fail(int statusCode, string error) => new OAuthResult { Success = false, StatusCode = statusCode, ErrorMessage = error };
    }

    public static class ApiKeyGenerator
    {
        public static string GenerateApiKey()
        {
            byte[] secretKeyBytes = new byte[32]; // 256-bit
            RandomNumberGenerator.Fill(secretKeyBytes);
            return Convert.ToBase64String(secretKeyBytes);
        }
    }
}
