using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamtaroNNQKnJ_ScriptEditor
{
    public class TileFile : FileInDirectory
    {
        public SpriteMapFile SpriteMapFile { get; set; }

        public byte[] CompressedData { get; set; }
        public byte[] PixelData { get; set; }

        public static TileFile ParseBGFromCompressedData(byte[] data)
        {
            var tileFile = new TileFile
            {
                CompressedData = data,
            };
            tileFile.PixelData = GraphicsDriver.DecompressBgTiles(data);
            return tileFile;
        }

        public static TileFile ParseSpriteFromCompressedData(byte[] data)
        {
            var tileFile = new TileFile
            {
                CompressedData = data,
            };
            tileFile.PixelData = GraphicsDriver.DecompressSpriteTiles(data);
            return tileFile;
        }

        public void WritePixelsToFile(string file)
        {
            File.WriteAllBytes(file, PixelData);
        }
    }
}