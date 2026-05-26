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
        var openMainWindowOnStartup = plugin.Configuration.OpenMainWindowOnStartup;
        if (ImGui.Checkbox("起動時に戦況を開く", ref openMainWindowOnStartup))
        {
            plugin.Configuration.OpenMainWindowOnStartup = openMainWindowOnStartup;
            plugin.Configuration.Save();
        }
    }
}
