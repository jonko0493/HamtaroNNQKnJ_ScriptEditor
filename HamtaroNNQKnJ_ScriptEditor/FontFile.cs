using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamtaroNNQKnJ_ScriptEditor
{
    public class FontFile : FileInDirectory
    {
        public List<ushort> CharList { get; set; } = new List<ushort>();
        public int UnknownInt { get; set; }
        public (int x, int y) Size { get; set; }
        public byte Layers { get; set; }
        public List<int> SizeList { get; set; } = new List<int>();
        public List<Bitmap> ImageList { get; set; } = new List<Bitmap>();

        public static FontFile FromData(byte[] data, int offset = 0, string notes = "")
        {
            FontFile fontFile = new FontFile();
            fontFile.Content = data;
            fontFile.FileType = "Font File";
            fontFile.Offset = offset;
            fontFile.Notes = notes;

            int endPartSize = BitConverter.ToInt32(data.Take(4).ToArray());
            int endPartPointer = BitConverter.ToInt32(data.Skip(0x04).Take(4).ToArray());
            for (int i = endPartPointer; i < endPartPointer + endPartSize; i += 2)
            {
                fontFile.CharList.Add(BitConverter.ToUInt16(data.Skip(i).Take(2).ToArray()));
            }

            fontFile.UnknownInt = BitConverter.ToInt32(data.Skip(0x08).Take(4).ToArray());

            ushort sizeSpecs = BitConverter.ToUInt16(data.Skip(0x0C).Take(2).ToArray());
            int height = 4 * (sizeSpecs & 0x0F);
            int width = (4 * (sizeSpecs & 0xF0)) >> 4;
            fontFile.Layers = data[0x0E];

            int imageSize = (width * height * fontFile.Layers) / 8;
            int aligned = imageSize;
            if (aligned % 4 != 0)
            {
                aligned += 4 - (aligned % 4);
            }

            int sizeStr = data[0x0F] * 4;
            for (int i = 0x10; i < 0x10 + sizeStr; i++)
            {
                fontFile.SizeList.Add(data[i] % 16);
                fontFile.SizeList.Add(data[i] / 16);
            }

            int actualLayers;
            if (width / 4 == 4 || height / 4 == 4)
            {
                actualLayers = 2;
            }
            else
            {
                actualLayers = fontFile.Layers;
            }

            for (int i = 0x10 + sizeStr; i < endPartPointer; i += aligned)
            {
                Bitmap image = new Bitmap(width, height);
                byte[] characterData = data.Skip(i).Take(imageSize).ToArray();
                int maxT = (int)Math.Ceiling(width / 8.0);

                for (int t = 0; t < maxT; t++)
                {
                    for (int c = 0; c < height / 4; c++)
                    {
                        int maxY = 4;
                        if (t == maxT - 1 && width % 8 != 0)
                        {
                            maxY = 2;
                        }
                        for (int y = 0; y < maxY; y++)
                        {
                            for (int x = 0; x < 8; x++)
                            {
                                for (int l = 0; l < actualLayers; l++)
                                {
                                    if ((characterData[t * height * actualLayers + c * actualLayers * maxY + y + l * maxY] & (1 << x)) > 0)
                                    {
                                        int xPos = t * 8 + y * 2 + x / 4;
                                        int yPos = c * 4 + x % 4;

                                        Color currentColor = image.GetPixel(xPos, yPos);

                                        Color nextColor;
                                        if (currentColor == Color.Transparent)
                                        {
                                            nextColor = Color.Black;
                                        }
                                        else if (currentColor == Color.Black)
                                        {
                                            nextColor = Color.FromArgb(128, 128, 128);
                                        }
                                        else
                                        {
                                            nextColor = Color.FromArgb(192, 192, 192);
                                        }

                                        image.SetPixel(xPos, yPos, nextColor);
                                    }
                                }
                            }
                        }
                    }
                }
                fontFile.Size = (width, height);
                fontFile.ImageList.Add(image);
            }

            return fontFile;
        }
    }
}
