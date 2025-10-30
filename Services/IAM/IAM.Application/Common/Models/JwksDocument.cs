using System.Text.Json.Serialization;

namespace IAM.Application.Common.Models
{
    public sealed class JwksDocument
    {
        [JsonPropertyName("keys")] public IEnumerable<JsonWebKey> Keys { get; init; } = Array.Empty<JsonWebKey>();
    }

    public sealed class JsonWebKey
    {
        [JsonPropertyName("kty")] public string Kty { get; init; } = string.Empty;
        [JsonPropertyName("use")] public string Use { get; init; } = "sig";
        [JsonPropertyName("alg")] public string Alg { get; init; } = "RS256";
        [JsonPropertyName("kid")] public string Kid { get; init; } = string.Empty;
        [JsonPropertyName("n")] public string N { get; init; } = string.Empty;
        [JsonPropertyName("e")] public string E { get; init; } = string.Empty;
    }
}