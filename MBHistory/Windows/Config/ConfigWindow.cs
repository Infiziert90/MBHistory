using System;
using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;

namespace MBHistory.Windows.Config;

public partial class ConfigWindow : Window, IDisposable
{
    private readonly Plugin Plugin;

    private int CurrentNumber;

    public ConfigWindow(Plugin plugin) : base("Configuration##MBHistory")
    {
        Plugin = plugin;

        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(300, 230),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        CurrentNumber = Plugin.Configuration.NumberToCheck;
    }

    public void Dispose() { }

    public override void Draw()
    {
        using var tabBar = ImRaii.TabBar("##ConfigTabBar");
        if (!tabBar.Success)
            return;

        Settings();

        History();

        About();
    }
}
