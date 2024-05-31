using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Game.Network.Structures;
using ImGuiNET;
using System;
using Dalamud.Game;
using Dalamud.Hooking;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using MBHistory.Attributes;
using MBHistory.Data;
using MBHistory.Windows;


namespace MBHistory
{
    public sealed class Plugin : IDalamudPlugin
    {
        // From: https://github.com/goatcorp/Dalamud/blob/master/Dalamud/Game/Network/Internal/NetworkHandlersAddressResolver.cs#L51
        private const string MarketBoardHistorySig = "40 53 48 83 EC 20 48 8B 0D ?? ?? ?? ?? 48 8B DA E8 ?? ?? ?? ?? 48 85 C0 74 36 4C 8B 00 48 8B C8 41 FF 90 ?? ?? ?? ?? 48 8B C8 BA ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 85 C0 74 17 48 8D 53 04";
        private delegate nint MbHistoryPacketHandler(nint a1, nint packetData, uint a3, char a4);
        private readonly Hook<MbHistoryPacketHandler> MbHistoryHook;

        [PluginService] public static DalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService] public static IChatGui Chat { get; private set; } = null!;
        [PluginService] public static IClientState ClientState { get; private set; } = null!;
        [PluginService] public static IPluginLog Log { get; private set; } = null!;
        [PluginService] public static ICommandManager Commands { get; private set; } = null!;
        [PluginService] public static ISigScanner Scanner { get; private set; } = null!;
        [PluginService] public static IGameInteropProvider Hook { get; private set; } = null!;

        public Configuration Configuration { get; init; }

        public readonly WindowSystem WindowSystem = new("MBHistory");
        public ConfigWindow ConfigWindow { get; init; }
        public MainWindow MainWindow { get; init; }

        private readonly PluginCommandManager<Plugin> CommandManager;
        public readonly HistoryList HistoryList;

        public Plugin()
        {
            Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Configuration.Initialize(PluginInterface);

            HistoryList = new HistoryList(Configuration);

            ConfigWindow = new ConfigWindow(this);
            MainWindow = new MainWindow(this);

            WindowSystem.AddWindow(ConfigWindow);
            WindowSystem.AddWindow(MainWindow);

            CommandManager = new PluginCommandManager<Plugin>(this, Commands);

            PluginInterface.UiBuilder.Draw += DrawUI;
            PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;

            var mbPointer = Scanner.ScanText(MarketBoardHistorySig);
            MbHistoryHook = Hook.HookFromAddress<MbHistoryPacketHandler>(mbPointer, MarketHistoryPacketDetour);
            MbHistoryHook.Enable();
        }

        public void Dispose()
        {
            MbHistoryHook?.Dispose();

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

        private nint MarketHistoryPacketDetour(nint a1, nint packetData, uint a3, char a4)
        {
            nint result;
            try
            {
                if (Configuration.On && ClientState.LocalPlayer != null)
                {
                    var playerName = ClientState.LocalPlayer.Name.ToString();
                    var listing = MarketBoardHistory.Read(packetData);

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
            finally
            {
                result = MbHistoryHook.Original(a1, packetData, a3, a4);
            }

            return result;
        }

        private void DrawUI()
        {
            WindowSystem.Draw();
        }

        private void DrawConfigUI()
        {
            ConfigWindow.Toggle();
        }
    }
}
