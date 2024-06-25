using DearImguiSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gbfr.utility.modtools.Windows;

public unsafe class LogWindow
{
    public static bool IsOpen = false;

    public static bool _autoScroll = true;

    public static List<LogMessage> _lines = new List<LogMessage>(2000);
    private static object _lock = new object();

    public static void Render()
    {
        if (!IsOpen)
            return;

        if (ImGui.Begin("Log Window", ref IsOpen, 0))
        {
            if (ImGui.SmallButton("Clear"))
                _lines.Clear();

            ImGui.SameLine(0, 2);
            if (ImGui.SmallButton("Copy"))
                ;

            ImGui.SameLine(0, 2);
            ImGui.Checkbox("Auto-scroll", ref _autoScroll);

            var vecInternal = new ImVec2.__Internal();
            var vector = new ImVec2(&vecInternal); // Heap allocation

            lock (_lock)
            {
                ImGui.BeginChildEx("##log", 1234, vector, true, (int)(ImGuiWindowFlags.AlwaysVerticalScrollbar | ImGuiWindowFlags.AlwaysHorizontalScrollbar));

                var greyColor4 = new ImVec4.__Internal();
                var greyColor = new ImVec4(&greyColor4); // Heap allocation
                greyColor.X = 0.4f;
                greyColor.Y = 0.4f;
                greyColor.Z = 0.4f;
                greyColor.W = 1.0f;

                var whiteColor4 = new ImVec4.__Internal();
                var whiteColor = new ImVec4(&whiteColor4); // Heap allocation
                whiteColor.X = 1.0f;
                whiteColor.Y = 1.0f;
                whiteColor.Z = 1.0f;
                whiteColor.W = 1.0f;

                for (int i = 0; i < _lines.Count; i++)
                {
                    ImGui.TextColored(greyColor, $"[{_lines[i].Time:HH:mm:ss.fff}]"); ImGui.SameLine(0, 4);
                    ImGui.TextColored(greyColor, $"[{_lines[i].Handler}]"); ImGui.SameLine(0, 4);
                    ImGui.TextColored(whiteColor, _lines[i].Message);
                }
            }

            if (_autoScroll && ImGui.GetScrollY() >= ImGui.GetScrollMaxY())
                ImGui.SetScrollHereY(1.0f);

            ImGui.EndChild();

            ImGui.End();
        }
    }

    public static void Log(string handler, string message)
    {
        lock (_lock)
        {
            if (_lines.Count >= 2000)
                _lines.Remove(_lines[0]);

            _lines.Add(new LogMessage(DateTime.UtcNow, handler, message));
        }
    }

    public record LogMessage(DateTime Time, string Handler, string Message);
}
