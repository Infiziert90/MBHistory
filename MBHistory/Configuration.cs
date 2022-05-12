using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace MBHistory
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;
        public bool On { get; set; } = false;
        public bool OnlySelf { get; set; } = true;
        public bool Chatlog { get; set; } = true;
        public bool VerboseChatlog { get; set; } = false;
        public int NumberToCheck { get; set; } = 20;
        public bool Buyer { get; set; } = true;
        public bool Price { get; set; } = true;
        public bool Amount { get; set; } = true;
        public bool Date { get; set; } = true;

        // the below exist just to make saving less cumbersome

        [NonSerialized]
        private DalamudPluginInterface? pluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
        }

        public void Save()
        {
            this.pluginInterface!.SavePluginConfig(this);
        }
    }
}
