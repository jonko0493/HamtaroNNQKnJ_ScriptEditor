using FluentAssertions;
using NUnit;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamtaroNNQKnJ_ScriptEditor.Tests
{
    public class DirectoryFileTests
    {
        private const string SIMPLE_LARGE_DIRECTORY_FILE = ".\\inputs\\SimpleLargeDirectory.dat";
        private const string COMPLEX_LARGE_DIRECTORY_FILE = ".\\inputs\\ComplexLargeDirectory.dat";
        private const string MASSIVE_DIRECTORY_FILE = ".\\inputs\\MassiveDirectory.dat";
        private const string COMPLEX_LARGE_DIRECTORY_FILE_EDITED = ".\\inputs\\ComplexLargeDirectoryEdited.dat";
        private const string MASSIVE_DIRECTORY_FILE_EDITED = ".\\inputs\\MassiveDirectoryEdited.dat";

        [Test]
        [TestCase(SIMPLE_LARGE_DIRECTORY_FILE)]
        [TestCase(COMPLEX_LARGE_DIRECTORY_FILE)]
        [TestCase(MASSIVE_DIRECTORY_FILE)]
        public void ParseWriteMatchTest(string file)
        {
            byte[] dataOnDisk = File.ReadAllBytes(file);
            DirectoryFile directoryFile = DirectoryFile.ParseFromData(dataOnDisk);

            byte[] dataInMemory = directoryFile.GetBytes();

            Assert.AreEqual(dataOnDisk, dataInMemory);
        }

        [Test]
        [TestCase(COMPLEX_LARGE_DIRECTORY_FILE, 11)]
        [TestCase(MASSIVE_DIRECTORY_FILE, 0)]
        public void ReinsertFileTest(string file, int fileIndex)
        {
            byte[] dataOnDisk = File.ReadAllBytes(file);
            DirectoryFile directoryFile = DirectoryFile.ParseFromData(dataOnDisk);

            MessageFile scriptFile = MessageFile.ParseFromData(directoryFile.FilesInDirectory[fileIndex].Content);
            directoryFile.ReinsertMessageFile(fileIndex, scriptFile);

            byte[] dataInMemory = directoryFile.GetBytes();

            Assert.AreEqual(dataOnDisk, dataInMemory);
        }

        [Test]
        [TestCase(COMPLEX_LARGE_DIRECTORY_FILE, 11, 1, "やったー", COMPLEX_LARGE_DIRECTORY_FILE_EDITED)]
        [TestCase(MASSIVE_DIRECTORY_FILE, 0, 2, "ランク", MASSIVE_DIRECTORY_FILE_EDITED)]
        public void RecalculatePointersTest(string file, int fileIndex, int messageIndex, string textToReplace, string editedFile)
        {
            DirectoryFile directoryFile = DirectoryFile.ParseFromFile(file);

            string[] replacements = new string[] { "W", "Wo", "Woo", "Wooh", "Wooho", "Woohoo", "Woohoo!" };

            for (int i = 0; i < replacements.Length; i++)
            {
                try
                {
                    string previousReplacement = i == 0 ? textToReplace : replacements[i - 1];

                    MessageFile scriptFile = MessageFile.ParseFromData(directoryFile.FilesInDirectory[fileIndex].Content);
                    scriptFile.Messages[messageIndex].Text = scriptFile.Messages[messageIndex].Text.Replace(previousReplacement, replacements[i]);
                    directoryFile.ReinsertMessageFile(fileIndex, scriptFile);

                    MessageFile reParseFile = MessageFile.ParseFromData(directoryFile.FilesInDirectory[fileIndex].Content);
                }
                catch (Exception e)
                {
                    throw new Exception($"Failed on replacement '{replacements[i]}'", e);
                }
            }

            byte[] dataInMemory = directoryFile.GetBytes();
            byte[] dataOnDisk = File.ReadAllBytes(editedFile);

            Assert.AreEqual(dataOnDisk, dataInMemory);
        }
    }
}
