using Dalamud.Configuration;
using Dalamud.Plugin;

namespace FrontlineOverlay.Plugin;

[Serializable]
public sealed class Configuration : IPluginConfiguration
{
    [NonSerialized]
    private IDalamudPluginInterface? pluginInterface;

    public int Version { get; set; } = 1;

    public bool EnableBridge { get; set; } = true;

    public int BridgePort { get; set; } = 47774;

    public void Initialize(IDalamudPluginInterface pluginInterface)
    {
        this.pluginInterface = pluginInterface;
    }

    public void Save()
    {
        pluginInterface?.SavePluginConfig(this);
    }
}
