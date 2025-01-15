using System;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

namespace MBHistory.Windows.Config;

public partial class ConfigWindow
{
    private void History()
    {
        using var tabItem = ImRaii.TabItem("History");
        if (!tabItem.Success)
            return;

        ImGui.TextUnformatted("Number of transactions to copy:");

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
        ImGui.TextUnformatted("Include:");

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
