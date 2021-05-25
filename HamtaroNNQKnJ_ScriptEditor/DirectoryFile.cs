using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamtaroNNQKnJ_ScriptEditor
{
    public class DirectoryFile
    {
        public List<FileInDirectory> FilesInDirectory { get; set; }
    }

    public class FileInDirectory
    {
        public int Offset { get; set; }
        public int Size { get; set; }

        public override string ToString()
        {
            return $"Offset: 0x{Offset:X4}; Size: {Size} bytes";
        }
    }
}
