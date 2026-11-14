namespace SecureNoteApp.Infrastructure.Security;

public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    public string SecretKey { get; set; } = "SecureNoteApp_SuperSecretKey_For_Jwt_Auth_2026_Minimum_32_Chars!";
    public string Issuer { get; set; } = "SecureNoteApp";
    public string Audience { get; set; } = "SecureNoteAppClient";
    public int ExpiryInMinutes { get; set; } = 1440; // 24 hours
}
