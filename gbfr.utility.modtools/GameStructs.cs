using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gbfr.utility.modtools;

public unsafe struct FileOpenResult
{
    public void* pFileStorage; // flatark::impl::Flatark::ChunkFileStorage
    public byte* pFileData;
    public ulong FileSize;
}

public unsafe struct StringWrap
{
    public char* pStr;
    public char* StringSize;
}
