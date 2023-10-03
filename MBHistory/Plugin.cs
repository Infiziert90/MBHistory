using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Game.Network.Structures;
using Dalamud.Game.Network;
using ImGuiNET;
using System;
using Dalamud.Plugin.Services;
using Num = System.Numerics;
using MBHistory.Attributes;
using MBHistory.Data;


namespace MBHistory
{
    public sealed class Plugin : IDalamudPlugin
    {
        [PluginService] public static DalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService] public static IDataManager Data { get; private set; } = null!;
        [PluginService] public static IGameNetwork GameNetwork { get; private set; } = null!;
        [PluginService] public static IChatGui Chat { get; private set; } = null!;
        [PluginService] public static IClientState ClientState { get; private set; } = null!;
        [PluginService] public static IPluginLog Log { get; private set; } = null!;
        [PluginService] public static ICommandManager Commands { get; private set; } = null!;

        private Configuration Configuration { get; init; }
        private PluginUI PluginUi { get; init; }

        private readonly PluginCommandManager<Plugin> CommandManager;
        private readonly HistoryList HistoryList;

        public Plugin()
        {
            Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Configuration.Initialize(PluginInterface);

            HistoryList = new HistoryList(Configuration);

            PluginUi = new PluginUI(Configuration, HistoryList);

            GameNetwork.NetworkMessage += OnNetworkEvent;

            CommandManager = new PluginCommandManager<Plugin>(this, Commands);

            PluginInterface.UiBuilder.Draw += DrawUI;
            PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
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
                    PluginUi.SettingsVisible = true;
                    break;
                default:
                    PluginUi.Visible = true;
                    break;
            }
        }

        public void Dispose()
        {
            GameNetwork.NetworkMessage -= OnNetworkEvent;
            PluginUi.Dispose();
            CommandManager.Dispose();
        }

        private const ushort Opcode = 0x39f;
        private void OnNetworkEvent(IntPtr dataPtr, ushort opCode, uint sourceActorId, uint targetActorId, NetworkMessageDirection direction)
        {
            if (!Configuration.On)
                return;

            if (opCode != Opcode)
                return;

            if (ClientState.LocalPlayer == null)
                return;

            var playerName = ClientState.LocalPlayer.Name.ToString();
            var listing = MarketBoardHistory.Read(dataPtr);
            HistoryList.ResetAndUpdate(playerName);
            foreach (var item in listing.HistoryListings)
                HistoryList.Append(item);

            if (!HistoryList.Any())
                return;

            HistoryList.Update();
            if (Configuration.Chatlog)
                Chat.Print("History: Copied clipboard.");

            ImGui.SetClipboardText(HistoryList.GetClipboardString());
        }

        private void DrawUI()
        {
            this.PluginUi.Draw();
        }

        private void DrawConfigUI()
        {
            this.PluginUi.SettingsVisible = true;
        }
    }
}
