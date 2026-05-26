using System.Globalization;

namespace FrontlineOverlay.Plugin.Core.State;

public sealed record BattleSnapshot
{
    public BattleSnapshot(int kills, int deaths, long damageDealt)
    {
        Kills = kills;
        Deaths = deaths;
        DamageDealt = damageDealt;
    }

    public int Kills { get; }

    public int Deaths { get; }

    public long DamageDealt { get; }

    public string KillDeathText => $"{Kills} / {Deaths}";

    public string DamageDealtText => DamageDealt.ToString("N0", CultureInfo.InvariantCulture);
}
