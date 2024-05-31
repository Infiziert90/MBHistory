using System;
using System.Numerics;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace MBHistory.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Plugin Plugin;

    private int CurrentNumber;

    public ConfigWindow(Plugin plugin) : base("Configuration##MBHistory")
    {
        Plugin = plugin;

        Size = new Vector2(205, 280);
        SizeCondition = ImGuiCond.Always;

        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;

        CurrentNumber = Plugin.Configuration.NumberToCheck;
    }

    public void Dispose() { }

    public override void Draw()
    {
        using var tabBar = ImRaii.TabBar("##settings-tabs");
        if (!tabBar.Success)
            return;

        using (var generalTab = ImRaii.TabItem("General###general-tab"))
        {
            if (generalTab)
            {
                ImGuiHelpers.ScaledDummy(5.0f);
                var on = Plugin.Configuration.On;
                if (ImGui.Checkbox("On", ref on))
                {
                    Plugin.Configuration.On = on;
                    Plugin.Configuration.Save();
                }

                ImGuiHelpers.ScaledDummy(5.0f);
                ImGui.Text("Options:");

                var onlySelf = Plugin.Configuration.OnlySelf;
                if (ImGui.Checkbox("Only track own purchases", ref onlySelf))
                {
                    Plugin.Configuration.OnlySelf = onlySelf;
                    Plugin.Configuration.Save();

                    Plugin.HistoryList.Update();
                }

                var chatlog = Plugin.Configuration.Chatlog;
                if (ImGui.Checkbox("Show copied notification", ref chatlog))
                {
                    Plugin.Configuration.Chatlog = chatlog;
                    Plugin.Configuration.Save();
                }

                ImGuiHelpers.ScaledDummy(65.0f);
                var verboseChatlog = Plugin.Configuration.VerboseChatlog;
                if (ImGui.Checkbox("Debug", ref verboseChatlog))
                {
                    Plugin.Configuration.VerboseChatlog = verboseChatlog;
                    Plugin.Configuration.Save();
                }
            }
        }

        using (var historyTab = ImRaii.TabItem("History###history-tab"))
        {
            if (historyTab)
            {
                ImGuiHelpers.ScaledDummy(5.0f);
                ImGui.Text("Number of transactions to copy:");

                ImGui.SliderInt("##s", ref CurrentNumber, 1, 20);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    CurrentNumber = Math.Clamp(CurrentNumber, 1, 20);
                    if (CurrentNumber != Plugin.Configuration.NumberToCheck)
                    {
                        Plugin.Configuration.NumberToCheck = CurrentNumber;
                        Plugin.Configuration.Save();

                        Plugin.HistoryList.Update();
                    }
                }

                ImGuiHelpers.ScaledDummy(5.0f);
                ImGui.Text("Include:");

                if (!Plugin.Configuration.HasOptions)
                    ImGui.TextColored(ImGuiColors.DPSRed, "Please select an option.");

                var check = false;
                var buyer = Plugin.Configuration.Buyer;
                if (ImGui.Checkbox("Buyer Name", ref buyer))
                    check = true;

                var price = Plugin.Configuration.Price;
                if (ImGui.Checkbox("Sale Price", ref price))
                    check = true;

                var amount = Plugin.Configuration.Amount;
                if (ImGui.Checkbox("Quantity", ref amount))
                    check = true;

                var date = Plugin.Configuration.Date;
                if (ImGui.Checkbox("Purchase Time", ref date))
                    check = true;

                if (check)
                {
                    Plugin.Configuration.Buyer = buyer;
                    Plugin.Configuration.Price = price;
                    Plugin.Configuration.Amount = amount;
                    Plugin.Configuration.Date = date;
                    Plugin.Configuration.Save();

                    Plugin.Configuration.GenerateOptions();
                    Plugin.HistoryList.Update();
                }
            }
        }
    }
}