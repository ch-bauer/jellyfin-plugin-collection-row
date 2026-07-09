using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.CollectionSection.Model
{
    public class PatchRequestPayload
    {
        [JsonPropertyName("contents")]
        public string? Contents { get; set; }
    }
}
