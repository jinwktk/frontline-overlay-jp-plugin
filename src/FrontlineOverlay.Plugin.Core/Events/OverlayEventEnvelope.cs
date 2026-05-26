using System.Text.Json.Serialization;

namespace FrontlineOverlay.Plugin.Core.Events;

public abstract record OverlayEventEnvelope
{
    protected OverlayEventEnvelope(string type)
    {
        Type = type;
    }

    [JsonPropertyName("type")]
    public string Type { get; }

    public static ChangeZoneEvent ChangeZone(int zoneId, string zoneName)
    {
        return new ChangeZoneEvent(zoneId, zoneName);
    }

    public static ChangePrimaryPlayerEvent ChangePrimaryPlayer(int charId, string charName)
    {
        return new ChangePrimaryPlayerEvent(charId, charName);
    }

    public static LogLineEvent LogLine(string rawLine)
    {
        return new LogLineEvent(rawLine.Split('|'), rawLine);
    }
}

public sealed record ChangeZoneEvent : OverlayEventEnvelope
{
    public ChangeZoneEvent(int zoneId, string zoneName)
        : base("ChangeZone")
    {
        ZoneId = zoneId;
        ZoneName = zoneName;
    }

    [JsonPropertyName("zoneID")]
    public int ZoneId { get; }

    [JsonPropertyName("zoneName")]
    public string ZoneName { get; }
}

public sealed record ChangePrimaryPlayerEvent : OverlayEventEnvelope
{
    public ChangePrimaryPlayerEvent(int charId, string charName)
        : base("ChangePrimaryPlayer")
    {
        CharId = charId;
        CharName = charName;
    }

    [JsonPropertyName("charID")]
    public int CharId { get; }

    [JsonPropertyName("charName")]
    public string CharName { get; }
}

public sealed record LogLineEvent : OverlayEventEnvelope
{
    public LogLineEvent(IReadOnlyList<string> line, string rawLine)
        : base("LogLine")
    {
        Line = line;
        RawLine = rawLine;
    }

    [JsonPropertyName("line")]
    public IReadOnlyList<string> Line { get; }

    [JsonPropertyName("rawLine")]
    public string RawLine { get; }
}
