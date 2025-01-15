using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Game.Network.Structures;
using ImGuiNET;
using System;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using MBHistory.Attributes;
using MBHistory.Data;
using MBHistory.Windows;
using MBHistory.Windows.Config;


namespace MBHistory
{
    public sealed class Plugin : IDalamudPlugin
    {
        [PluginService] public static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService] public static IChatGui Chat { get; private set; } = null!;
        [PluginService] public static IClientState ClientState { get; private set; } = null!;
        [PluginService] public static IPluginLog Log { get; private set; } = null!;
        [PluginService] public static ICommandManager Commands { get; private set; } = null!;
        [PluginService] public static IMarketBoard MarketBoard { get; private set; } = null!;

        public Configuration Configuration { get; init; }

        public readonly WindowSystem WindowSystem = new("MBHistory");
        public ConfigWindow ConfigWindow { get; init; }
        public MainWindow MainWindow { get; init; }

        private readonly PluginCommandManager<Plugin> CommandManager;
        public readonly HistoryList HistoryList;

        public Plugin()
        {
            Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Configuration.Initialize();

            HistoryList = new HistoryList(Configuration);

            ConfigWindow = new ConfigWindow(this);
            MainWindow = new MainWindow(this);

            WindowSystem.AddWindow(ConfigWindow);
            WindowSystem.AddWindow(MainWindow);

            CommandManager = new PluginCommandManager<Plugin>(this, Commands);

            PluginInterface.UiBuilder.Draw += DrawUi;
            PluginInterface.UiBuilder.OpenMainUi += DrawMainUi;
            PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUi;

            MarketBoard.HistoryReceived += MarketHistory;
        }

        public void Dispose()
        {
            WindowSystem.RemoveAllWindows();
            ConfigWindow.Dispose();
            MainWindow.Dispose();

            CommandManager.Dispose();
        }

        [Command("/history")]
        [HelpMessage("Toggles UI\nArguments:\non - Turns on\noff - Turns off\nconfig - Opens config")]
        public void PluginCommand(string command, string args)
        {
            switch (args)
            {
                case "on":
                    Configuration.On = true;
                    Configuration.Save();
                    break;
                case "off":
                    Configuration.On = false;
                    Configuration.Save();
                    break;
                case "config":
                    ConfigWindow.Toggle();
                    break;
                default:
                    MainWindow.Toggle();
                    break;
            }
        }

        private void MarketHistory(IMarketBoardHistory listing)
        {
            try
            {
                if (Configuration.On && ClientState.LocalPlayer != null)
                {
                    var playerName = ClientState.LocalPlayer.Name.ToString();

                    HistoryList.ResetAndUpdate(playerName);
                    foreach (var item in listing.HistoryListings)
                        HistoryList.Append(item);

                    if (HistoryList.Any())
                    {
                        HistoryList.Update();
                        if (Configuration.Chatlog)
                            Chat.Print("[MBHistory] Copied to clipboard.");

                        ImGui.SetClipboardText(HistoryList.GetClipboardString());
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unable to read marketboard history.");
            }
        }

        private void DrawUi()
        {
            WindowSystem.Draw();
        }

        private void DrawMainUi()
        {
            MainWindow.Toggle();
        }

        private void DrawConfigUi()
        {
            ConfigWindow.Toggle();
        }
    }
}
