using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamtaroNNQKnJ_ScriptEditor
{
    public class PaletteFile : FileInDirectory
    {
        public int Index { get; set; }
        public short UnknownShort1 { get; set; }
        public short UnknownShort2 { get; set; }
        public short UnknownShort3 { get; set; }

        public List<Color> Palette { get; set; } = new List<Color>();

        public void LoadColors(byte[] data)
        {
            for (int i = 0; i < data.Length; i += 2)
            {
                short color = BitConverter.ToInt16(new byte[] { data[i], data[i + 1] });
                Palette.Add(Color.FromArgb((color & 0x1F) << 3, ((color >> 5) & 0x1F) << 3, ((color >> 10) & 0x1F) << 3));
            }

            while (Palette.Count < 256)
            {
                Palette.Add(Color.FromArgb(0, 0, 0));
            }
        }

        public static PaletteFile ParseFromData(byte[] data)
        {
            if (data.Length % 2 != 0)
            {
                throw new ArgumentException($"Palette has invalid length of {data.Length} -- must be even number");
            }

            PaletteFile paletteFile = new PaletteFile();
            paletteFile.LoadColors(data);

            return paletteFile;
        }

        public static PaletteFile ParseFromFile(string file)
        {
            return ParseFromData(File.ReadAllBytes(file));
        }

        public byte[] GetRiffPaletteBytes()
        {
            List<byte> riffBytes = new List<byte>();

            int documentSize = 16 + Palette.Count * 4;
            ushort count = (ushort)Palette.Count;

            riffBytes.AddRange(Encoding.ASCII.GetBytes("RIFF"));
            riffBytes.AddRange(BitConverter.GetBytes(documentSize));
            riffBytes.AddRange(Encoding.ASCII.GetBytes("PAL data"));
            riffBytes.AddRange(BitConverter.GetBytes(0));
            riffBytes.AddRange(new byte[] { 0, 3 }); // version
            riffBytes.AddRange(BitConverter.GetBytes(count));

            foreach (Color color in Palette)
            {
                riffBytes.Add(color.R);
                riffBytes.Add(color.G);
                riffBytes.Add(color.B);
                riffBytes.Add(0); // leave flags unset
            }

            return riffBytes.ToArray();
        }

        public void WriteRiffPaletteFile(string file)
        {
            File.WriteAllBytes(file, GetRiffPaletteBytes());
        }

        public Bitmap GetPaletteDisplay()
        {
            Bitmap bitmap = new Bitmap(256, 256);
            for (int x = 0; x < bitmap.Width; x++)
            {
                int offset = 0;
                for (int y = 0; y < bitmap.Height; y++)
                {
                    if (y % 16 == 0 && y != 0)
                    {
                        offset++;
                    }
                    bitmap.SetPixel(x, y, Palette[x / 16 + 16 * offset]);
                }
            }
            return bitmap;
        }
    }
}
