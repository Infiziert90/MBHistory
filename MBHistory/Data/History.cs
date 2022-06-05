using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.Network.Structures;

namespace MBHistory.Data;

public class History
{
    private Configuration config;
    
    public readonly string Name;
    public readonly uint Price;
    public readonly uint Quantity;
    public readonly DateTime Date;    
    
    public string GeneratedString = "";

    public bool IsUsed = true;
    
    public History(string name, uint price, uint quantity, DateTime date, Configuration config)
    {
        this.Name = name;
        this.Price = price;
        this.Quantity = quantity;
        this.Date = date;
        
        this.config = config;
        this.GenerateString();
    }

    public void UpdateUsed(bool used)
    {
        IsUsed = used;
    }
    
    public void GenerateString()
    {
        if (config.Separator == "" || config.IncludeOptions == IncludeOption.None) return;
        
        // remove last separator
        var tmp = string.Join("", this.GenerateStringList().ToArray());
        if (tmp.EndsWith(config.Separator)) tmp = tmp.Remove(tmp.Length - 1);
        
        GeneratedString = tmp;
    }
    
    private IEnumerable<string> GenerateStringList()
    {
        if (!this.IsUsed) return Enumerable.Empty<string>();
        
        return IncludeOptions.GetFlags(config.IncludeOptions).Select(option => option switch
        {
            IncludeOption.Name => $"{this.Name}{config.Separator}",
            IncludeOption.Price => $"{this.Price}{config.Separator}",
            IncludeOption.Quantity => $"{this.Quantity}{config.Separator}",
            IncludeOption.Date => $"{this.Date:yyyy-MM-dd h:mm}{config.Separator}",
            _ => ""
        });
    }
}

public class HistoryList
{
    private Configuration config;
    public List<History> HistoryObjects;
    private string PlayerName = "";

    public HistoryList(Configuration config)
    {
        this.HistoryObjects = new List<History>();
        this.config = config;
    }

    public void ResetAndUpdate(string playerName)
    {
        this.PlayerName = playerName;
        HistoryObjects.Clear();
    }
    
    public void Append(MarketBoardHistory.MarketBoardHistoryListing h)
    {
        this.HistoryObjects.Add(new History(h.BuyerName, h.SalePrice, h.Quantity, h.PurchaseTime, config));
    }

    public void Update()
    {
        if (!this.HasItems()) return;
        this.UpdateIsUsed();
        this.HistoryObjects.ForEach(t => t.GenerateString());
    }

    private void UpdateIsUsed()
    {
        foreach (var (item, i) in HistoryObjects.Select((value, i) => ( value, i )))
        {
            if (i >= config.NumberToCheck)
                item.UpdateUsed(false);
            else
                item.UpdateUsed(!config.OnlySelf || item.Name == this.PlayerName); 
        }
    }
    
    public bool HasItems()
    {
        return this.HistoryObjects.Count > 0;
    }

    public string GetClipboardString()
    {
        return string.Join("\n", this.HistoryObjects.Select(item => item.GeneratedString).Where(item => item != "").ToArray());
    }
    
    public IEnumerable<Enum> GetOptionsIterator()
    {
        return IncludeOptions.GetFlags(config.IncludeOptions);
    }    
    
    public int IncludeOptionCount()
    {
        return IncludeOptions.Count(config.IncludeOptions);
    }    
    
    public IEnumerable<string> IncludeOptionNames()
    {
        return IncludeOptions.GetNameOfUsedFlags(config.IncludeOptions);
    }
}