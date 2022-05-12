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
using Num = System.Numerics;
using MBHistory.Attributes;


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
        
        
        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commands,
            [RequiredVersion("1.0")] ClientState clientState)
        {
            this.PluginInterface = pluginInterface;
            this.clientState = clientState;
            
            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);
            
            this.PluginUi = new PluginUI(this.Configuration);
            
            GameNetwork.NetworkMessage += OnNetworkEvent;
            
            this.commandManager = new PluginCommandManager<Plugin>(this, commands);

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
        }
        
        [Command("/history")]
        [HelpMessage("Toggles UI\nArguments:\non - Turns on\noff - Turns off\nconfig - Opens config")]
        public void PluginCommand(string command, string args)
        {
            if (args == "on")
            {
                Configuration.On = true;
                Configuration.Save();
            } 
            else if (args == "off")
            {
                Configuration.On = false;
                Configuration.Save();
            }            
            else if (args == "config")
            {
                this.PluginUi.SettingsVisible = true;
            }
            else
            {
                this.PluginUi.Visible = true;
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
            if (opCode != Data.ServerOpCodes["MarketBoardHistory"]) return;
            if (!Configuration.On) return;

            if (clientState?.LocalPlayer == null)
            {
                TurnOff();
                Chat.PrintError("History: Unable to fetch character name.");
                return;
            }
            
            if (Configuration.VerboseChatlog) Chat.Print(("History: MarketBoardHistory Event fired."));
            
            var clipboardString = "";
            PluginUi.CurrentText = clipboardString;
            
            var playerName = clientState.LocalPlayer.Name.ToString();
            var listing = MarketBoardHistory.Read(dataPtr);
            foreach (var (item, i) in listing.HistoryListings.Select((value, i) => ( value, i )))
            {
                if (i >= Configuration.NumberToCheck) break;
                if (Configuration.OnlySelf && item.BuyerName != playerName) continue;
                
                
                var tmp = 
                    $"{(Configuration.Buyer ? $"{item.BuyerName}\t" : "")}" +
                    $"{(Configuration.Price ? $"{item.SalePrice.ToString()}\t" : "")}" +
                    $"{(Configuration.Amount ? $"{item.Quantity.ToString()}\t" : "")}" +
                    $"{(Configuration.Date ? $"{item.PurchaseTime:yyyy-MM-dd h:mm}" : "")}";
                
                if (tmp == "")
                {
                    Chat.PrintError("History: No include option selected, pls check your settings.");
                    break;
                }

                if (tmp.EndsWith("\t")) tmp = tmp.Remove(tmp.Length - 1);
                clipboardString += tmp + "\n";
                
                if (Configuration.VerboseChatlog) Chat.Print($"History: {tmp.Replace("\t", " | ")}");
            }

            if (clipboardString == "") return;
            if (Configuration.Chatlog) Chat.Print("History: Copied clipboard.");
            PluginUi.CurrentText = clipboardString;
            ImGui.SetClipboardText(clipboardString);
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
