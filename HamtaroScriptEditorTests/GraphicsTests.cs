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

        private const string ALPHA_DREAM_LOGO_SPRITE_PIXEL_DATA = ".\\inputs\\GraphicsHajimeruSpriteTiles.dat";

        [Test]
        [TestCase(NINTENDO_LOGO_COMPRESSED_TILES, NINTENDO_LOGO_TILES_PIXEL_DATA)]
        [TestCase(ALPHA_DREAM_LOGO_COMPRESSED_TILES, ALPHA_DREAM_LOGO_TILES_PIXEL_DATA)]
        [TestCase(COPYRIGHT_SCREEN_COMPRESSED_TILES, COPYRIGHT_SCREEN_TILES_PIXEL_DATA)]
        public void ParseBgPixelsTest(string compressedDataFile, string pixelDataFile)
        {
            var compressedData = File.ReadAllBytes(compressedDataFile);
            var pixelDataInMemory = GraphicsDriver.DecompressBgTiles(compressedData);

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

            var asmSimulatorPixelData = asmGraphicsDriver.GetBgTilePixelsUsingCrudeASMSimulator(compressedData);
            var newAlgorithmPixelData = GraphicsDriver.DecompressBgTiles(compressedData);

            Assert.AreEqual(newAlgorithmPixelData, asmSimulatorPixelData);
        }

        [Test]
        [TestCase(ALPHA_DREAM_LOGO_SPRITE_PIXEL_DATA, "")]
        public void ParseSpritePixelsTest(string compressedDataFile, string pixelDataFile)
        {
            var compressedData = File.ReadAllBytes(compressedDataFile);

            var asmGraphicsDriver = new GraphicsDriver();

            var asmSimulatorPixelData = asmGraphicsDriver.GetSpriteTilePixelsUsingCrudeASMSimulator(compressedData);
            File.WriteAllBytes("pixels.dstile", asmSimulatorPixelData);
        }
    }
}
