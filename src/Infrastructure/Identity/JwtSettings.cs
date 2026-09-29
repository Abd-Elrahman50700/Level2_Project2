namespace Infrastructure.Identity;

public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    public string Secret { get; set; } = "SuperSecretKeyForJwtTokenGenerationMustBeAtLeast32BytesLong!";
    public string Issuer { get; set; } = "TaskManagementApi";
    public string Audience { get; set; } = "TaskManagementApiUsers";
    public int AccessTokenExpirationMinutes { get; set; } = 60;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
