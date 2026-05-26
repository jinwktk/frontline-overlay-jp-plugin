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

    public string KillDeathRatioText => Deaths == 0
        ? Kills.ToString("N2", CultureInfo.InvariantCulture)
        : ((double)Kills / Deaths).ToString("N2", CultureInfo.InvariantCulture);

    public string DamageDealtText => DamageDealt.ToString("N0", CultureInfo.InvariantCulture);
}
