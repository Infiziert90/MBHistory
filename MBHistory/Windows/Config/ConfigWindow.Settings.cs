using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

namespace MBHistory.Windows.Config;

public partial class ConfigWindow
{
    private void Settings()
    {
        using var tabItem = ImRaii.TabItem("Settings");
        if (!tabItem.Success)
            return;

        var on = Plugin.Configuration.On;
        if (ImGui.Checkbox("On", ref on))
        {
            Plugin.Configuration.On = on;
            Plugin.Configuration.Save();
        }

        ImGuiHelpers.ScaledDummy(5.0f);
        ImGui.TextUnformatted("Options:");

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
