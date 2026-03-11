using System.Text.Json;
using TornBounty.Models;

namespace TornBounty.Services;

public sealed class TornApiService : IDisposable
{
    private const string BaseUrl = "https://api.torn.com/v2/torn/bounties";
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public TornApiService()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "TornBounty/1.0");
    }

    public async Task<BountyResponse> GetBountiesAsync(string apiKey, CancellationToken cancellationToken = default)
    {
        var url = $"{BaseUrl}?key={apiKey}";
        return await FetchAsync(url, cancellationToken);
    }

    public async Task<BountyResponse> GetBountiesByUrlAsync(string url, string apiKey, CancellationToken cancellationToken = default)
    {
        // The pagination URLs from the API don't include the key, so append it
        var separator = url.Contains('?') ? "&" : "?";
        var fullUrl = $"{url}{separator}key={apiKey}";
        return await FetchAsync(fullUrl, cancellationToken);
    }

    private async Task<BountyResponse> FetchAsync(string url, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<BountyResponse>(json, JsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize bounty response.");
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
