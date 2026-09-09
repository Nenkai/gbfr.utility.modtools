using Reloaded.Mod.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NenTools.ImGui.Interfaces.Shell;
using NenTools.ImGui.Interfaces;
using System.Numerics;

namespace gbfr.utility.modtools.ImGuiSupport.Windows;

public unsafe class LogWindow : IImGuiComponent
{
    private readonly IImGui _imGui;
    private readonly ILogger _logger;

    public bool IsOverlay => false;

    public bool IsOpen = false;
    public bool _autoScroll = true;

    private readonly StreamWriter _sw = new("modtools_log.txt");

    public List<LogMessage> LastLines = new(2000);
    private static Lock _lock = new();

    public LogWindow(IImGui imGui, ILogger logger)
    {
        _imGui = imGui;

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

    public void RenderMenu(IImGuiShell imGuiShell)
    {
        if (_imGui.MenuItem("Logs"u8))
        {
            IsOpen = true;
        }
    }


    public void Render(IImGuiShell imGuiShell)
    {
        if (!IsOpen)
            return;

        if (_imGui.Begin("Log Window"u8, ref IsOpen))
        {
            if (_imGui.SmallButton("Clear"u8))
                LastLines.Clear();

            _imGui.SameLineEx(0, 2);
            if (_imGui.SmallButton("Copy"u8))
                _imGui.SetClipboardText(string.Join("\n", LastLines.Select(e => e.Message)));

            _imGui.SameLineEx(0, 2);
            _imGui.Checkbox("Auto-scroll", ref _autoScroll);

            _imGui.Checkbox("Enable File Logging", ref ImGuiConfig.LogFiles);

            _imGui.BeginChild("##log", Vector2.Zero, window_flags: ImGuiWindowFlags.ImGuiWindowFlags_AlwaysVerticalScrollbar | ImGuiWindowFlags.ImGuiWindowFlags_AlwaysHorizontalScrollbar);

            lock (_lock)
            {
                for (int i = 0; i < LastLines.Count; i++)
                {
                    _imGui.TextColored(new System.Numerics.Vector4(0.4f, 0.4f, 0.4f, 1.0f), $"[{LastLines[i].Time:HH:mm:ss.fff}]"); _imGui.SameLineEx(0, 4);
                    //ImGui.TextColored(greyColor, $"[{LastLines[i].Handler}]"); ImGui.SameLine(0, 4);
                    _imGui.TextColored(new System.Numerics.Vector4(1.0f), LastLines[i].Message);
                }
            }


            if (_autoScroll && _imGui.GetScrollY() >= _imGui.GetScrollMaxY())
                _imGui.SetScrollHereY(1.0f);

            _imGui.EndChild();
        }
        _imGui.End();
    }
}

public record LogMessage(DateTime Time, string Handler, string Message);
