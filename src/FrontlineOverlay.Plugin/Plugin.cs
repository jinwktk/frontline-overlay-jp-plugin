using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FrontlineOverlay.Plugin.Core.State;
using FrontlineOverlay.Plugin.Windows;

namespace FrontlineOverlay.Plugin;

public sealed class Plugin : IDalamudPlugin
{
    private const string CommandName = "/flopjp";

    private readonly IDalamudPluginInterface pluginInterface;
    private readonly ICommandManager commandManager;
    private readonly WindowSystem windowSystem = new("FrontlineOverlayJPPlugin");
    private readonly BattleSituationTracker battleSituationTracker = new();
    private readonly MainWindow mainWindow;
    private readonly ConfigWindow configWindow;

    public Plugin(IDalamudPluginInterface pluginInterface, ICommandManager commandManager)
    {
        this.pluginInterface = pluginInterface;
        this.commandManager = commandManager;

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

        mainWindow.IsOpen = Configuration.OpenMainWindowOnStartup;
    }

    public Configuration Configuration { get; }

    public BattleSnapshot CurrentBattleSnapshot => battleSituationTracker.GetSnapshot();

    public void Dispose()
    {
        pluginInterface.UiBuilder.Draw -= DrawUi;
        pluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        commandManager.RemoveHandler(CommandName);
        windowSystem.RemoveAllWindows();
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

    internal void ResetCurrentBattle()
    {
        battleSituationTracker.Reset();
    }
}
