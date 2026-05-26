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
        ImGui.TextUnformatted("Frontline Overlay JP Plugin");
        ImGui.Separator();

        ImGui.TextUnformatted($"ブリッジ: {(plugin.IsBridgeRunning ? "起動中" : "停止中")}");
        ImGui.TextUnformatted($"URL: {plugin.BridgeEndpoint?.ToString() ?? "-"}");

        if (ImGui.Button("設定"))
        {
            plugin.ToggleConfigUi();
        }

        ImGui.SameLine();

        if (ImGui.Button("再起動"))
        {
            _ = plugin.RestartBridgeAsync();
        }
    }
}
