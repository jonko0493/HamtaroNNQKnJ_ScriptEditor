using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamtaroNNQKnJ_ScriptEditor
{
    public class DirectoryFile
    {
        public List<FileInDirectory> FilesInDirectory { get; set; } = new List<FileInDirectory>();
        public int FileStart { get; set; } = 0;
        public int FileEnd { get; set; } = 0;
        public string FileName { get; set; }

        public static DirectoryFile ParseFromData(byte[] data, string fileName = "")
        {
            int firstPointer = data.Length;
            var directoryFile = new DirectoryFile { FileName = fileName };

            int firstByte = BitConverter.ToInt32(new byte[] { data[0], data[1], data[2], data[3] });
            int secondByte = BitConverter.ToInt32(new byte[] { data[4], data[5], data[6], data[7] });
            if (firstByte == 0x08 && secondByte == data.Length)
            {
                directoryFile.FileStart = firstByte;
                directoryFile.FileEnd = secondByte;
            }

            for (int i = directoryFile.FileStart; i < data.Length && i < firstPointer; i += 4)
            {
                if (i < firstPointer)
                {
                    int pointer = directoryFile.FileStart + BitConverter.ToInt32(new byte[] { data[i], data[i + 1], data[i + 2], data[i + 3] });
                    if (i == directoryFile.FileStart)
                    {
                        firstPointer = pointer;
                    }
                    directoryFile.FilesInDirectory.Add(new FileInDirectory { Offset = pointer });
                }
            }

            for (int i = 0; i < directoryFile.FilesInDirectory.Count; i++)
            {
                int nextOffset;
                if (i == directoryFile.FilesInDirectory.Count - 1)
                {
                    nextOffset = data.Length;
                }
                else
                {
                    nextOffset = directoryFile.FilesInDirectory[i + 1].Offset;
                }
                var content = new List<byte>();
                for (int j = directoryFile.FilesInDirectory[i].Offset; j < nextOffset; j++)
                {
                    if (j > 0)
                    {
                        content.Add(data[j]);
                    }
                }
                directoryFile.FilesInDirectory[i].Content = content.ToArray();
            }

            return directoryFile;
        }

        public static DirectoryFile ParseFromFile(string file)
        {
            return ParseFromData(File.ReadAllBytes(file), Path.GetFileName(file));
        }

        public byte[] GetBytes()
        {
            RecalculatePointers();

            List<byte> data = new List<byte>();

            if (FileStart != FileEnd)
            {
                data.AddRange(BitConverter.GetBytes(FileStart));
                data.AddRange(BitConverter.GetBytes(FileEnd));
            }

            foreach (var file in FilesInDirectory)
            {
                data.AddRange(BitConverter.GetBytes(file.Offset));
            }
            foreach (var file in FilesInDirectory)
            {
                data.AddRange(file.Content);
            }

            return data.ToArray();
        }

        public void WriteToFile(string file)
        {
            File.WriteAllBytes(file, GetBytes());
        }

        public void ReinsertMessageFile(int index, MessageFile messageFile)
        {
            FilesInDirectory[index].Content = messageFile.GetBytes();
            RecalculatePointers();
        }

        public void ReinsertSpriteFile(int index, TileFile tileFile)
        {
            tileFile.Notes = FilesInDirectory[index].Notes;
            tileFile.FileType = FilesInDirectory[index].FileType;

            if (FilesInDirectory[index].GetType() == typeof(TileFile))
            {
                TileFile currentFile = (TileFile)FilesInDirectory[index];
                tileFile.SpriteMapFile = currentFile.SpriteMapFile;
                tileFile.Palette = currentFile.Palette;
                tileFile.SpriteMapFile.AssociatedTiles = tileFile;
            }

            if (tileFile.Content.Length % 4 != 0)
            {
                List<byte> tileFileContent = tileFile.Content.ToList();
                for (int i = tileFile.Content.Length % 4; i < 4; i++)
                {
                    tileFileContent.Add(0x00);
                }
                tileFile.Content = tileFileContent.ToArray();
            }

            FilesInDirectory[index] = tileFile;

            RecalculatePointers();
        }

        private void RecalculatePointers()
        {
            FilesInDirectory[0].Offset = FilesInDirectory.Count * 4; // Adds the number of 32-bit integer bytes (4 * # of pointers) as first pointer
            for (int i = 1; i < FilesInDirectory.Count; i++)
            {
                FilesInDirectory[i].Offset = FilesInDirectory[i - 1].Offset + FilesInDirectory[i - 1].Content.Length;
            }

            if (FileEnd != FileStart)
            {
                FileEnd = FilesInDirectory.Last().Offset + FilesInDirectory.Last().Content.Length;
            }
        }

        // This routine is borrowed from Yoshi Magic and modified
        public void ParseSpriteIndexFile()
        {
            byte[] spriteIndexFileData = FilesInDirectory[0].Content;
            FilesInDirectory[0].FileType = "Sprite File Index";

            int numSpriteMaps = BitConverter.ToInt32(spriteIndexFileData.Take(4).ToArray()); // first word is number of sprites
            int numPals = BitConverter.ToInt32(spriteIndexFileData.Skip(4).Take(4).ToArray()); // second word is number of palettes

            // loop through palettes first
            List<PaletteFile> paletteFiles = new List<PaletteFile>();
            for (int i = 8 + numSpriteMaps * 8; i < numPals * 8 + numSpriteMaps * 8 + 8; i += 8)
            {
                short paletteIndex = BitConverter.ToInt16(new byte[] { spriteIndexFileData[i], spriteIndexFileData[i + 1] });
                short unknown1 = BitConverter.ToInt16(new byte[] { spriteIndexFileData[i + 2], spriteIndexFileData[i + 3] });
                short unknown2 = BitConverter.ToInt16(new byte[] { spriteIndexFileData[i + 4], spriteIndexFileData[i + 5] });
                short unknown3 = BitConverter.ToInt16(new byte[] { spriteIndexFileData[i + 6], spriteIndexFileData[i + 7] });

                paletteFiles.Add(new PaletteFile
                {
                    Offset = FilesInDirectory[paletteIndex].Offset,
                    Content = FilesInDirectory[paletteIndex].Content,
                    Notes = FilesInDirectory[paletteIndex].Notes,
                    Index = paletteIndex,
                    UnknownShort1 = unknown1,
                    UnknownShort2 = unknown2,
                    UnknownShort3 = unknown3,
                });
                FilesInDirectory[paletteIndex] = paletteFiles.Last();

                ((PaletteFile)FilesInDirectory[paletteIndex]).LoadColors(FilesInDirectory[paletteIndex].Content);
            }
            // then loop through sprite maps
            for (int i = 8; i < numSpriteMaps * 8 + 8; i += 8)
            {
                short spriteIndex = BitConverter.ToInt16(new byte[] { spriteIndexFileData[i], spriteIndexFileData[i + 1] });
                short associatedPaletteIndex = BitConverter.ToInt16(new byte[] { spriteIndexFileData[i + 2], spriteIndexFileData[i + 3] });
                short unknown1 = BitConverter.ToInt16(new byte[] { spriteIndexFileData[i + 4], spriteIndexFileData[i + 5] });
                short unknown2 = BitConverter.ToInt16(new byte[] { spriteIndexFileData[i + 6], spriteIndexFileData[i + 7] });

                FilesInDirectory[spriteIndex] = new SpriteMapFile
                {
                    Offset = FilesInDirectory[spriteIndex].Offset,
                    Content = FilesInDirectory[spriteIndex].Content,
                    Notes = FilesInDirectory[spriteIndex].Notes,
                    Index = spriteIndex,
                    AssociatedPaletteIndex = associatedPaletteIndex,
                    AssociatedPalette = paletteFiles[associatedPaletteIndex],
                    UnknownShort1 = unknown1,
                    UnknownShort2 = unknown2,
                };
                ((SpriteMapFile)FilesInDirectory[spriteIndex]).Initialize();

                FilesInDirectory[spriteIndex + 1] = new TileFile
                {
                    Offset = FilesInDirectory[spriteIndex + 1].Offset,
                    Content = FilesInDirectory[spriteIndex + 1].Content,
                    Notes = FilesInDirectory[spriteIndex + 1].Notes,
                    FileType = "Sprite Tile File",
                    SpriteMapFile = (SpriteMapFile)FilesInDirectory[spriteIndex],
                };
                ((SpriteMapFile)FilesInDirectory[spriteIndex]).AssociatedTiles = (TileFile)FilesInDirectory[spriteIndex + 1];
                try
                {
                    ((TileFile)FilesInDirectory[spriteIndex + 1]).PixelData = GraphicsDriver.DecompressSpriteData(FilesInDirectory[spriteIndex + 1].Content);
                }
                catch (Exception)
                {
                    FilesInDirectory[spriteIndex + 1].FileType = "Unknown Tile File";
                }
            }
        }
    }

    public class FileInDirectory
    {
        private byte[] _content;

        public int Offset { get; set; }
        public byte[] Content
        {
            get { return _content; }
            set
            {
                _content = value;

                // these are all educated guesses
                if (_content.FirstOrDefault() == 0x10)
                {
                    FileType = "Background File";
                }
                else if (GetType() == typeof(SpriteMapFile))
                {
                    FileType = "Sprite Map File";
                }
                else if (GetType() == typeof(PaletteFile))
                {
                    FileType = "Palette File";
                }
                //else if (MessageFile.CanParse(Content))
                //{
                //    FileType = "Message File";
                //}
                else
                {
                    FileType = "Unknown File";
                }
            }
        }
        public string FileType { get; set; }
        public string Notes { get; set; }

        public override string ToString()
        {
            return $"Offset: 0x{Offset:X4}\tSize: {_content.Length} bytes\t{FileType}\t{Notes}";
        }
    }
}
