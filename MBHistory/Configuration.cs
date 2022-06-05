using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using MBHistory.Data;

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
        
        public string Separator { get; set; } = "\t";
        
        public bool Buyer { get; set; } = true;
        public bool Price { get; set; } = true;
        public bool Amount { get; set; } = true;
        public bool Date { get; set; } = true;

        // the below exist just to make saving less cumbersome

        [NonSerialized]
        private DalamudPluginInterface? pluginInterface;

        [NonSerialized]
        public IncludeOption IncludeOptions = IncludeOption.None;
        
        [NonSerialized]
        public bool HasOptions;

        public void GenerateOptions()
        {
            IncludeOptions = IncludeOption.None;
            if (this.Buyer) IncludeOptions |= IncludeOption.Name;
            if (this.Price) IncludeOptions |= IncludeOption.Price;
            if (this.Amount) IncludeOptions |= IncludeOption.Quantity;
            if (this.Date) IncludeOptions |= IncludeOption.Date;
            UpdateHasOptions();
        }

        public void UpdateHasOptions()
        {
            this.HasOptions = IncludeOptions switch
            {
                IncludeOption.None => false,
                _ => true
            };
        }
        
        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
            this.GenerateOptions();
        }

        public void Save()
        {
            this.pluginInterface!.SavePluginConfig(this);
        }
    }
}
