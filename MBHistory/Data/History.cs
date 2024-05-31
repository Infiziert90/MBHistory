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
        Name = name;
        Price = price;
        Quantity = quantity;
        Date = date;

        this.config = config;
        GenerateString();
    }

    public void UpdateUsed(bool used)
    {
        IsUsed = used;
    }

    public void GenerateString()
    {
        if (config.Separator == "" || config.IncludeOptions == IncludeOption.None) return;

        // remove last separator
        var tmp = string.Join("", GenerateStringList().ToArray());
        if (tmp.EndsWith(config.Separator)) tmp = tmp.Remove(tmp.Length - 1);

        GeneratedString = tmp;
    }

    private IEnumerable<string> GenerateStringList()
    {
        if (!IsUsed)
            return [];

        return IncludeOptions.GetFlags(config.IncludeOptions).Select(option => option switch
        {
            IncludeOption.Name => $"{Name}{config.Separator}",
            IncludeOption.Price => $"{Price}{config.Separator}",
            IncludeOption.Quantity => $"{Quantity}{config.Separator}",
            IncludeOption.Date => $"{Date:yyyy-MM-dd h:mm}{config.Separator}",
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
        HistoryObjects = new List<History>();
        this.config = config;
    }

    public void ResetAndUpdate(string playerName)
    {
        PlayerName = playerName;
        HistoryObjects.Clear();
    }

    public void Append(MarketBoardHistory.MarketBoardHistoryListing h)
    {
        HistoryObjects.Add(new History(h.BuyerName, h.SalePrice, h.Quantity, h.PurchaseTime, config));
    }

    public void Update()
    {
        if (!Any())
            return;

        UpdateIsUsed();
        HistoryObjects.ForEach(t => t.GenerateString());
    }

    private void UpdateIsUsed()
    {
        foreach (var (item, i) in HistoryObjects.Select((value, i) => ( value, i )))
        {
            if (i >= config.NumberToCheck)
                item.UpdateUsed(false);
            else
                item.UpdateUsed(!config.OnlySelf || item.Name == PlayerName);
        }
    }

    public bool Any()
    {
        return HistoryObjects.Count > 0;
    }

    public string GetClipboardString()
    {
        return string.Join("\n", HistoryObjects.Select(item => item.GeneratedString).Where(item => item != "").ToArray());
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