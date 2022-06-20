using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Game.Network.Structures;
using Dalamud.Data;
using Dalamud.Game.Gui;
using Dalamud.Game.Network;
using Dalamud.Game.ClientState;
using ImGuiNET;
using System;
using System.Linq;
using Dalamud.Logging;
using Num = System.Numerics;
using MBHistory.Attributes;
using MBHistory.Data;


namespace MBHistory
{
    public sealed class Plugin : IDalamudPlugin
    {
        [PluginService] public static DataManager Data { get; private set; } = null!;
        [PluginService] public static GameNetwork GameNetwork { get; private set; } = null!;
        [PluginService] public static ChatGui Chat { get; private set; } = null!;
        
        public string Name => "Easy MBHistory";

        private DalamudPluginInterface PluginInterface { get; init; }
        private Configuration Configuration { get; init; }
        private PluginUI PluginUi { get; init; }
        private ClientState clientState;
        
        private readonly PluginCommandManager<Plugin> commandManager;
        private readonly HistoryList HistoryList;
        
        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commands,
            [RequiredVersion("1.0")] ClientState clientState)
        {
            this.PluginInterface = pluginInterface;
            this.clientState = clientState;
            
            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);
            
            this.HistoryList = new HistoryList(this.Configuration);
            
            this.PluginUi = new PluginUI(this.Configuration, HistoryList);
            
            GameNetwork.NetworkMessage += OnNetworkEvent;
            
            this.commandManager = new PluginCommandManager<Plugin>(this, commands);

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
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
                    this.PluginUi.SettingsVisible = true;
                    break;
                default:
                    this.PluginUi.Visible = true;
                    break;
            }
        }
        
        public void Dispose()
        {
            GameNetwork.NetworkMessage -= OnNetworkEvent;
            this.PluginUi.Dispose();
            this.commandManager.Dispose();
        }
        
        private void OnNetworkEvent(IntPtr dataPtr, ushort opCode, uint sourceActorId, uint targetActorId, NetworkMessageDirection direction)
        {
            if (direction != NetworkMessageDirection.ZoneDown) return;
            if (!Data.IsDataReady) return;
            if (Configuration.VerboseChatlog) PluginLog.Debug($"History: Opcode {opCode}");
            if (opCode != Data.ServerOpCodes["MarketBoardHistory"]) return;
            if (!Configuration.On) return;
            if (clientState?.LocalPlayer == null)
            {
                TurnOff();
                Chat.PrintError("History: Unable to fetch character name.");
                return;
            }
            
            if (Configuration.VerboseChatlog) PluginLog.Debug("History: MarketBoardHistory Event fired.");

            var playerName = clientState.LocalPlayer.Name.ToString();
            var listing = MarketBoardHistory.Read(dataPtr);
            HistoryList.ResetAndUpdate(playerName);
            foreach (var item in listing.HistoryListings)
            {
                HistoryList.Append(item);
            }

            if (!HistoryList.HasItems()) return;
            HistoryList.Update();
            if (Configuration.Chatlog) Chat.Print("History: Copied clipboard.");
            ImGui.SetClipboardText(HistoryList.GetClipboardString());
        }

        private void TurnOff()
        {
            Configuration.On = false;
            Configuration.Save();
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
