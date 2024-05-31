using System;
using System.Numerics;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using MBHistory.Data;

namespace MBHistory.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly Plugin Plugin;

    public MainWindow(Plugin plugin) : base("Easy MBHistory Overview##MBHistory")
    {
        Plugin = plugin;

        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(380, 480),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public void Dispose() { }

    public override void Draw()
    {
        if (ImGui.Button("Show Settings"))
            Plugin.ConfigWindow.Toggle();

        ImGui.SameLine(ImGui.GetContentRegionMax().X - (118.0f * ImGuiHelpers.GlobalScale));

        if (ImGui.Button("Copy to clipboard"))
            ImGui.SetClipboardText(Plugin.HistoryList.GetClipboardString());

        ImGui.Spacing();

        ImGui.Text("Current selection:");
        if (!Plugin.Configuration.HasOptions)
            ImGui.TextColored(ImGuiColors.DPSRed, "Please select include options.");
        else
            RenderTable();
    }

    private void RenderTable()
    {
        using var table = ImRaii.Table("##history", Plugin.HistoryList.IncludeOptionCount());
        if (!table.Success)
            return;

        foreach (var name in Plugin.HistoryList.IncludeOptionNames())
            ImGui.TableSetupColumn($"{name}", ImGuiTableColumnFlags.None,  name == "Name" ? 0.45f : 0.25f);

        ImGui.TableHeadersRow();
        foreach (var item in Plugin.HistoryList.HistoryObjects)
        {
            if (!item.IsUsed)
                continue;

            foreach (var option in Plugin.HistoryList.GetOptionsIterator())
            {
                ImGui.TableNextColumn();
                switch (option)
                {
                    case IncludeOption.Name:
                        ImGui.Text($"{item.Name}");
                        break;
                    case IncludeOption.Price:
                        ImGui.Text($"{item.Price:N0}");
                        break;
                    case IncludeOption.Quantity:
                        ImGui.Text($"{item.Quantity}");
                        break;
                    case IncludeOption.Date:
                        ImGui.Text($"{item.Date.ToLocalTime():g}");
                        break;
                }
            }
        }
    }
}