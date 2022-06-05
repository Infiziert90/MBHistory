using System;
using System.Collections.Generic;
using System.Linq;

namespace MBHistory.Data;

[Flags]
public enum IncludeOption : uint
{
    None = 0,
    Name = 1,
    Price = 2,
    Quantity = 4,
    Date = 8,
}

public static class IncludeOptions
{
    public static IEnumerable<Enum> GetFlags(Enum e)
    {
        var tmp = Enum.GetValues(e.GetType()).Cast<Enum>().Where(e.HasFlag).ToList();
        
        // remove None from the List
        tmp.RemoveAt(0);
        return tmp.AsEnumerable();
    }
    
    public static int Count(Enum e)
    {
        return GetFlags(e).Count();
    }
    
    public static IEnumerable<string> GetNameOfUsedFlags(Enum e)
    {
        return GetFlags(e).Select(x => Enum.GetName(typeof(IncludeOption), x) ?? "");
    }
}