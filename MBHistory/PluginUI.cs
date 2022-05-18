using ImGuiNET;
using System;
using System.Numerics;

namespace MBHistory
{
    // It is good to have this be disposable in general, in case you ever need it
    // to do any cleanup
    class PluginUI : IDisposable
    {
        private Configuration configuration;

        // this extra bool exists for ImGui, since you can't ref a property
        private bool visible = false;
        public bool Visible
        {
            get { return this.visible; }
            set { this.visible = value; }
        }

        private bool settingsVisible = false;
        public bool SettingsVisible
        {
            get { return this.settingsVisible; }
            set { this.settingsVisible = value; }
        }

        private int _currentNumber;
        public string CurrentText = "";

        // passing in the image here just for simplicityw
        public PluginUI(Configuration configuration)
        {
            this.configuration = configuration;
            this._currentNumber = configuration.NumberToCheck;
        }

        public void Dispose()
        {
            
        }

        public void Draw()
        {
            // This is our only draw handler attached to UIBuilder, so it needs to be
            // able to draw any windows we might have open.
            // Each method checks its own visibility/state to ensure it only draws when
            // it actually makes sense.
            // There are other ways to do this, but it is generally best to keep the number of
            // draw delegates as low as possible.

            DrawMainWindow();
            DrawSettingsWindow();
        }

        public void DrawMainWindow()
        {
            if (!Visible)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(375, 480), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(375, 480), new Vector2(float.MaxValue, float.MaxValue));
            if (ImGui.Begin("Easy MBHistory Overview", ref this.visible))
            {
                if (ImGui.Button("Show Settings"))
                {
                    SettingsVisible = true;
                }
                
                var spacing = ImGui.GetScrollY() == 0 ? 118.0f : 143.0f;
                ImGui.SameLine(ImGui.GetWindowWidth()-spacing);
                
                if (ImGui.Button("Copy to clipboard"))
                {
                    ImGui.SetClipboardText(CurrentText);
                }
                
                ImGui.Spacing();

                ImGui.Text("Current selection:");
                ImGui.InputTextMultiline(
                    "##copytext", 
                    ref CurrentText, 
                    99999, 
                    new Vector2(ImGui.GetWindowWidth()-20.0f, 20 * 18.2f), 
                    ImGuiInputTextFlags.ReadOnly
                    );
            }
            ImGui.End();
        }

        public void DrawSettingsWindow()
        {
            if (!SettingsVisible)
            {
                return;
            }
            
            ImGui.SetNextWindowSize(new Vector2(205, 280), ImGuiCond.Always);
            if (ImGui.Begin("Easy MBHistory Config", ref this.settingsVisible,
                ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {

                if (ImGui.BeginTabBar("##settings-tabs"))
                {
                    if (ImGui.BeginTabItem($"General###general-tab"))
                    {
                        ImGui.Dummy(new Vector2(0.0f, 5.0f));
                        var on = this.configuration.On;
                        if (ImGui.Checkbox("On", ref on))
                        {
                            this.configuration.On = on;
                            this.configuration.Save();
                        }
                
                        ImGui.Dummy(new Vector2(0.0f, 5.0f));
                        ImGui.Text("Options:");
                        
                        var onlySelf = this.configuration.OnlySelf;
                        if (ImGui.Checkbox("Only track own purchases", ref onlySelf))
                        {
                            this.configuration.OnlySelf = onlySelf;
                            this.configuration.Save();
                        }
                        
                        var chatlog = this.configuration.Chatlog;
                        if (ImGui.Checkbox("Show copied notification", ref chatlog))
                        {
                            this.configuration.Chatlog = chatlog;
                            this.configuration.Save();
                        }                        
                        
                        ImGui.Dummy(new Vector2(0.0f, 65.0f));
                        var verboseChatlog = this.configuration.VerboseChatlog;
                        if (ImGui.Checkbox("Debug", ref verboseChatlog))
                        {
                            this.configuration.VerboseChatlog = verboseChatlog;
                            this.configuration.Save();
                        }

                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem($"History###history-tab"))
                    {
                        ImGui.Dummy(new Vector2(0.0f, 5.0f));
                        ImGui.Text("Number of transactions to copy:");
                        
                        ImGui.SliderInt("##s", ref this._currentNumber, 1, 20);
                        if (ImGui.IsItemDeactivatedAfterEdit())
                        {
                            this._currentNumber = Math.Clamp(this._currentNumber, 1, 20); 
                            if (this._currentNumber != this.configuration.NumberToCheck)
                            {
                                this.configuration.NumberToCheck = _currentNumber;
                                this.configuration.Save();
                            }
                        }
                        
                        ImGui.Dummy(new Vector2(0.0f, 5.0f));
                        ImGui.Text("Include:");
                        ImGui.Dummy(new Vector2(0.0f, 5.0f));
                        
                        var buyer = this.configuration.Buyer;
                        if (ImGui.Checkbox("Buyer Name", ref buyer))
                        {
                            this.configuration.Buyer = buyer;
                            this.configuration.Save();
                        }
                
                        var price = this.configuration.Price;
                        if (ImGui.Checkbox("Sale Price", ref price))
                        {
                            this.configuration.Price = price;
                            this.configuration.Save();
                        }
                
                        var amount = this.configuration.Amount;
                        if (ImGui.Checkbox("Quantity", ref amount))
                        {
                            this.configuration.Amount = amount;
                            this.configuration.Save();
                        }                
                
                        var date = this.configuration.Date;
                        if (ImGui.Checkbox("Purchase Time", ref date))
                        {
                            this.configuration.Date = date;
                            this.configuration.Save();
                        }
                        
                        ImGui.EndTabItem();
                    }
                    ImGui.EndTabBar();
                }
            }
            ImGui.End();
        }
    }
}
