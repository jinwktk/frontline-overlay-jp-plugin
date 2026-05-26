using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace FrontlineOverlay.Plugin.Windows;

public sealed class ConfigWindow : Window, IDisposable
{
    private readonly Plugin plugin;

    public ConfigWindow(Plugin plugin)
        : base("Frontline Overlay JP 設定###FrontlineOverlayJPConfig")
    {
        this.plugin = plugin;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(360, 140),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue),
        };
    }

    public void Dispose()
    {
    }

    public override void Draw()
    {
        var enableBridge = plugin.Configuration.EnableBridge;
        if (ImGui.Checkbox("ローカルブリッジを有効にする", ref enableBridge))
        {
            plugin.Configuration.EnableBridge = enableBridge;
            plugin.Configuration.Save();
            _ = plugin.RestartBridgeAsync();
        }

        var port = plugin.Configuration.BridgePort;
        if (ImGui.InputInt("ブリッジポート", ref port))
        {
            plugin.Configuration.BridgePort = Math.Clamp(port, 1024, 65535);
            plugin.Configuration.Save();
        }

        if (ImGui.Button("ブリッジを再起動"))
        {
            _ = plugin.RestartBridgeAsync();
        }
    }
}
