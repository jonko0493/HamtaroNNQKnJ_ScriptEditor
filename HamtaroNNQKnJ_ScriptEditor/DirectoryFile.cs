using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamtaroNNQKnJ_ScriptEditor
{
    public interface IDirectoryItem
    {
        public int Offset { get; set; }
        public int Level { get; set; }
    }

    public class DirectoryFile : IDirectoryItem
    {
        public int Offset { get; set; }
        public int Level { get; set; }
        public List<DirectoryFile> Subdirectories { get; set; } = new List<DirectoryFile>();
        public List<FileInDirectory> FilesInDirectory { get; set; } = new List<FileInDirectory>();

        public static DirectoryFile ParseFromFile(string file)
        {
            byte[] data = File.ReadAllBytes(file);
            int firstFilePointer = (int)GetFirstFilePointer(data, 0);
            var directoryFile = new DirectoryFile { Offset = 0, Level = -1 };
            directoryFile.GetAllFilesRecurse(data, 0, firstFilePointer);
            directoryFile.GetAllFileContents(data, data.Length);

            return directoryFile;
        }

        public List<IDirectoryItem> GetAllDirectoryItems()
        {
            var directoryItems = new List<IDirectoryItem>();
            foreach (var subdirectory in Subdirectories)
            {
                directoryItems.Add(subdirectory);
                directoryItems.AddRange(subdirectory.GetAllDirectoryItems());
            }
            directoryItems.AddRange(FilesInDirectory);

            return directoryItems;
        }

        public List<FileInDirectory> GetAllFiles()
        {
            var allFiles = new List<FileInDirectory>();

            foreach (var subdirectory in Subdirectories)
            {
                allFiles.AddRange(subdirectory.GetAllFiles());
            }
            allFiles.AddRange(FilesInDirectory);

            return allFiles;
        }

        public override string ToString()
        {
            string indent = "";
            for (int i = 0; i < Level; i++)
            {
                indent += "\t";
            }
            return $"{indent}{Subdirectories.Count} subdirectories; {FilesInDirectory.Count} files";
        }

        private static uint GetFirstFilePointer(byte[] data, uint pointer)
        {
            uint nextPointer = pointer + BitConverter.ToUInt32(new byte[] { data[pointer], data[pointer + 1], data[pointer + 2], data[pointer + 3] });
            uint deepPointer = nextPointer + BitConverter.ToUInt32(new byte[] { data[nextPointer], data[nextPointer + 1], data[nextPointer + 2], data[nextPointer + 3] });

            if (deepPointer < data.Length)
            {
                return GetFirstFilePointer(data, nextPointer);
            }
            else
            {
                return nextPointer;
            }
        }

        private void GetAllFilesRecurse(byte[] data, int pointer, int firstFilePointer)
        {
            for (int i = pointer; i < firstFilePointer; i += 4)
            {
                int nextPointer = pointer + BitConverter.ToInt32(new byte[] { data[i], data[i + 1], data[i + 2], data[i + 3] });
                if (nextPointer > 0 && nextPointer < data.Length)
                {
                    int deepPointer = nextPointer < data.Length - 1 ?
                    nextPointer + BitConverter.ToInt32(new byte[] { data[nextPointer], data[nextPointer + 1], data[nextPointer + 2], data[nextPointer + 3] }) : data.Length;
                    if (nextPointer < firstFilePointer && deepPointer < firstFilePointer)
                    {
                        DirectoryFile subdirectory = new DirectoryFile { Offset = nextPointer, Level = Level + 1 };
                        subdirectory.GetAllFilesRecurse(data, nextPointer, firstFilePointer);
                        Subdirectories.Add(subdirectory);
                        if (Subdirectories.Count == 1)
                        {
                            firstFilePointer = subdirectory.Offset;
                        }
                    }
                    else
                    {
                        FilesInDirectory.Add(new FileInDirectory { Offset = nextPointer, Level = Level + 1 });
                    }
                }
            }
        }

        private void GetAllFileContents(byte[] data, int endPointer)
        {
            for (int i = 0; i < Subdirectories.Count; i++)
            {
                int subdirectoryEndPointer;
                if (i < Subdirectories.Count - 1 && FilesInDirectory.Count > 0)
                {
                    subdirectoryEndPointer = Math.Min(SubdirectoryEndPointer(Subdirectories[i + 1], endPointer), FilesInDirectory[0].Offset);
                }
                else if (i < Subdirectories.Count - 1)
                {
                    subdirectoryEndPointer = SubdirectoryEndPointer(Subdirectories[i + 1], endPointer);
                }
                else if (FilesInDirectory.Count > 0)
                {
                    subdirectoryEndPointer = FilesInDirectory[0].Offset;
                }
                else
                {
                    subdirectoryEndPointer = endPointer;
                }

                Subdirectories[i].GetAllFileContents(data, subdirectoryEndPointer);
            }

            for (int i = 0; i < FilesInDirectory.Count; i++)
            {
                int nextOffset;
                if (i == FilesInDirectory.Count - 1)
                {
                    nextOffset = endPointer;
                }
                else
                {
                    nextOffset = FilesInDirectory[i + 1].Offset;
                }

                List<byte> content = new List<byte>();
                for (int j = FilesInDirectory[i].Offset; j < nextOffset; j++)
                {
                    content.Add(data[j]);
                }
                FilesInDirectory[i].Content = content.ToArray();
            }
        }

        private int SubdirectoryEndPointer(DirectoryFile nextSubdirectory, int endPointer)
        {
            int firstSubdirectoryOffset = endPointer, firstFileOffset = endPointer;
            if (nextSubdirectory.Subdirectories.Count > 0)
            {
                firstSubdirectoryOffset = nextSubdirectory.Subdirectories[0].Offset;
            }
            if (nextSubdirectory.FilesInDirectory.Count > 0)
            {
                firstFileOffset = nextSubdirectory.FilesInDirectory[0].Offset;
            }
            return Math.Min(firstSubdirectoryOffset, firstFileOffset);
        }
    }

    public class FileInDirectory : IDirectoryItem
    {
        public int Offset { get; set; }
        public int Level { get; set; }
        public byte[] Content { get; set; }

        public override string ToString()
        {
            string indent = "";
            for (int i = 0; i < Level; i++)
            {
                indent += "\t";
            }
            return $"{indent}Offset: 0x{Offset:X4}; Size: {Content.Length} bytes";
        }
    }
}
