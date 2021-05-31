using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamtaroNNQKnJ_ScriptEditor
{
    public class GraphicsDriver
    {
        private int currentByte = 0; // r0
        private int a; // r1, current pixel address
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
        private byte[] pixels { get; set; }

        public byte[] GetTilePixels(byte[] compressedData)
        {
            data = compressedData;
            currentByte = 0;
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

        #region Tile_ASM
        private void Lxx_020747D8()
        {
            h = BitConverter.ToInt32(data.Take(4).ToArray());   // ldr      r8,[r0],4h
            currentByte += 4;
            j = h >> 8;                                         // mov      r10,r8,lsr 8h
            pixels = new byte[j];
            b = 0;                                              // mov      r2,0h
            Lxx_020747E4();                                     // continuing naturally
        }

        private void Lxx_020747E4()
        {
            if (j <= 0)                                         // cmp      r10,0h
            {
                Lxx_020748A0();                                 // ble      Lxx_20748A0h
            }
            else
            {
                f = data[currentByte];
                currentByte++;                                  // ldrb     r6,[r0],1h
                g = 8;                                          // mov      r7,8h
                Lxx_020747F4();                                 // continuing naturally
            }
        }

        private void Lxx_020747F4()
        {
            g -= 1;                                             // subs     r7,r7,1h
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
                    i = data[currentByte];
                    currentByte++;                              // ldrb     r9,[r0],1h
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
            i = data[currentByte];                              // ldrb     r9,[r0]
            h = 3;                                              // mov      r8,3h
            e = h + (i >> 4);                                   // add      r5,r8,r9,asr 4h
            i = data[currentByte];
            currentByte++;                                      // ldrb     r9,[r0],1h
            h = i & 0x0F;                                       // and      r8,r9,0Fh
            d = h << 8;                                         // mov      r4,r8,lsl 8h
            i = data[currentByte];
            currentByte++;                                      // ldrb     r9,[r0],1h
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

        private void Lxx_020748A0()
        {

        }
        #endregion
    }
}
