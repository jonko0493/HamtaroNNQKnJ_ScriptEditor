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
        private const string MASSIVE_DIRECTORY = ".\\inputs\\MassiveDirectory.dat";

        [Test]
        [TestCase(SIMPLE_LARGE_DIRECTORY_FILE)]
        [TestCase(COMPLEX_LARGE_DIRECTORY_FILE)]
        [TestCase(MASSIVE_DIRECTORY)]
        public void ParseWriteMatchTest(string file)
        {
            byte[] dataOnDisk = File.ReadAllBytes(file);
            DirectoryFile directoryFile = DirectoryFile.ParseFromData(dataOnDisk);

            byte[] dataInMemory = directoryFile.GetBytes();

            Assert.AreEqual(dataOnDisk, dataInMemory);
        }

        [Test]
        [TestCase(COMPLEX_LARGE_DIRECTORY_FILE, 11)]
        [TestCase(MASSIVE_DIRECTORY, 0)]
        public void ReinsertFileTest(string file, int fileIndex)
        {
            byte[] dataOnDisk = File.ReadAllBytes(file);
            DirectoryFile directoryFile = DirectoryFile.ParseFromData(dataOnDisk);

            ScriptFile scriptFile = ScriptFile.ParseFromData(directoryFile.FilesInDirectory[fileIndex].Content);
            scriptFile.Messages[0].Text = scriptFile.Messages[0].Text.Replace("やったー", "Wooo");
            directoryFile.ReinsertFile(fileIndex, scriptFile);

            byte[] dataInMemory = directoryFile.GetBytes();

            Assert.AreEqual(dataOnDisk, dataInMemory);
        }

        [Test]
        [TestCase(COMPLEX_LARGE_DIRECTORY_FILE, 11, 1, "やったー")]
        //[TestCase(MASSIVE_DIRECTORY, 0)]
        public void RecalculatePointersTest(string file, int fileIndex, int messageIndex, string textToReplace)
        {
            byte[] dataOnDisk = File.ReadAllBytes(file);
            DirectoryFile directoryFile = DirectoryFile.ParseFromData(dataOnDisk);

            string[] replacements = new string[] { "W", "Wo", "Woo", "Wooh", "Wooho", "Woohoo", "Woohoo!" };

            for (int i = 0; i < replacements.Length; i++)
            {
                try
                {
                    string previousReplacement = i == 0 ? textToReplace : replacements[i - 1];

                    ScriptFile scriptFile = ScriptFile.ParseFromData(directoryFile.FilesInDirectory[fileIndex].Content);
                    scriptFile.Messages[messageIndex].Text = scriptFile.Messages[messageIndex].Text.Replace(previousReplacement, replacements[i]);
                    directoryFile.ReinsertFile(fileIndex, scriptFile);

                    ScriptFile reParseFile = ScriptFile.ParseFromData(directoryFile.FilesInDirectory[fileIndex].Content);
                }
                catch (Exception e)
                {
                    throw new Exception($"Failed on replacement '{replacements[i]}'", e);
                }
            }

            byte[] dataInMemory = directoryFile.GetBytes();
        }
    }
}
