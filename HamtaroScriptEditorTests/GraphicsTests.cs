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

        private const string ALPHA_DREAM_LOGO_SPRITE_COMPRESSED_TILES = ".\\inputs\\GraphicsAlphaDreamLogoSpriteCompressedTiles.dat";
        private const string ALPHA_DREAM_LOGO_SPRITE_TILES_PIXEL_DATA = ".\\inputs\\GraphicsAlphaDreamLogoSpriteTilePixels.dstile";

        private const string HAJIMERU_SPRITE_COMPRESSED_TILES = ".\\inputs\\GraphicsHajimeruSpriteCompressedTiles.dat";
        private const string HAJIMERU_SPRITE_TILES_PIXEL_DATA = ".\\inputs\\GraphicsHajimeruSpriteTilePixels.dstile";

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
        public void NewBgDecompressionAlgorithmTest(string compressedDataFile, string pixelDataFile)
        {
            var compressedData = File.ReadAllBytes(compressedDataFile);

            var asmGraphicsDriver = new GraphicsDriver();

            var asmSimulatorPixelData = asmGraphicsDriver.GetBgTilePixelsUsingCrudeASMSimulator(compressedData);
            var newAlgorithmPixelData = GraphicsDriver.DecompressBgTiles(compressedData);

            Assert.AreEqual(newAlgorithmPixelData, asmSimulatorPixelData);
        }

        [Test]
        [TestCase(ALPHA_DREAM_LOGO_SPRITE_COMPRESSED_TILES, ALPHA_DREAM_LOGO_SPRITE_TILES_PIXEL_DATA)]
        [TestCase(HAJIMERU_SPRITE_COMPRESSED_TILES, HAJIMERU_SPRITE_TILES_PIXEL_DATA)]
        public void ParseSpritePixelsTest(string compressedDataFile, string pixelDataFile)
        {
            var compressedData = File.ReadAllBytes(compressedDataFile);

            var asmGraphicsDriver = new GraphicsDriver();
            var pixelDataInMemory = asmGraphicsDriver.GetSpriteTilePixelsUsingCrudeASMSimulator(compressedData);

            var pixelDataOnDisk = File.ReadAllBytes(pixelDataFile);
            Assert.AreEqual(pixelDataOnDisk, pixelDataInMemory);
        }

        [Test]
        [TestCase(ALPHA_DREAM_LOGO_SPRITE_COMPRESSED_TILES, ALPHA_DREAM_LOGO_SPRITE_TILES_PIXEL_DATA)]
        [TestCase(HAJIMERU_SPRITE_COMPRESSED_TILES, HAJIMERU_SPRITE_TILES_PIXEL_DATA)]
        public void NewSpriteDecompressionAlgorithmTest(string compressedDataFile, string pixelDataFile)
        {
            var compressedData = File.ReadAllBytes(compressedDataFile);

            var asmGraphicsDriver = new GraphicsDriver();
            var newGraphicsDriver = new GraphicsDriver();

            var asmSimulatorPixelData = asmGraphicsDriver.GetSpriteTilePixelsUsingCrudeASMSimulator(compressedData);
            var newAlgorithmPixelData = newGraphicsDriver.DecompressSpriteTiles(compressedData);

            Assert.AreEqual(newAlgorithmPixelData, asmSimulatorPixelData);
        }
    }
}
