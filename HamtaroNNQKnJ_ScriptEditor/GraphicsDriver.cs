using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamtaroNNQKnJ_ScriptEditor
{
    public class GraphicsDriver
    {
        private int z = 0; // r0
        private uint a; // r1, current pixel address
        private int b; // r2
        private int c; // r3
        private int d; // r4
        private int e; // r5
        private int f; // r6
        private int g; // r7
        private int h; // r8
        private int i; // r9
        private int j; // r10
        private int k; // r11
        private int l; // r12
        private int m; // r13
        private int n; // r14

        private byte[] data { get; set; }
        private List<byte> _data { get; set; }
        private byte[] pixels { get; set; }
        private List<byte> _pixels { get; set; }
        private int globalByteIndex { get; set; }

        public byte[] GetBgTilePixelsUsingCrudeASMSimulator(byte[] compressedData)
        {
            data = compressedData;
            z = 0;
            a = 0;
            b = 1;
            c = 0;
            d = 0;
            e = 0;
            f = 0;
            g = 0;
            h = 0;
            j = 0;
            k = 0;
            l = 0x020722EC;
            m = 0x027E3A9C;
            n = 0x020747DC;

            Lxx_020747D8();
            return pixels;
        }

        public byte[] GetSpriteTilePixelsUsingCrudeASMSimulator(byte[] compressedData)
        {
            data = compressedData;
            z = 0x020C9A94;
            a = 0x020C9ABC;
            b = 0;
            c = 0;
            d = 0x20C9A8C;
            e = 0x0C;
            f = 0x02066520;
            g = 0; // byte counter
            h = 0; // pixel counter;
            i = 0x020C95E0;
            j = 0x020D0D94;
            k = 0;
            l = 0;
            m = 0x027E3A48;
            n = 0x020666C8;

            _pixels = new List<byte>();
            globalByteIndex = 3;
            SpriteTileDataPrep();
            return _pixels.ToArray();
        }

        public static byte[] DecompressBgTiles(byte[] data)
        {
            int decompressedLength = BitConverter.ToInt32(data.Take(4).ToArray()) >> 8;
            byte[] pixels = new byte[decompressedLength];
            int currentByte = 4;
            int currentPixel = 0;
            int pixelDataHolder = 0;
            int eightSwitch = 0;

            while (decompressedLength > 0)
            {
                int bitwiseChecker = data[currentByte++];
                for (int bytesInDword = 7; bytesInDword >= 0; bytesInDword--)
                {
                    if ((bitwiseChecker & 0x80) != 0)
                    {
                        int bytesToDecompress = 3 + (data[currentByte] >> 4);
                        int combinedTwoBytes = ((data[currentByte] & 0x0F) << 8 | data[currentByte + 1]) + 1;
                        currentByte += 2;
                        int xoredTwoBytes = (8 - eightSwitch) ^ ((combinedTwoBytes & 1) << 3);
                        decompressedLength -= bytesToDecompress;

                        for (; bytesToDecompress > 0; bytesToDecompress--)
                        {
                            xoredTwoBytes ^= 8;
                            int previousPixelOffset = ((combinedTwoBytes + (8 - eightSwitch >> 3)) >> 1) << 1;

                            ushort previousPixels = BitConverter.ToUInt16(
                                new byte[] { pixels[currentPixel - previousPixelOffset],
                                pixels[currentPixel - previousPixelOffset + 1] });

                            pixelDataHolder |= ((previousPixels & (0xFF << xoredTwoBytes)) >> xoredTwoBytes) << eightSwitch;
                            eightSwitch ^= 8;
                            if (eightSwitch == 0)
                            {
                                var cBytes = BitConverter.GetBytes((short)pixelDataHolder);
                                pixels[currentPixel] = cBytes[0];
                                pixels[currentPixel + 1] = cBytes[1];
                                currentPixel += 2;
                                pixelDataHolder = 0;
                            }
                        }
                    }
                    else
                    {
                        if (currentByte < data.Length)
                        {
                            pixelDataHolder |= (data[currentByte++] << eightSwitch);
                            decompressedLength--;
                            eightSwitch ^= 8;
                            if (eightSwitch == 0 && currentPixel < pixels.Length - 1)
                            {
                                var cBytes = BitConverter.GetBytes((short)pixelDataHolder);
                                pixels[currentPixel] = cBytes[0];
                                pixels[currentPixel + 1] = cBytes[1];
                                currentPixel += 2;
                                pixelDataHolder = 0;
                            }
                        }
                    }
                    bitwiseChecker <<= 1;
                }
            }

            return pixels;
        }

        #region BG_Tile_ASM
        private void Lxx_020747D8()
        {
            h = BitConverter.ToInt32(data.Take(4).ToArray());   // ldr      r8,[r0],4h
            z += 4;
            j = h >> 8;                                         // mov      r10,r8,lsr 8h
            pixels = new byte[j];
            b = 0;                                              // mov      r2,0h
            Lxx_020747E4();                                     // continuing naturally
        }

        private void Lxx_020747E4()
        {
            if (j <= 0)                                         // cmp      r10,0h
            {
                // complete                                     // ble      Lxx_20748A0h
            }
            else
            {
                f = data[z];
                z++;                                  // ldrb     r6,[r0],1h
                g = 8;                                          // mov      r7,8h
                Lxx_020747F4();                                 // continuing naturally
            }
        }

        private void Lxx_020747F4()
        {
            g--;                                                // subs     r7,r7,1h
            if (g < 0)
            {
                Lxx_020747E4();                                 // blt      Lxx_20747E4h
            }
            else
            {
                if ((f & 0x80) != 0)                            // tst      r6,80h
                {
                    Lxx_02074820();                             // ble      Lxx_20748A0h;
                }
                else
                {
                    i = data[z];
                    z++;                              // ldrb     r9,[r0],1h
                    c |= (i << b);                              // orr      r3,r3,r9,lsl r2
                    j--;                                        // sub      r10,r10,1h
                    b ^= 8;                                     // eors     r2,r2,8h
                    if (b == 0)
                    {
                        var cBytes = BitConverter.GetBytes((short)c);
                        pixels[a] = cBytes[0];
                        pixels[a + 1] = cBytes[1];
                        a += 2;                                 // strheq   r3,[r1],2h
                        c = 0;                                  // mov      r3,0h
                    }
                    Lxx_02074890();                             // b        Lxx_2074890h
                }
            }
        }

        private void Lxx_02074820()
        {
            i = data[z];                              // ldrb     r9,[r0]
            h = 3;                                              // mov      r8,3h
            e = h + (i >> 4);                                   // add      r5,r8,r9,asr 4h
            i = data[z];
            z++;                                      // ldrb     r9,[r0],1h
            h = i & 0x0F;                                       // and      r8,r9,0Fh
            d = h << 8;                                         // mov      r4,r8,lsl 8h
            i = data[z];
            z++;                                      // ldrb     r9,[r0],1h
            h = i | d;                                          // orr      r8,r9,r4
            d = h + 1;                                          // add      r4,r8,1h
            h = 8 - b;                                          // rsb      r8,r2,8h
            i = d & 1;                                          // and      r9,r4,1h
            n = h ^ (i << 3);                                   // eor      r14,r8,r9,lsl 3h
            j -= e;                                             // sub      r10,r10,r5
            Lxx_02074854();                                     // continue naturally
        }

        private void Lxx_02074854()
        {
            n ^= 8;                                             // eor      r14,r14,8h
            h = 8 - b;                                          // rsb      r8,r2,8h
            h = d + (h >> 3);                                   // add      r8,r4,r8,lsr 3h
            h = h >> 1;                                         // mov      r8,r8,lsr 1h
            h = h << 1;                                         // mov      r8,r8,lsl 1h
            i = BitConverter.ToUInt16(
                new byte[] { pixels[a - h],
                pixels[a - h + 1] });                           // ldrh     r9,[r1,-r8]
            h = 0xFF;                                           // mov      r8,0FFh
            h = i & (h << n);                                   // and      r8,r9,r8,lsl r14
            h = h >> n;                                         // mov      r8,r8,asr r14
            c = c | (h << b);                                   // orr      r3,r3,r8,lsl r2
            b ^= 8;                                             // eors     r2,r2,8h
            if (b == 0)
            {
                var cBytes = BitConverter.GetBytes((short)c);
                pixels[a] = cBytes[0];
                pixels[a + 1] = cBytes[1];
                a += 2;                                         // strheq   r3,[r1],2h
                c = 0;                                          // mov      r3,0h
            }
            e--;                                                // subs     r5,r5,1h
            if (e > 0)
            {
                Lxx_02074854();                                 // bgt      Lxx_2074854h
            }
            else
            {
                if (j > 0)                                      // cmp      r10,0h
                {
                    f = f << 1;                                 // movgt    r6,r6,lsl 1h
                    Lxx_020747F4();                             // bgt      Lxx_20747F4h
                }
                else
                {
                    Lxx_020747E4();                             // b        Lxx_20747E4h
                }
            }
        }

        private void Lxx_02074890()
        {
            if (j > 0)                                          // cmp      r10,0h
            {
                f = f << 1;                                     // movgt    r6,r6,lsl 1h
                Lxx_020747F4();                                 // bgt      Lxx_20747F4h
            }
            else
            {
                Lxx_020747E4();                                 // b        Lxx_20747E4h
            }
        }
        #endregion

        #region Sprite_Tile_ASM
        #region Block_Loading_Subroutine
        public void SpriteTileDataPrep()
        {
            if (globalByteIndex >= data.Length)
            {
                return;
            }
            if (data[globalByteIndex] == 0x60)
            {
                globalByteIndex += 2;
                _data = data.Skip(globalByteIndex).Take(data.Length - globalByteIndex - 3).ToList();
                globalByteIndex = data.Length;
            }
            else
            {
                short blockLength = BitConverter.ToInt16(new byte[] { data[globalByteIndex], data[globalByteIndex + 1] });
                globalByteIndex += 2;

                _data = data.Skip(globalByteIndex).Take(blockLength).ToList();
                _data.AddRange(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 });
                globalByteIndex += blockLength;
            }

            b = 0;
            e = 0x0C;
            g = 0;
            Lxx_020664A0();
        }
        #endregion

        #region Block_Decompression_Subroutine
        public void Lxx_020664A0()
        {
            b >>= 2;                    // mov      r2,r2,lsr 2h
            if ((b & 0x100) == 0)       // tst      r2,100h
            {
                l = _data[g++];         // ldrbeq   r12,[r7],1h
                b = l | 0xFF00;         // orreq    r2,r12,0FF00h
            }
            c = e & (b << 2);           // and      r3,r5,r2,lsl 2h
            switch (c)                  // ldr      r15,[r6,r3]
            {
                case 0x0:
                    SpriteTileDataPrep();
                    break;

                case 0x4:
                    Lxx_020664C8();
                    break;

                case 0x8:
                    Lxx_020664D4();
                    break;

                case 0xC:
                    Lxx_02066504();
                    break;

                default:
                    throw new IndexOutOfRangeException($"Unexpected index {c} encountered");
            }
        }

        public void Lxx_020664C8()
        {
            l = _data[g++];             // ldrb     r12,[r7],1h
            _pixels.Add((byte)l);       // strb     r12,[r8],1h
            Lxx_020664A0();             // b        Lxx_20664A0h;
        }

        public void Lxx_020664D4()
        {
            c = _data[g++];             // ldrb     r3,[r7],1h
            d = _data[g++];             // ldrb     r4,[r8],1h
            l = d & 0xF0;               // and      r12,r4,0F0h
            c |= (l << 4);              // orr      r3,r3,r12,lsl 4h
            l = d - l;                  // sub      r12,r4,r12
            d = l + 2;                  // add      r4,r12,2h
            c = (_pixels.Count) - c;    // sub      r3,r8,r3        // h is current index
            Lxx_020664F0();
        }

        public void Lxx_020664F0()
        {
            l = _pixels[c++];           // ldrb     r12,[r3],1h
            _pixels.Add((byte)l);       // strb     r12,[r8],1h
            d--;
            if (d != 0)                 // subs     r4,r4,1h
            {
                Lxx_020664F0();         // bne      Lxx_20664F0h
            }
            else
            {
                Lxx_020664A0();         // b        Lxx_20664A0h
            }
        }

        public void Lxx_02066504()
        {
            c = _data[g++];             // ldrb     r3,[r7],1h
            l = _data[g++];             // ldrb     r12,[r7],1h
            c += 2;                     // add      r3,r3,2h
            Lxx_02066510();
        }

        public void Lxx_02066510()
        {
            _pixels.Add((byte)l);       // strb     r12,[r8],1h
            c--;
            if (c != 0)                 // subs     r3,r3,1h
            {
                Lxx_02066510();         // bne      Lxx_2066510h
            }
            else
            {
                Lxx_020664A0();         // b        Lxx_20664A0h
            }
        }
        #endregion
        #endregion
    }
}
