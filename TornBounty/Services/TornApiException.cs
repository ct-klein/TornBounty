namespace TornBounty.Services;

/// <summary>
/// Raised when the Torn API returns an error payload (which it does with HTTP 200).
/// </summary>
public sealed class TornApiException(int code, string apiMessage)
    : Exception($"Torn API error {code}: {apiMessage}")
{
    public int Code { get; } = code;
    public string ApiMessage { get; } = apiMessage;
}
