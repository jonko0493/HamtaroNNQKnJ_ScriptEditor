using FluentAssertions;
using HamtaroNNQKnJ_ScriptEditor;
using NUnit;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamtaroNNQKnJ_ScriptEditor.tests
{
    public class ScriptFileTests
    {
        private static string SINGLE_SCRIPT_FILE = ".\\inputs\\SingleScriptFile.dat";
        private static string EDITED_POINTERS_FILE = ".\\inputs\\SingleScriptFileEditedPointers.dat";

        [Test]
        public void ParseWriteMatchTest()
        {
            byte[] dataOnDisk = File.ReadAllBytes(SINGLE_SCRIPT_FILE);
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
