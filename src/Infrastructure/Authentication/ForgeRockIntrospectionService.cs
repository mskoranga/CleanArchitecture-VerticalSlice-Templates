using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.VerticalSlice.Infrastructure.Authentication;

public interface IIntrospectionService
{
    Task<bool> IsActiveAsync(string token, CancellationToken cancellationToken = default);
}

public class ForgeRockIntrospectionService : IIntrospectionService
{
    private readonly HttpClient _httpClient;
    private readonly ForgeRockOptions _options;
    private readonly IMemoryCache _cache;

    public ForgeRockIntrospectionService(
        HttpClient httpClient,
        IOptions<ForgeRockOptions> options,
        IMemoryCache cache)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _cache = cache;
    }

    public async Task<bool> IsActiveAsync(string token, CancellationToken cancellationToken = default)
    {
        if (!_options.EnableTokenCaching)
        {
            return await IntrospectTokenAsync(token, cancellationToken);
        }

        var cacheKey = $"token_active_{token}";

        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.CacheDurationMinutes);
            return await IntrospectTokenAsync(token, cancellationToken);
        });
    }

    private async Task<bool> IntrospectTokenAsync(string token, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, _options.IntrospectionEndpoint);
        
        var authHeaderValue = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{_options.ClientId}:{_options.ClientSecret}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authHeaderValue);

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("token", token)
        });

        request.Content = content;

        try
        {
            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return false;

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(json);
            
            return document.RootElement.TryGetProperty("active", out var activeElement) && activeElement.GetBoolean();
        }
        catch
        {
            return false;
        }
    }
}
