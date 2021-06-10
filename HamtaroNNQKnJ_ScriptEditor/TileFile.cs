using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamtaroNNQKnJ_ScriptEditor
{
    public class TileFile : FileInDirectory
    {
        private SpriteMapFile _spriteMapFile;

        public SpriteMapFile SpriteMapFile { get { return _spriteMapFile; } set { _spriteMapFile = value; Palette = _spriteMapFile.AssociatedPalette; } }
        public PaletteFile Palette { get; set; }

        public byte[] PixelData { get; set; }

        public static TileFile ParseBGFromCompressedData(byte[] data)
        {
            var tileFile = new TileFile
            {
                Content = data,
            };
            tileFile.PixelData = GraphicsDriver.DecompressBgTiles(data);
            return tileFile;
        }

        public static TileFile ParseSpriteFromCompressedData(byte[] data)
        {
            var tileFile = new TileFile
            {
                Content = data,
            };
            tileFile.PixelData = GraphicsDriver.DecompressSpriteTiles(data);
            return tileFile;
        }

        public void WritePixelsToFile(string file)
        {
            File.WriteAllBytes(file, PixelData);
        }

        public Bitmap Get16ColorImage()
        {
            var bitmap = new Bitmap(256, 256);
            int pixelIndex = 0;
            for (int row = 0; row < 32 && pixelIndex < PixelData.Length; row++)
            {
                for (int col = 0; col < 32 && pixelIndex < PixelData.Length; col++)
                {
                    for (int ypix = 0; ypix < 8 && pixelIndex < PixelData.Length; ypix++)
                    {
                        for (int xpix = 0; xpix < 4 && pixelIndex < PixelData.Length; xpix++)
                        {
                            for (int xypix = 0; xypix < 2 && pixelIndex < PixelData.Length; xypix++)
                            {
                                bitmap.SetPixel((col << 3) + (xpix << 1) + xypix, (row << 3) + ypix,
                                    Palette.Palette[PixelData[pixelIndex] >> (xypix << 2) & 0xF]);
                            }
                            pixelIndex++;
                        }
                    }
                }
            }
            return bitmap;
        }

        public Bitmap Get256ColorImage()
        {
            var bitmap = new Bitmap(256, 256);
            int pixelIndex = 0;
            for (int row = 0; row < 32 && pixelIndex < PixelData.Length; row++)
            {
                for (int col = 0; col < 32 && pixelIndex < PixelData.Length; col++)
                {
                    for (int ypix = 0; ypix < 8 && pixelIndex < PixelData.Length; ypix++)
                    {
                        for (int xpix = 0; xpix < 8 && pixelIndex < PixelData.Length; xpix++)
                        {
                            bitmap.SetPixel((col << 3) + xpix, (row << 3) + ypix,
                                Palette.Palette[PixelData[pixelIndex++]]);
                        }
                    }
                }
            }
            return bitmap;
        }
    }
}