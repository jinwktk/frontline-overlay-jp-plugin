using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace FrontlineOverlay.Plugin.Windows;

public sealed class MainWindow : Window, IDisposable
{
    private readonly Plugin plugin;

    public MainWindow(Plugin plugin)
        : base("Frontline Overlay JP###FrontlineOverlayJPMain")
    {
        this.plugin = plugin;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(420, 180),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue),
        };
    }

    public void Dispose()
    {
    }

    public override void Draw()
    {
        var snapshot = plugin.CurrentBattleSnapshot;

        ImGui.TextUnformatted("戦況");
        ImGui.Separator();

        ImGui.TextUnformatted($"K/D: {snapshot.KillDeathText}");
        ImGui.TextUnformatted($"K/D比: {snapshot.KillDeathRatioText}");
        ImGui.TextUnformatted($"与ダメージ: {snapshot.DamageDealtText}");

        ImGui.Spacing();

        if (ImGui.Button("リセット"))
        {
            plugin.ResetCurrentBattle();
        }

        ImGui.SameLine();

        if (ImGui.Button("設定"))
        {
            plugin.ToggleConfigUi();
        }
    }
}
