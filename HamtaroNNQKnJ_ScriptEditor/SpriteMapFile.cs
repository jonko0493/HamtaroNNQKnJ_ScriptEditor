using System;
using System.Collections.Generic;
using System.Drawing;
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
        public TileFile AssociatedTiles { get; set; }
        public short UnknownShort1 { get; set; }
        public short UnknownShort2 { get; set; }

        public byte[] DecompressedData { get; set; }
        public int NumSequences { get; set; }
        public int NumFrames { get; set; }
        public int NumClips { get; set; }
        public int SequenceAddress { get; set; }
        public int FrameAddress { get; set; }
        public int ClipAddress { get; set; }
        public int LyrAddress { get; set; }

        // This routine is borrowed from Yoshi Magic
        public void Initialize()
        {
            DecompressedData = GraphicsDriver.DecompressSpriteTiles(Content);

            NumSequences = BitConverter.ToInt16(new byte[] { DecompressedData[0x0C], DecompressedData[0x0D] });
            NumFrames = BitConverter.ToInt16(new byte[] { DecompressedData[0x0E], DecompressedData[0x0F] });
            NumClips = BitConverter.ToInt16(new byte[] { DecompressedData[0x10], DecompressedData[0x11] });

            SequenceAddress = 0x18;
            FrameAddress = SequenceAddress + (NumSequences * 8);
            ClipAddress = FrameAddress + (NumFrames * 4);
            LyrAddress = ClipAddress + (NumClips * 4);
        }


        // This routine is borrowed from Yoshi Magic
        public Bitmap GetAnimationPreview(int clip)
        {
            Bitmap bitmap = new Bitmap(576, 320);

            int clipAddress = ClipAddress + clip * 4;
            int startLyr = BitConverter.ToInt16(DecompressedData.Skip(clipAddress).Take(2).ToArray());
            int lastLyr = BitConverter.ToInt16(DecompressedData.Skip(clipAddress + 2).Take(2).ToArray());
            int curlAddress = LyrAddress + (lastLyr * 0x0C);

            // don't know what this variable name stands for
            byte[] shpsz = new byte[] { 8, 8, 16, 16, 32, 32, 64, 64, 16, 8, 32, 8, 32, 16, 64, 32, 8, 16, 8, 32, 16, 32, 32, 64 };

            for (int i = 0; i < lastLyr - startLyr; i++)
            {
                curlAddress -= 0x0C;

                int lyrY = DecompressedData[curlAddress] ^ 0x80;
                int lyrX = (BitConverter.ToInt16(DecompressedData.Skip(curlAddress + 2).Take(2).ToArray()) & 0x01FF) ^ 0x0100;
                int num = BitConverter.ToInt16(DecompressedData.Skip(curlAddress + 4).Take(2).ToArray()) << 5;
                byte shpszIndex = (byte)(((DecompressedData[curlAddress + 1] >> 3) & 0x18) | ((DecompressedData[curlAddress + 3] >> 5) & 0x06));

                byte tXFlip1;
                byte tXFlip2;
                short xStep;
                byte tYFlip1;
                byte tYFlip2;
                short yStep;

                switch ((DecompressedData[curlAddress + 1] >> 5) & 0x01)
                {
                    case 0: // 16 colors
                        if (((DecompressedData[curlAddress + 3] >> 4) & 1) == 1)
                        {
                            tXFlip1 = (byte)((shpsz[shpszIndex] >> 3) - 1);
                            tXFlip2 = 0;
                            xStep = -1;
                        }
                        else
                        {
                            tXFlip1 = 0;
                            tXFlip2 = (byte)((shpsz[shpszIndex] >> 3) - 1);
                            xStep = 1;
                        }

                        if (((DecompressedData[curlAddress + 3] >> 5) & 1) == 1)
                        {
                            tYFlip1 = (byte)((shpsz[shpszIndex + 1] >> 3) - 1);
                            tYFlip2 = 0;
                            yStep = -1;
                        }
                        else
                        {
                            tYFlip1 = 0;
                            tYFlip2 = (byte)((shpsz[shpszIndex + 1] >> 3) - 1);
                            yStep = 1;
                        }

                        for (int tileY = tYFlip1; (yStep == -1 && tileY >= tYFlip2) || (yStep == 1 && tileY <= tYFlip2); tileY += yStep)
                        {
                            for (int tileX = tXFlip1; (xStep == -1 && tileX >= tXFlip2) || (xStep == 1 && tileX <= tXFlip2); tileX += xStep)
                            {
                                for (int ypix = 0; ypix < 8; ypix++)
                                {
                                    for (int xpix = 0; xpix < 4; xpix++)
                                    {
                                        for (int xypix = 0; xypix < 2; xypix++)
                                        {
                                            int pix = AssociatedTiles.PixelData[num] >> (xypix * 4) & 0x0F;
                                            if (pix != 0)
                                            {
                                                bitmap.SetPixel(lyrX + (tileX * 8) + Math.Abs((-7 * ((DecompressedData[curlAddress + 3] >> 4) & 1)) + (xpix * 2) + xypix),
                                                    lyrY + (tileY * 8) + Math.Abs((-7 * ((DecompressedData[curlAddress + 3] >> 5) & 1)) + ypix),
                                                    AssociatedPalette.Palette[pix]);
                                            }
                                        }
                                        num++;
                                    }
                                }
                            }
                        }
                        break;

                    case 1: // 256 colors
                        num *= 2;

                        if (((DecompressedData[curlAddress + 3] >> 4) & 1) == 1)
                        {
                            tXFlip1 = (byte)((shpsz[shpszIndex] >> 3) - 1);
                            tXFlip2 = 0;
                            xStep = -1;
                        }
                        else
                        {
                            tXFlip1 = 0;
                            tXFlip2 = (byte)((shpsz[shpszIndex] >> 3) - 1);
                            xStep = 1;
                        }

                        if (((DecompressedData[curlAddress + 3] >> 5) & 1) == 1)
                        {
                            tYFlip1 = (byte)((shpsz[shpszIndex + 1] >> 3) - 1);
                            tYFlip2 = 0;
                            yStep = -1;
                        }
                        else
                        {
                            tYFlip1 = 0;
                            tYFlip2 = (byte)((shpsz[shpszIndex + 1] >> 3) - 1);
                            yStep = 1;
                        }

                        for (int tileY = tYFlip1; (yStep == -1 && tileY >= tYFlip2) || (yStep == 1 && tileY <= tYFlip2); tileY += yStep)
                        {
                            for (int tileX = tXFlip1; (xStep == -1 && tileX >= tXFlip2) || (xStep == 1 && tileX <= tXFlip2); tileX += xStep)
                            {
                                for (int ypix = 0; ypix < 8; ypix++)
                                {
                                    for (int xpix = 0; xpix < 8; xpix++)
                                    {
                                        if (AssociatedTiles.PixelData[num] != 0)
                                        {
                                            bitmap.SetPixel(lyrX + (tileX << 3) + Math.Abs((-7 * ((DecompressedData[curlAddress + 3] >> 4) & 1)) + xpix),
                                                lyrY + (tileY << 3) + Math.Abs((-7 * ((DecompressedData[curlAddress + 3] >> 5) & 1)) + ypix),
                                                AssociatedPalette.Palette[AssociatedTiles.PixelData[num]]);
                                        }
                                        num++;
                                    }
                                }
                            }
                        }
                        break;
                }
            }
    
            return bitmap;
        }
    }
}
