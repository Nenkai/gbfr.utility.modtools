using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace gbfr.utility.modtools;

public class Utils
{
    /// <summary>
    /// Returns a null reference to type T.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public unsafe static ref T NullReference<T>() => ref Unsafe.AsRef<T>((void*)0x0);
}
