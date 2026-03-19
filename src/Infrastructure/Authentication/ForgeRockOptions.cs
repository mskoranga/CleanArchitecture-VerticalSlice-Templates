namespace CleanArchitecture.VerticalSlice.Infrastructure.Authentication;

public class ForgeRockOptions
{
    public string Authority { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string IntrospectionEndpoint { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public bool EnableTokenCaching { get; set; } = true;
    public bool AutoRefreshTokens { get; set; } = true;
    public int CacheDurationMinutes { get; set; } = 5;
}
