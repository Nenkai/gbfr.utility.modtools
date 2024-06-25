using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DearImguiSharp;

using GBFRDataTools.Database.Entities;

namespace gbfr.utility.modtools.Windows;

public unsafe class CharacterManagerWindow
{
    public static bool IsOpen;

    public static List<DatabaseTable> Tables { get; set; } = new();
    public static DatabaseTable SelectedTable { get; set; }

    public static void Render()
    {
        if (!IsOpen)
            return;

        if (ImGui.Begin("CharacterManager", ref IsOpen, 0))
        {
            var vecInternal = new ImVec2.__Internal();
            var vector = new ImVec2(&vecInternal); // Heap allocation

            if (ImGui.BeginCombo("Table", SelectedTable.Name, (int)ImGuiComboFlags.None))
            {
                foreach (var table in Tables)
                {
                    bool isSelected = table == SelectedTable;
                    ImGui.SelectableBool(table.Name, isSelected, 0, vector);

                    if (isSelected)
                        ImGui.SetItemDefaultFocus();
                }

                ImGui.EndCombo();
            }

            ImGui.Spacing();

            if (ImGui.BeginChildEx("#tbl_child", 1234567, vector, true, (int)(ImGuiWindowFlags.AlwaysVerticalScrollbar | ImGuiWindowFlags.AlwaysHorizontalScrollbar)))
            {
                if (ImGui.BeginTable("#tbl", SelectedTable.Columns.Count, (int)(ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable | ImGuiTableFlags.SizingFixedFit), vector, 1.0f))
                {
                    foreach (var column in SelectedTable.Columns)
                        ImGui.TableSetupColumn(column.Name, (int)ImGuiTableColumnFlags.None, 0.0f, 0);
                    ImGui.TableHeadersRow();


                    StdListNode* currentEntry = SelectedTable.Rows->List.Node->Next; // First entry is always empty

                    for (int i = 0; i < SelectedTable.Rows->List.Size; i++)
                    {
                        ImGui.TableNextRow((int)ImGuiTableRowFlags.None, 0.0f);

                        var tableRow = new TableRow(); // TODO: don't create row objects on every frame.
                        Span<byte> rowBytes = new Span<byte>(currentEntry->Data, SelectedTable.RowSize);
                        tableRow.ReadRow(SelectedTable.Columns, rowBytes);

                        for (int j = 0; j < SelectedTable.Columns.Count; j++)
                        {
                            ImGui.TableNextColumn(); ImGui.Text(tableRow.Cells[j].ToString());
                        }

                        currentEntry = currentEntry->Next;
                    }

                    ImGui.EndTable();
                }

                ImGui.EndChild();
            }

            ImGui.End();
        }
    }
}
