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

        private const string COMPRESSED_TILE_FILE_80 = ".\\inputs\\Graphics80CompressedTileFile.dat";
        private const string DECOMPRESSED_TILE_FILE_80 = ".\\inputs\\Graphics80DecompressedTileFile.dstile";

        private const string COMPRESSED_TILE_FILE_A0 = ".\\inputs\\GraphicsA0CompressedTileFile.dat";
        private const string DECOMPRESSED_TILE_FILE_A0 = ".\\inputs\\GraphicsA0DecompressedTileFile.dstile";

        private const string BUTTONS_PALETTE_DATA = ".\\inputs\\PaletteButtons.dat";
        private const string BUTTONS_PALETTE_RIFF = ".\\inputs\\PaletteButtons.pal";

        private const string HAMTARO_PALETTE_DATA = ".\\inputs\\PaletteHamtaro.dat";
        private const string HAMTARO_PALETTE_RIFF = ".\\inputs\\PaletteHamtaro.pal";

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
            var newAlgorithmPixelData = newGraphicsDriver.DecompressSpriteTilesUsingRefinedAsmSimulator(compressedData);

            Assert.AreEqual(asmSimulatorPixelData, newAlgorithmPixelData);
        }

        [Test]
        [TestCase(ALPHA_DREAM_LOGO_SPRITE_COMPRESSED_TILES)]
        [TestCase(HAJIMERU_SPRITE_COMPRESSED_TILES)]
        public void YoshiMagicSpriteDecompressionAlgorithTest(string compressedDataFile)
        {
            var compressedData = File.ReadAllBytes(compressedDataFile);

            var graphicsDriver = new GraphicsDriver();

            var yoshiMagicaPixelData = GraphicsDriver.DecompressSpriteData(compressedData);
            var oldAlgorithmPixelData = graphicsDriver.DecompressSpriteTilesUsingRefinedAsmSimulator(compressedData);

            Assert.AreEqual(oldAlgorithmPixelData, yoshiMagicaPixelData);
        }

        [Test]
        [TestCase(ALPHA_DREAM_LOGO_SPRITE_TILES_PIXEL_DATA)]
        [TestCase(HAJIMERU_SPRITE_TILES_PIXEL_DATA)]
        [TestCase(DECOMPRESSED_TILE_FILE_80)]
        [TestCase(DECOMPRESSED_TILE_FILE_A0)]
        public void SpriteCompressionAlgorithmTest(string pixelDataFile)
        {
            var originalDecompressedData = File.ReadAllBytes(pixelDataFile);
            var compressedData = GraphicsDriver.CompressSpriteData(originalDecompressedData);

            byte[] newDecompressedData;
            if (compressedData[0] == 0x40)
            {
                // we use the ASM simulator for small files because it's slightly closer to the original subroutine
                var graphicsDriver = new GraphicsDriver();
                newDecompressedData = graphicsDriver.DecompressSpriteTilesUsingRefinedAsmSimulator(compressedData);
            }
            else
            {
                // but I haven't made it work for large files yet lol
                newDecompressedData = GraphicsDriver.DecompressSpriteData(compressedData);
            }

            Assert.AreEqual(originalDecompressedData, newDecompressedData);
        }

        [Test]
        [TestCase(BUTTONS_PALETTE_DATA, BUTTONS_PALETTE_RIFF)]
        [TestCase(HAMTARO_PALETTE_DATA, HAMTARO_PALETTE_RIFF)]
        public void PaletteConversionTest(string paletteDataFile, string paletteRiffFile)
        {
            var paletteData = File.ReadAllBytes(paletteDataFile);
            var paletteRiffOnDisk = File.ReadAllBytes(paletteRiffFile);

            PaletteFile paletteFile = PaletteFile.ParseFromData(paletteData);
            var paletteRiffInMemory = paletteFile.GetRiffPaletteBytes();

            Assert.AreEqual(paletteRiffOnDisk, paletteRiffInMemory);
        }
    }
}
