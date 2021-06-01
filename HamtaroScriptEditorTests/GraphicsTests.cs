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
        private const string NINTENDO_LOGO_TILES_PIXEL_DATA = ".\\inputs\\GraphicsNintendoLogoTilePixels.dstile";

        private const string ALPHA_DREAM_LOGO_COMPRESSED_TILES = ".\\inputs\\GraphicsAlphaDreamLogoCompressedTiles.dat";
        private const string ALPHA_DREAM_LOGO_TILES_PIXEL_DATA = ".\\inputs\\GraphicsAlphaDreamLogoTilePixels.dstile";

        private const string COPYRIGHT_SCREEN_COMPRESSED_TILES = ".\\inputs\\GraphicsCopyrightScreenCompressedTiles.dat";
        private const string COPYRIGHT_SCREEN_TILES_PIXEL_DATA = ".\\inputs\\GraphicsCopyrightScreenTilePixels.dstile";

        [Test]
        [TestCase(NINTENDO_LOGO_COMPRESSED_TILES, NINTENDO_LOGO_TILES_PIXEL_DATA)]
        [TestCase(ALPHA_DREAM_LOGO_COMPRESSED_TILES, ALPHA_DREAM_LOGO_TILES_PIXEL_DATA)]
        [TestCase(COPYRIGHT_SCREEN_COMPRESSED_TILES, COPYRIGHT_SCREEN_TILES_PIXEL_DATA)]
        public void ParsePixelsTest(string compressedDataFile, string pixelDataFile)
        {
            var compressedData = File.ReadAllBytes(compressedDataFile);
            var graphicsDriver = new GraphicsDriver();
            var pixelDataInMemory = graphicsDriver.GetTilePixels(compressedData);

            var pixelDataOnDisk = File.ReadAllBytes(pixelDataFile);
            Assert.AreEqual(pixelDataOnDisk, pixelDataInMemory);
        }

        [Test]
        [TestCase(NINTENDO_LOGO_COMPRESSED_TILES, NINTENDO_LOGO_TILES_PIXEL_DATA)]
        [TestCase(ALPHA_DREAM_LOGO_COMPRESSED_TILES, ALPHA_DREAM_LOGO_TILES_PIXEL_DATA)]
        [TestCase(COPYRIGHT_SCREEN_COMPRESSED_TILES, COPYRIGHT_SCREEN_TILES_PIXEL_DATA)]
        public void NewDecompressionAlgorithmTest(string compressedDataFile, string pixelDataFile)
        {
            var compressedData = File.ReadAllBytes(compressedDataFile);

            var asmGraphicsDriver = new GraphicsDriver();
            var newGraphicsDriver = new GraphicsDriver();

            var asmSimulatorPixelData = asmGraphicsDriver.GetTilePixelsUsingCrudeASMSimulator(compressedData);
            var newAlgorithmPixelData = newGraphicsDriver.GetTilePixels(compressedData);

            Assert.AreEqual(newAlgorithmPixelData, asmSimulatorPixelData);
        }
    }
}
