using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using DearImguiSharp;

using gbfr.utility.modtools.Hooks;
using gbfr.utility.modtools.ImGuiSupport.Windows;

namespace gbfr.utility.modtools.ImGuiSupport;

public unsafe class OverlayLogger : IImguiWindow
{
    // Inspired by xenomods
    public TimeSpan LINE_LIFETIME = TimeSpan.FromSeconds(8.0f);
    public TimeSpan TOAST_LIFETIME = TimeSpan.FromSeconds(2.0f);
    public TimeSpan FADEOUT_START = TimeSpan.FromSeconds(0.5f);
    public const int MAX_LINES = 20;

    private readonly List<LoggerMessage> lines = [];

    public bool IsOverlay => true;

    private bool _open = true;

    private static OverlayLogger _instance = new OverlayLogger();
    public static OverlayLogger Instance => _instance;
    
    public void AddMessage(string message)
    {
        if (lines.Count >= MAX_LINES)
            lines.Remove(lines[0]);

        var now = DateTimeOffset.UtcNow;
        lines.Add(new LoggerMessage()
        {
            Text = $"[{now}] {message}",
            Date = now,
            EndsAt = now + LINE_LIFETIME,
            // Logger::LINE_LIFETIME
        });
    }

    void DrawInternal(LoggerMessage msg, ushort x, ushort y)
    {
        float alpha = 1.0f;
		if (msg.Lifetime <= TimeSpan.Zero) 
        {
            alpha = 0.0f;
		} 
        else if (msg.Lifetime <= FADEOUT_START) 
        {
            // make the text fade out before it gets removed
            alpha = (float)(msg.Lifetime / FADEOUT_START);
		}

        var vecInternal = new ImVec2.__Internal();
        var vector = new ImVec2(&vecInternal); // Heap allocation
        vector.X = x;
        vector.Y = y;

        var colInternal = new ImVec4.__Internal();
        var col = new ImVec4(&colInternal); // Heap allocation
        col.X = 1.0f;
        col.Y = 1.0f;
        col.Z = 1.0f;
        col.W = alpha;

        ImGui.SetCursorScreenPos(vector);
        ImGui.TextColored(col, msg.Text);
	}

    public void Render(ImguiSupport imguiSupport)
    {
        float barHeight = 0;
        if (imguiSupport.IsMenuOpen)
            barHeight += ImGui.GetFrameHeight();

        var sizeVec2Internal = new ImVec2.__Internal();
        var sizeVec = new ImVec2(&sizeVec2Internal); // Heap allocation
        sizeVec.X = ImGui.GetIO().DisplaySize.X;
        sizeVec.Y = ImGui.GetIO().DisplaySize.Y - barHeight;
        ImGui.SetNextWindowSize(sizeVec, 1);

        if (ImGui.Begin("log_overlay", ref _open, (int)(ImGuiWindowFlags.NoDecoration |
            ImGuiWindowFlags.NoDocking |
            ImGuiWindowFlags.AlwaysAutoResize |
            ImGuiWindowFlags.NoSavedSettings |
            ImGuiWindowFlags.NoFocusOnAppearing |
            ImGuiWindowFlags.NoNav |
            ImGuiWindowFlags.NoInputs |
            ImGuiWindowFlags.NoBackground)))
        {
            var vecInternal = new ImVec2.__Internal();
            var vector = new ImVec2(&vecInternal); // Heap allocation
            vector.X = 0;
            vector.Y = barHeight;
            ImGui.SetWindowPosVec2(vector, 1);

            for (int i = 0; i < lines.Count; ++i)
            {
                var msg = lines[i];

                // check lifetime greater than 0, but also decrement it for next time
                if (msg.Lifetime > TimeSpan.Zero)
                    DrawInternal(msg, 10, (ushort)(barHeight + 5 + (i * 16)));
                else if (lines.Count != 0)
                    // erase the current index but decrement i so we try again with the next one
                    lines.Remove(lines[i--]);

            }

            ImGui.End();
        }
    }

    public void BeginMenuComponent()
    {
        
    }
}

public class LoggerMessage
{
    public string Text;
    public string group;
    public DateTimeOffset Date;
    public DateTimeOffset EndsAt;
    public TimeSpan Lifetime => EndsAt - DateTimeOffset.UtcNow;
};
