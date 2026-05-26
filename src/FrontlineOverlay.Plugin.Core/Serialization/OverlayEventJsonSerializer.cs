using System.Text.Encodings.Web;
using System.Text.Json;
using FrontlineOverlay.Plugin.Core.Events;

namespace FrontlineOverlay.Plugin.Core.Serialization;

public static class OverlayEventJsonSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false,
    };

    public static string Serialize(OverlayEventEnvelope payload)
    {
        return JsonSerializer.Serialize(payload, payload.GetType(), Options);
    }
}
