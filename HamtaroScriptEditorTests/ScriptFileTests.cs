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

        [Test]
        public void ParseWriteMatch()
        {
            byte[] dataOnDisk = File.ReadAllBytes(SINGLE_SCRIPT_FILE);
            var scriptFile = ScriptFile.ParseFromData(dataOnDisk);

            byte[] dataInMemory = scriptFile.GetBytes();

            Assert.AreEqual(dataOnDisk, dataInMemory);
        }
    }
}
