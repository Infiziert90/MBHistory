using ImGuiNET;
using MBHistory.Data;

namespace MBHistory.Gui;

public class Table
{
    private HistoryList _historyList;

    public Table(HistoryList historyList)
    {
        this._historyList = historyList;
    }

    public void RenderTable()
    {
        if (!ImGui.BeginTable("##rolls", _historyList.IncludeOptionCount())) return;
        foreach (var name in _historyList.IncludeOptionNames())
            ImGui.TableSetupColumn($"{name}##theader", ImGuiTableColumnFlags.None,  name == "Name" ? 0.45f : 0.25f);
                    
        ImGui.TableHeadersRow();
        foreach (var item in _historyList.HistoryObjects)
        {
            if (!item.IsUsed) continue;
            foreach (var option in _historyList.GetOptionsIterator())
            {
                ImGui.TableNextColumn();
                switch (option)
                {
                    case IncludeOption.Name:
                        ImGui.Text($"{item.Name}");
                        break;                    
                    case IncludeOption.Price:
                        ImGui.Text($"{item.Price}");
                        break;                    
                    case IncludeOption.Quantity:
                        ImGui.Text($"{item.Quantity}");
                        break; 
                    case IncludeOption.Date:
                        ImGui.Text($"{item.Date:yyyy-MM-dd h:mm}");
                        break;
                }
            }
        }
        ImGui.EndTable();
    }
}