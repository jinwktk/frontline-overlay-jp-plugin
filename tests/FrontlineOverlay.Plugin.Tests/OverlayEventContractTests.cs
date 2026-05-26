using System.Text.Json;
using FrontlineOverlay.Plugin.Core.Events;
using FrontlineOverlay.Plugin.Core.Serialization;
using FrontlineOverlay.Plugin.Core.State;

namespace FrontlineOverlay.Plugin.Tests;

public sealed class OverlayEventContractTests
{
    [Fact]
    public void ChangeZoneEventUsesOverlayPluginCompatibleShape()
    {
        var payload = OverlayEventEnvelope.ChangeZone(554, "シールロック (争奪戦)");

        var json = OverlayEventJsonSerializer.Serialize(payload);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal("ChangeZone", root.GetProperty("type").GetString());
        Assert.Equal(554, root.GetProperty("zoneID").GetInt32());
        Assert.Equal("シールロック (争奪戦)", root.GetProperty("zoneName").GetString());
    }

    [Fact]
    public void ChangePrimaryPlayerEventUsesOverlayPluginCompatibleShape()
    {
        var payload = OverlayEventEnvelope.ChangePrimaryPlayer(0x10FF0001, "Player Name");

        var json = OverlayEventJsonSerializer.Serialize(payload);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal("ChangePrimaryPlayer", root.GetProperty("type").GetString());
        Assert.Equal(0x10FF0001, root.GetProperty("charID").GetInt32());
        Assert.Equal("Player Name", root.GetProperty("charName").GetString());
    }

    [Fact]
    public void LogLineEventKeepsRawLineAndSplitFields()
    {
        var payload = OverlayEventEnvelope.LogLine("00|2026-05-26T19:00:00.0000000+09:00|0039|フロントラインに不滅隊として参加しました！");

        var json = OverlayEventJsonSerializer.Serialize(payload);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal("LogLine", root.GetProperty("type").GetString());
        Assert.Equal("00|2026-05-26T19:00:00.0000000+09:00|0039|フロントラインに不滅隊として参加しました！", root.GetProperty("rawLine").GetString());
        Assert.Equal("00", root.GetProperty("line")[0].GetString());
        Assert.Equal("0039", root.GetProperty("line")[2].GetString());
    }

    [Fact]
    public void BattleSnapshotFormatsCurrentSituationStats()
    {
        var snapshot = new BattleSnapshot(kills: 7, deaths: 2, damageDealt: 1234567);

        Assert.Equal("7 / 2", snapshot.KillDeathText);
        Assert.Equal("1,234,567", snapshot.DamageDealtText);
    }
}
