using System.Text.Json;
using TornBounty.Models;

namespace TornBounty.Services;

public sealed class TornApiService : IDisposable
{
    private const string ApiRoot = "https://api.torn.com/v2";
    private const string BountiesUrl = $"{ApiRoot}/torn/bounties";
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // Torn allows ~100 calls/minute per player across ALL their keys and tools.
    // Stay below that so other tools (and the browser) still have headroom.
    private const int MaxRequestsPerWindow = 60;
    private static readonly TimeSpan RateLimitWindow = TimeSpan.FromSeconds(60);
    private readonly Queue<DateTime> _recentRequests = new();
    private readonly SemaphoreSlim _rateLimitLock = new(1, 1);

    /// <summary>Raised with the wait time when a request is held back to respect the rate limit.</summary>
    public event Action<TimeSpan>? RateLimitWaiting;

    public TornApiService()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "TornBounty/1.0");
    }

    public Task<BountyResponse> GetBountiesAsync(string apiKey, CancellationToken cancellationToken = default)
    {
        var url = $"{BountiesUrl}?key={Uri.EscapeDataString(apiKey)}";
        return FetchAsync<BountyResponse>(url, cancellationToken);
    }

    public Task<BountyResponse> GetBountiesByUrlAsync(string url, string apiKey, CancellationToken cancellationToken = default)
    {
        // The pagination URLs from the API don't include the key, so append it
        var separator = url.Contains('?') ? "&" : "?";
        var fullUrl = $"{url}{separator}key={Uri.EscapeDataString(apiKey)}";
        return FetchAsync<BountyResponse>(fullUrl, cancellationToken);
    }

    /// <summary>
    /// Fetches a player's profile (status, rank, level) and popular personal stats (crimes, networth)
    /// in a single call, so the battle stats estimate costs no extra rate limit.
    /// </summary>
    public Task<UserLookupResponse> GetUserLookupAsync(int userId, string apiKey, CancellationToken cancellationToken = default)
    {
        var url = $"{ApiRoot}/user?selections=profile,personalstats&cat=popular&id={userId}&key={Uri.EscapeDataString(apiKey)}";
        return FetchAsync<UserLookupResponse>(url, cancellationToken);
    }

    private async Task WaitForRateLimitAsync(CancellationToken cancellationToken)
    {
        // Held for the whole wait so queued requests go out in order once slots free up
        await _rateLimitLock.WaitAsync(cancellationToken);
        try
        {
            while (true)
            {
                var now = DateTime.UtcNow;
                while (_recentRequests.Count > 0 && now - _recentRequests.Peek() >= RateLimitWindow)
                    _recentRequests.Dequeue();

                if (_recentRequests.Count < MaxRequestsPerWindow)
                {
                    _recentRequests.Enqueue(now);
                    return;
                }

                var wait = _recentRequests.Peek() + RateLimitWindow - now;
                RateLimitWaiting?.Invoke(wait);
                await Task.Delay(wait, cancellationToken);
            }
        }
        finally
        {
            _rateLimitLock.Release();
        }
    }

    private async Task<T> FetchAsync<T>(string url, CancellationToken cancellationToken)
    {
        await WaitForRateLimitAsync(cancellationToken);
        using var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        ThrowIfApiError(json);
        return JsonSerializer.Deserialize<T>(json, JsonOptions)
            ?? throw new InvalidOperationException($"Failed to deserialize {typeof(T).Name}.");
    }

    // Torn reports errors with HTTP 200 and a body like {"error":{"code":2,"error":"Incorrect key"}}
    private static void ThrowIfApiError(string json)
    {
        using var doc = JsonDocument.Parse(json);
        if (!doc.RootElement.TryGetProperty("error", out var error))
            return;

        var code = error.TryGetProperty("code", out var codeProp) && codeProp.TryGetInt32(out var c) ? c : -1;
        var message = error.TryGetProperty("error", out var msgProp) ? msgProp.GetString() : null;
        throw new TornApiException(code, message ?? "Unknown error");
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        _rateLimitLock.Dispose();
    }
}
