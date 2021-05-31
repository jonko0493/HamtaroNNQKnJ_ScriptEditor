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
        private const string NINTENDO_LOGO_COMPRESSED_TILES = ".\\inputs\\GraphicsNintendoLogoCompressedTiles.dat";

        private const string NINTENDO_LOGO_TILES_PIXEL_DATA = ".\\inputs\\GraphicsNintendoLogoTilePixels.dat";

        [Test]
        public void NintendoLogoParsePixelsTest()
        {
            var compressedData = File.ReadAllBytes(NINTENDO_LOGO_COMPRESSED_TILES);
            var graphicsDriver = new GraphicsDriver();
            var pixelDataInMemory = graphicsDriver.GetTilePixels(compressedData);

            var pixelDataOnDisk = File.ReadAllBytes(NINTENDO_LOGO_TILES_PIXEL_DATA);
            Assert.AreEqual(pixelDataOnDisk, pixelDataInMemory);
        }
    }
}
