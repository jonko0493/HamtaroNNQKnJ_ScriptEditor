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
    public class GraphicsTests
    {
        private const string NINTENDO_LOGO_TILES = ".\\inputs\\GraphicsNintendoLogoTiles.dat";

        [Test]
        public void NintendoLogoParseTest()
        {
            var data = File.ReadAllBytes(NINTENDO_LOGO_TILES);
            var graphicsDriver = new GraphicsDriver();
            File.WriteAllBytes("pixels.dat", graphicsDriver.GetTilePixels(data));
        }
    }
}
