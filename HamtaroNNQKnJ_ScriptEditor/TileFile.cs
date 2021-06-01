using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamtaroNNQKnJ_ScriptEditor
{
    public class TileFile
    {
        public byte[] CompressedData { get; set; }
        public byte[] PixelData { get; set; }

        public static TileFile ParseFromCompressedData(byte[] data)
        {
            var tileFile = new TileFile
            {
                CompressedData = data,
            };
            tileFile.PixelData = GraphicsDriver.DecompressTiles(data);
            return tileFile;
        }

        public void WritePixelsToFile(string file)
        {
            File.WriteAllBytes(file, PixelData);
        }
    }
}