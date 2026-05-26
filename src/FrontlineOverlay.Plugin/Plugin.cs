using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FrontlineOverlay.Plugin.Bridge;
using FrontlineOverlay.Plugin.Core.Events;
using FrontlineOverlay.Plugin.Windows;

namespace FrontlineOverlay.Plugin;

public sealed class Plugin : IDalamudPlugin
{
    private const string CommandName = "/flopjp";

    private readonly IDalamudPluginInterface pluginInterface;
    private readonly ICommandManager commandManager;
    private readonly IPluginLog log;
    private readonly WindowSystem windowSystem = new("FrontlineOverlayJPPlugin");
    private readonly MainWindow mainWindow;
    private readonly ConfigWindow configWindow;

    private OverlayBridgeServer? bridgeServer;

    public Plugin(IDalamudPluginInterface pluginInterface, ICommandManager commandManager, IPluginLog log)
    {
        this.pluginInterface = pluginInterface;
        this.commandManager = commandManager;
        this.log = log;

        Configuration = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration.Initialize(pluginInterface);

        mainWindow = new MainWindow(this);
        configWindow = new ConfigWindow(this);
        windowSystem.AddWindow(mainWindow);
        windowSystem.AddWindow(configWindow);

        commandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Frontline Overlay JP Plugin のメイン画面を開きます。",
        });

        pluginInterface.UiBuilder.Draw += DrawUi;
        pluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;

        _ = RestartBridgeAsync();
    }

    public Configuration Configuration { get; }

    public Uri? BridgeEndpoint => bridgeServer?.Endpoint;

    public bool IsBridgeRunning => bridgeServer?.IsRunning == true;

    public async Task RestartBridgeAsync()
    {
        if (bridgeServer is not null)
        {
            await bridgeServer.DisposeAsync().ConfigureAwait(false);
            bridgeServer = null;
        }

        if (!Configuration.EnableBridge)
        {
            return;
        }

        bridgeServer = new OverlayBridgeServer(Configuration.BridgePort);

        try
        {
            await bridgeServer.StartAsync().ConfigureAwait(false);
            await bridgeServer.BroadcastAsync(OverlayEventEnvelope.LogLine("00|Plugin|0000|Frontline Overlay JP Plugin bridge started")).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            log.Error(ex, "Failed to start Frontline Overlay JP bridge.");
        }
    }

    public void Dispose()
    {
        pluginInterface.UiBuilder.Draw -= DrawUi;
        pluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        commandManager.RemoveHandler(CommandName);
        windowSystem.RemoveAllWindows();

        if (bridgeServer is not null)
        {
            bridgeServer.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
    }

    private void OnCommand(string command, string args)
    {
        ToggleMainUi();
    }

    private void DrawUi()
    {
        windowSystem.Draw();
    }

    internal void ToggleMainUi()
    {
        mainWindow.Toggle();
    }

    internal void ToggleConfigUi()
    {
        configWindow.Toggle();
    }
}
