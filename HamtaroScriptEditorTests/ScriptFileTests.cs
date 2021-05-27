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
    public class ScriptFileTests
    {
        private const string SINGLE_SCRIPT_FILE = ".\\inputs\\SingleScriptFile.dat";
        private const string CUTSCENE_SCRIPT_FILE = ".\\inputs\\CutsceneScriptFile.dat";
        private const string SCRIPT_WITH_INTRO_BYTES = ".\\inputs\\SingleScriptWithIntroBytes.dat";
        private const string EDITED_POINTERS_FILE = ".\\inputs\\SingleScriptFileEditedPointers.dat";

        [Test]
        [TestCase(SINGLE_SCRIPT_FILE)]
        [TestCase(CUTSCENE_SCRIPT_FILE)]
        [TestCase(SCRIPT_WITH_INTRO_BYTES)]
        public void ParseWriteMatchTest(string file)
        {
            byte[] dataOnDisk = File.ReadAllBytes(file);
            var scriptFile = ScriptFile.ParseFromData(dataOnDisk);

            byte[] dataInMemory = scriptFile.GetBytes();

            Assert.AreEqual(dataOnDisk, dataInMemory);
        }

        [Test]
        public void RecalculatePointersTest()
        {
            byte[] originalData = File.ReadAllBytes(SINGLE_SCRIPT_FILE);
            var scriptFile = ScriptFile.ParseFromData(originalData);

            scriptFile.Messages[2].Text = "<0x0B>Test\nt\nえらんでね!\n<0x0A>";

            byte[] dataInMemory = scriptFile.GetBytes();
            byte[] dataOnDisk = File.ReadAllBytes(EDITED_POINTERS_FILE);

            Assert.AreEqual(dataOnDisk, dataInMemory);
        }
    }
}
