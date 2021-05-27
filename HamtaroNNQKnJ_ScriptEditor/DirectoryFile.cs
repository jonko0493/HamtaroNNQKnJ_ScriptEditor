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

        public static DirectoryFile ParseFromData(byte[] data)
        {
            int firstPointer = data.Length;
            var directoryFile = new DirectoryFile();

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
                    content.Add(data[j]);
                }
                directoryFile.FilesInDirectory[i].Content = content.ToArray();
            }

            return directoryFile;
        }

        public static DirectoryFile ParseFromFile(string file)
        {
            return ParseFromData(File.ReadAllBytes(file));
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

        public void ReinsertFile(int index, ScriptFile scriptFile)
        {
            FilesInDirectory[index].Content = scriptFile.GetBytes();
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
    }

    public class FileInDirectory
    {
        public int Offset { get; set; }
        public byte[] Content { get; set; }

        public override string ToString()
        {
            return $"Offset: 0x{Offset:X4}; Size: {Content.Length} bytes";
        }
    }
}
