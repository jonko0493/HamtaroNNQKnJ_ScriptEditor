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
        private const string SIMPLE_SMALL_DIRECTORY_FILE = ".\\inputs\\SimpleSmallDirectory.dat";
        private const string SIMPLE_LARGE_DIRECTORY_FILE = ".\\inputs\\SimpleLargeDirectory.dat";
        private const string COMPLEX_LARGE_DIRECTORY_FILE = ".\\inputs\\ComplexLargeDirectory.dat";

        [Test]
        [TestCase(SIMPLE_SMALL_DIRECTORY_FILE)]
        [TestCase(SIMPLE_LARGE_DIRECTORY_FILE)]
        [TestCase(COMPLEX_LARGE_DIRECTORY_FILE)]
        public void ParseWriteMatchTest(string file)
        {
            byte[] dataOnDisk = File.ReadAllBytes(file);
            DirectoryFile directoryFile = DirectoryFile.ParseFromData(dataOnDisk);

            byte[] dataInMemory = directoryFile.GetBytes();

            Assert.AreEqual(dataOnDisk, dataInMemory);
        }

        [Test]
        [TestCase(SIMPLE_SMALL_DIRECTORY_FILE, 0)]
        [TestCase(COMPLEX_LARGE_DIRECTORY_FILE, 11)]
        public void ReinsertFileTest(string file, int fileIndex)
        {
            byte[] dataOnDisk = File.ReadAllBytes(file);
            DirectoryFile directoryFile = DirectoryFile.ParseFromData(dataOnDisk);

            ScriptFile scriptFile = ScriptFile.ParseFromData(directoryFile.FilesInDirectory[fileIndex].Content);
            //scriptFile.Messages[0].Text = scriptFile.Messages[0].Text.Replace("やったー", "Wooo");
            directoryFile.ReinsertFile(fileIndex, scriptFile);

            byte[] dataInMemory = directoryFile.GetBytes();

            Assert.AreEqual(dataOnDisk, dataInMemory);
        }

        [Test]
        public void RecalculatePointersTest()
        {

        }
    }
}
