using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using static gbfr.utility.modtools.Mod;

namespace gbfr.utility.modtools.Native;

public class NativeMethods
{
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern nint GetModuleHandle(string lpModuleName);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    public static extern nint LoadLibraryW(string lpFileName);

    [DllImport("kernel32.dll")]
    public static extern nint GetProcAddress(nint hModule, string procName);
}