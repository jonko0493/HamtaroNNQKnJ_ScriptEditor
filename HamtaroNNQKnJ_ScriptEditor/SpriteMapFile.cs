using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamtaroNNQKnJ_ScriptEditor
{
    public class SpriteMapFile : FileInDirectory
    {
        public short Index { get; set; }
        public short AssociatedPaletteIndex { get; set; }
        public PaletteFile AssociatedPalette { get; set; }
        public short UnknownShort1 { get; set; }
        public short UnknownShort2 { get; set; }
    }
}
