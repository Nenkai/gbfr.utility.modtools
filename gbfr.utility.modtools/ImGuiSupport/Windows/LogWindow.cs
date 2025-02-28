using Reloaded.Mod.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DearImguiSharp;

namespace gbfr.utility.modtools.ImGuiSupport.Windows;

public unsafe class LogWindow : IImguiWindow, IImguiMenuComponent
{
    public bool IsOverlay => false;

    public bool IsOpen = false;
    public bool _autoScroll = true;

    private ILogger _logger;

    private StreamWriter _sw = new StreamWriter("modtools_log.txt");

    public List<LogMessage> LastLines = new(2000);
    private static object _lock = new object();

    public LogWindow(ILogger logger)
    {
        _logger = logger;
        _logger.OnWriteLine += _logger_OnWriteLine;
    }

    private void _logger_OnWriteLine(object sender, (string text, System.Drawing.Color color) e)
    {
        lock (_lock)
        {
            if (LastLines.Count >= 2000)
                LastLines.Remove(LastLines[0]);

            var logMsg = new LogMessage(DateTime.UtcNow, sender.ToString(), e.text);
            LastLines.Add(logMsg);
            _sw.WriteLine(e.text);
        }
    }

    public void BeginMenuComponent()
    {
        if (ImGui.MenuItemEx("Logs", "", "", false, true))
        {
            IsOpen = true;
        }
    }


    public void Render(ImguiSupport imguiSupport)
    {
        if (!IsOpen)
            return;

        if (ImGui.Begin("Log Window", ref IsOpen, 0))
        {
            if (ImGui.SmallButton("Clear"))
                LastLines.Clear();

            ImGui.SameLine(0, 2);
            if (ImGui.SmallButton("Copy"))
                ;

            ImGui.SameLine(0, 2);
            ImGui.Checkbox("Auto-scroll", ref _autoScroll);

            var vecInternal = new ImVec2.__Internal();
            var vector = new ImVec2(&vecInternal); // Heap allocation


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

            lock (_lock)
            {
                for (int i = 0; i < LastLines.Count; i++)
                {
                    ImGui.TextColored(greyColor, $"[{LastLines[i].Time:HH:mm:ss.fff}]"); ImGui.SameLine(0, 4);
                    //ImGui.TextColored(greyColor, $"[{LastLines[i].Handler}]"); ImGui.SameLine(0, 4);
                    ImGui.TextColored(whiteColor, LastLines[i].Message);
                }
            }


            if (_autoScroll && ImGui.GetScrollY() >= ImGui.GetScrollMaxY())
                ImGui.SetScrollHereY(1.0f);

            ImGui.EndChild();

            ImGui.End();
        }
    }
}

public record LogMessage(DateTime Time, string Handler, string Message);
