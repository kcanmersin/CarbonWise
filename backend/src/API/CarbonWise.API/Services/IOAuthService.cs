using CarbonWise.BuildingBlocks.Domain.Users;
using CarbonWise.BuildingBlocks.Infrastructure;
using CarbonWise.BuildingBlocks.Infrastructure.Security;
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
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        private readonly string clientId = "0367F395212941E28DA342B6F28BC3BC";
        private readonly string clientSecret = "D9D8BE412F0A477B9B182E5AA378153F";
        private readonly string redirectUri = "http://localhost:3000/auth";

        private readonly string authorizationEndpoint = "https://kampus.gtu.edu.tr/oauth/yetki";
        private readonly string tokenEndpoint = "https://kampus.gtu.edu.tr/oauth/dogrulama";
        private readonly string queryServerAddress = "https://kampus.gtu.edu.tr/oauth/sorgu";

        public OAuthService(
            IUserRepository userRepository,
            AppDbContext dbContext,
            IJwtTokenGenerator jwtTokenGenerator)
        {
            _userRepository = userRepository;
            _dbContext = dbContext;
            _jwtTokenGenerator = jwtTokenGenerator;
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

                var user = await ControlUser(userInfo);
                if (user == null)
                    return OAuthResult.Fail(400, "User creation or retrieval failed.");

                var token = _jwtTokenGenerator.GenerateToken(user);

                return OAuthResult.SuccessResult(user, token);
            }
            catch (Exception ex)
            {
                return OAuthResult.Fail(500, $"An error occurred: {ex.Message}");
            }
        }

        private async Task<User?> ControlUser(JToken userInfo)
        {
            var userEmail = userInfo["kurumsal_email_adresi"]?.ToString() ?? "";
            var userName = userInfo["kullanici_adi"]?.ToString() ?? "";
            var name = userInfo["ad"]?.ToString() ?? "";
            var surname = userInfo["soyad"]?.ToString() ?? "";
            var gender = userInfo["cinsiyet"]?.ToString() ?? "Other";
            var uniqueId = userInfo["tc_kimlik_no"]?.ToString() ?? userEmail;

            bool isStudent = userInfo["ogrenci"]?.ToObject<bool>() ?? false;
            bool isAcademicPersonal = userInfo["akademik_personel"]?.ToObject<bool>() ?? false;
            bool isAdministrativeStaff = userInfo["idari_personel"]?.ToObject<bool>() ?? false;
            bool isInInstitution = isStudent || isAcademicPersonal || isAdministrativeStaff;

            if (string.IsNullOrEmpty(userEmail))
                return null;

            var existingUser = await _userRepository.GetByEmailAsync(userEmail);

            if (existingUser != null)
            {
                existingUser.UpdateLastLogin();
                existingUser.UpdateProfile(name, surname, gender, isInInstitution, isStudent, isAcademicPersonal, isAdministrativeStaff);
                await _userRepository.UpdateAsync(existingUser);
                await _dbContext.SaveChangesAsync();
                return existingUser;
            }

            var newUser = User.CreateFromOAuth(
                userName,
                name,
                surname,
                userEmail,
                gender,
                isInInstitution,
                isStudent,
                isAcademicPersonal,
                isAdministrativeStaff,
                uniqueId
            );

            await _userRepository.AddAsync(newUser);
            await _dbContext.SaveChangesAsync();

            return newUser;
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
            
            // Use standard Base64 encoding (not Base64URL)
            var base64String = Convert.ToBase64String(hashBytes);
            
            // Apply percent encoding as required by GTÜ documentation
            var percentEncoded = Uri.EscapeDataString(base64String);
            
            return percentEncoded;
        }
    }

    public class OAuthResult
    {
        public bool Success { get; set; }
        public User User { get; set; }
        public string Token { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;

        public static OAuthResult SuccessResult(User user, string token) => new OAuthResult
        {
            Success = true,
            User = user,
            Token = token,
            StatusCode = 200
        };

        public static OAuthResult Fail(int statusCode, string error) => new OAuthResult
        {
            Success = false,
            StatusCode = statusCode,
            ErrorMessage = error
        };
    }
}