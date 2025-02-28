using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using gbfr.utility.modtools.ImGuiSupport;

namespace gbfr.utility.modtools.ImGuiSupport.Windows;

public interface IImguiWindow : IImguiMenuComponent
{
    /// <summary>
    /// Whether to render regardless of menu enabled state.
    /// </summary>
    public bool IsOverlay { get; }

    public void Render(ImguiSupport imguiSupport);
}
