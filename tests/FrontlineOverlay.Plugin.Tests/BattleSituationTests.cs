using FrontlineOverlay.Plugin.Core.State;

namespace FrontlineOverlay.Plugin.Tests;

public sealed class BattleSituationTests
{
    [Fact]
    public void BattleSnapshotFormatsCurrentSituationStats()
    {
        var snapshot = new BattleSnapshot(kills: 7, deaths: 2, damageDealt: 1234567);

        Assert.Equal("7 / 2", snapshot.KillDeathText);
        Assert.Equal("3.50", snapshot.KillDeathRatioText);
        Assert.Equal("1,234,567", snapshot.DamageDealtText);
    }

    [Fact]
    public void BattleSituationTrackerAccumulatesDalamudOnlyStats()
    {
        var tracker = new BattleSituationTracker();

        tracker.RecordDamage(12000);
        tracker.RecordKnockout();
        tracker.RecordDeath();
        tracker.RecordDamage(34567);

        var snapshot = tracker.GetSnapshot();

        Assert.Equal(1, snapshot.Kills);
        Assert.Equal(1, snapshot.Deaths);
        Assert.Equal(46567, snapshot.DamageDealt);
    }

    [Fact]
    public void BattleSituationTrackerIgnoresNegativeDamage()
    {
        var tracker = new BattleSituationTracker();

        tracker.RecordDamage(-1);

        Assert.Equal(0, tracker.GetSnapshot().DamageDealt);
    }

    [Fact]
    public void BattleSituationTrackerCanResetCurrentBattle()
    {
        var tracker = new BattleSituationTracker();
        tracker.RecordDamage(100);
        tracker.RecordKnockout();
        tracker.RecordDeath();

        tracker.Reset();

        Assert.Equal(new BattleSnapshot(kills: 0, deaths: 0, damageDealt: 0), tracker.GetSnapshot());
    }
}
