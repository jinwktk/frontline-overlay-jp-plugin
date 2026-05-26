namespace FrontlineOverlay.Plugin.Core.State;

public sealed class BattleSituationTracker
{
    private int kills;
    private int deaths;
    private long damageDealt;

    public void RecordKnockout()
    {
        kills++;
    }

    public void RecordDeath()
    {
        deaths++;
    }

    public void RecordDamage(long amount)
    {
        if (amount <= 0)
        {
            return;
        }

        damageDealt += amount;
    }

    public BattleSnapshot GetSnapshot()
    {
        return new BattleSnapshot(kills, deaths, damageDealt);
    }

    public void Reset()
    {
        kills = 0;
        deaths = 0;
        damageDealt = 0;
    }
}
