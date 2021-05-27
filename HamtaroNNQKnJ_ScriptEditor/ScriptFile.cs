using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HamtaroNNQKnJ_ScriptEditor
{
    public class ScriptFile
    {
        public List<int> Pointers { get; set; } = new List<int>();
        public List<Message> Messages { get; set; } = new List<Message>();

        public static ScriptFile ParseFromFile(string file)
        {
            byte[] data = File.ReadAllBytes(file);
            return ParseFromData(data);
        }

        public static ScriptFile ParseFromData(byte[] data)
        {
            int firstPointer = data.Length;
            var scriptFile = new ScriptFile();

            for (int i = 0; i < data.Length && i < firstPointer; i += 4)
            {
                if (i < firstPointer)
                {
                    int pointer = BitConverter.ToInt32(new byte[] { data[i], data[i + 1], data[i + 2], data[i + 3] });
                    if (i == 0)
                    {
                        firstPointer = pointer;
                    }
                    scriptFile.Pointers.Add(pointer);
                }
            }

            for (int messageIndex = 0; messageIndex < scriptFile.Pointers.Count; messageIndex++)
            {
                scriptFile.Messages.Add(new Message { Text = "" });
                for (int i = scriptFile.Pointers[messageIndex]; Helpers.IsLessThanNextPointer(scriptFile.Pointers, i, messageIndex, data);)
                {
                    if (i == scriptFile.Pointers[messageIndex])
                    {
                        scriptFile.Messages[messageIndex].UnknownBytes = BitConverter.ToUInt16(new byte[] { data[i], data[i + 1] });
                        i += 2;
                    }
                    else if (data[i] == 0x00)
                    {
                        scriptFile.Messages[messageIndex].Text += "<00>";
                        i += 1;
                    }
                    else if (data[i] == 0xFF) // opcode
                    {
                        (string op, int bytes) = GetFFOp(new byte[] { data[i + 1], data[i + 2] },
                            new FileFormatException($"Unknown 0xFF opcode 0x{data[i + 1]:X2} encountered at position 0x{i + 1:X2}"));

                        scriptFile.Messages[messageIndex].Text += op;
                        i += bytes;
                    }
                    else if (data[i] == 0xFE)
                    {
                        scriptFile.Messages[messageIndex].Text += $"{FEByteToSpecialCharMap[data[i + 1]]}";
                        i += 2;
                    }
                    else
                    {
                        scriptFile.Messages[messageIndex].Text += $"{ByteToCharMap[data[i]]}";
                        i++;
                    }
                }
            }

            return scriptFile;
        }

        public static (string op, int bytes) GetFFOp(byte[] nextTwoBytes, Exception e)
        {
            if (nextTwoBytes[0] == 0x00)
            {
                // Inserts newline character
                return ("\n", 2);
            }
            else if (nextTwoBytes[0] == 0x01 && nextTwoBytes[1] == 0x00)
            {
                // Unknown what this does -- seems like line end
                return ("<0x01>", 3);
            }
            else if (nextTwoBytes[0] == 0x03)
            {
                // Unknown what this does -- seems like line end
                return ("<0x03>", 2);
            }
            else if (nextTwoBytes[0] == 0x0A && nextTwoBytes[1] == 0x00)
            {
                // Unknown what this does -- seems like line end
                return ("<0x0A>", 3);
            }
            else if (nextTwoBytes[0] == 0x0B && nextTwoBytes[1] == 0x01)
            {
                // Unknown what this does -- seems a bit like line start?
                return ("<0x0B>", 3);
            }
            else if (nextTwoBytes[0] == 0x0C)
            {
                // Wait command
                // second byte is wait duration
                return ($"<0x0C{nextTwoBytes[1]:X2}>", 3);
            }
            else if (nextTwoBytes[0] == 0x0E)
            {
                // Unknown what this does -- seems a bit like line start?
                return ("<0x0E>", 2);
            }
            else if (nextTwoBytes[0] == 0x0F && nextTwoBytes[1] == 0x01)
            {
                // Unknown what this does -- seems a bit like line start?
                return ("<0x0F01>", 3);
            }
            else if (nextTwoBytes[0] == 0x0F && nextTwoBytes[1] == 0x04)
            {
                // Unknown what this does -- seems a bit like line start?
                return ("<0x0F04>", 3);
            }
            else if (nextTwoBytes[0] == 0x0F && nextTwoBytes[1] == 0x05)
            {
                // Unknown what this does -- seems a bit like line start?
                return ("<0x0F05>", 3);
            }
            else if (nextTwoBytes[0] == 0x10 && nextTwoBytes[1] == 0x03)
            {
                // Unknown what this does
                return ("<0x1003>", 3);
            }
            else if (nextTwoBytes[0] == 0x10 && nextTwoBytes[1] == 0x04)
            {
                // Unknown what this does
                return ("<0x1004>", 3);
            }
            else if (nextTwoBytes[0] == 0x11 && nextTwoBytes[1] == 0x00)
            {
                // Unknown what this does
                return ("<0x1100>", 3);
            }
            else if (nextTwoBytes[0] == 0x11 && nextTwoBytes[1] == 0x01)
            {
                // Unknown what this does
                return ("<0x1101>", 3);
            }
            else if (nextTwoBytes[0] == 0x20)
            {
                // Makes text black
                return ("<black>", 2);
            }
            else if (nextTwoBytes[0] == 0x23)
            {
                // Makes text red
                return ("<red>", 2);
            }
            else if (nextTwoBytes[0] == 0x26)
            {
                // Makes text blue
                return ("<blue>", 2);
            }
            else if (nextTwoBytes[0] == 0x33)
            {
                // Makes text very big
                return ("<big>", 2);
            }
            else if (nextTwoBytes[0] == 0x35)
            {
                // Inserts tab character
                return ("\t", 2);
            }
            else if (nextTwoBytes[0] == 0x36)
            {
                // Inserts tab character
                return ("<longtab>", 2);
            }
            else
            {
                throw e;
            }
        }

        public byte[] GetBytes()
        {
            RecalculatePointers();

            List<byte> data = new List<byte>();

            foreach (int pointer in Pointers)
            {
                data.AddRange(BitConverter.GetBytes(pointer));
            }
            foreach (Message message in Messages)
            {
                data.AddRange(message.GetBytes());
            }

            return data.ToArray();
        }

        public void WriteToFile(string file)
        {
            File.WriteAllBytes(file, GetBytes());
        }

        public void RecalculatePointers()
        {
            List<int> pointers = new List<int>();

            pointers.Add(Messages.Count * 4); // Adds the number of 32-bit integer bytes (4 * # of pointers) as first pointer
            for (int i = 0; i < Messages.Count - 1; i++)
            {
                pointers.Add(pointers[i] + Messages[i].GetBytes().Length);
            }

            Pointers = pointers;
        }

        public static Dictionary<byte, string> ByteToCharMap = new Dictionary<byte, string>
        {
            #region ByteToCharMap
            { 0x01, "ノ" },
            { 0x02, "ハ" },
            { 0x03, "バ" },
            { 0x04, "パ" },
            { 0x05, "ヒ" },
            { 0x06, "ビ" },
            { 0x07, "ピ" },
            { 0x08, "フ" },
            { 0x09, "ブ" },
            { 0x0A, "プ" },
            { 0x0B, "ヘ" },
            { 0x0C, "ベ" },
            { 0x0D, "ペ" },
            { 0x0E, "ホ" },
            { 0x0F, "ボ" },
            { 0x10, "ポ" },
            { 0x11, "マ" },
            { 0x12, "ミ" },
            { 0x13, "ム" },
            { 0x14, "メ" },
            { 0x15, "モ" },
            { 0x16, "ャ" },
            { 0x17, "ヤ" },
            { 0x18, "ュ" },
            { 0x19, "ユ" },
            { 0x1A, "ョ" },
            { 0x1B, "ヨ" },
            { 0x1C, "ラ" },
            { 0x1D, "リ" },
            { 0x1E, "ル" },
            { 0x1F, "レ" },
            { 0x20, " " },
            { 0x21, "!" },
            { 0x22, "ヲ" },
            { 0x23, "ン" },
            { 0x24, "ヴ" },
            { 0x25, "%" },
            { 0x26, "&" },
            { 0x27, "\"" },
            { 0x28, "(" },
            { 0x29, ")" },
            { 0x2A, "・" },
            { 0x2B, "+" },
            { 0x2C, "," },
            { 0x2D, "-" },
            { 0x2E, "." },
            { 0x2F, "/" },
            { 0x30, "0" },
            { 0x31, "1" },
            { 0x32, "2" },
            { 0x33, "3" },
            { 0x34, "4" },
            { 0x35, "5" },
            { 0x36, "6" },
            { 0x37, "7" },
            { 0x38, "8" },
            { 0x39, "9" },
            { 0x3A, ":" },
            { 0x3B, ";" },
            { 0x3C, "。" },
            { 0x3D, "=" },
            { 0x3E, "、" },
            { 0x3F, "?" },
            { 0x40, "ー" },
            { 0x41, "A" },
            { 0x42, "B" },
            { 0x43, "C" },
            { 0x44, "D" },
            { 0x45, "E" },
            { 0x46, "F" },
            { 0x47, "G" },
            { 0x48, "H" },
            { 0x49, "I" },
            { 0x4A, "J" },
            { 0x4B, "K" },
            { 0x4C, "L" },
            { 0x4D, "M" },
            { 0x4E, "N" },
            { 0x4F, "O" },
            { 0x50, "P" },
            { 0x51, "Q" },
            { 0x52, "R" },
            { 0x53, "S" },
            { 0x54, "T" },
            { 0x55, "U" },
            { 0x56, "V" },
            { 0x57, "W" },
            { 0x58, "X" },
            { 0x59, "Y" },
            { 0x5A, "Z" },
            { 0x5B, "[" },
            { 0x5C, "￥" },
            { 0x5D, "]" },
            { 0x5E, "「" },
            { 0x5F, "」" },
            { 0x60, "~" },
            { 0x61, "a" },
            { 0x62, "b" },
            { 0x63, "c" },
            { 0x64, "d" },
            { 0x65, "e" },
            { 0x66, "f" },
            { 0x67, "g" },
            { 0x68, "h" },
            { 0x69, "i" },
            { 0x6A, "j" },
            { 0x6B, "k" },
            { 0x6C, "l" },
            { 0x6D, "m" },
            { 0x6E, "n" },
            { 0x6F, "o" },
            { 0x70, "p" },
            { 0x71, "q" },
            { 0x72, "r" },
            { 0x73, "s" },
            { 0x74, "t" },
            { 0x75, "u" },
            { 0x76, "v" },
            { 0x77, "w" },
            { 0x78, "x" },
            { 0x79, "y" },
            { 0x7A, "z" },
            { 0x7B, "ト" },
            { 0x7C, "ド" },
            { 0x7D, "ナ" },
            { 0x7E, "ニ" },
            { 0x7F, "ヌ" },
            { 0x80, "ネ" },
            { 0x81, "ぁ" },
            { 0x82, "あ" },
            { 0x83, "ぃ" },
            { 0x84, "い" },
            { 0x85, "ぅ" },
            { 0x86, "う" },
            { 0x87, "ぇ" },
            { 0x88, "え" },
            { 0x89, "ぉ" },
            { 0x8A, "お" },
            { 0x8B, "か" },
            { 0x8C, "が" },
            { 0x8D, "き" },
            { 0x8E, "ぎ" },
            { 0x8F, "く" },
            { 0x90, "ぐ" },
            { 0x91, "け" },
            { 0x92, "げ" },
            { 0x93, "こ" },
            { 0x94, "ご" },
            { 0x95, "さ" },
            { 0x96, "ざ" },
            { 0x97, "し" },
            { 0x98, "じ" },
            { 0x99, "す" },
            { 0x9A, "ず" },
            { 0x9B, "せ" },
            { 0x9C, "ぜ" },
            { 0x9D, "そ" },
            { 0x9E, "ぞ" },
            { 0x9F, "た" },
            { 0xA0, "だ" },
            { 0xA1, "ち" },
            { 0xA2, "ぢ" },
            { 0xA3, "っ" },
            { 0xA4, "つ" },
            { 0xA5, "づ" },
            { 0xA6, "て" },
            { 0xA7, "で" },
            { 0xA8, "と" },
            { 0xA9, "ど" },
            { 0xAA, "な" },
            { 0xAB, "に" },
            { 0xAC, "ぬ" },
            { 0xAD, "ね" },
            { 0xAE, "の" },
            { 0xAF, "は" },
            { 0xB0, "ば" },
            { 0xB1, "ぱ" },
            { 0xB2, "ひ" },
            { 0xB3, "び" },
            { 0xB4, "ぴ" },
            { 0xB5, "ふ" },
            { 0xB6, "ぶ" },
            { 0xB7, "ぷ" },
            { 0xB8, "へ" },
            { 0xB9, "べ" },
            { 0xBA, "ぺ" },
            { 0xBB, "ほ" },
            { 0xBC, "ぼ" },
            { 0xBD, "ぽ" },
            { 0xBE, "ま" },
            { 0xBF, "み" },
            { 0xC0, "む" },
            { 0xC1, "め" },
            { 0xC2, "も" },
            { 0xC3, "ゃ" },
            { 0xC4, "や" },
            { 0xC5, "ゅ" },
            { 0xC6, "ゆ" },
            { 0xC7, "ょ" },
            { 0xC8, "よ" },
            { 0xC9, "ら" },
            { 0xCA, "り" },
            { 0xCB, "る" },
            { 0xCC, "れ" },
            { 0xCD, "ろ" },
            { 0xCE, "…" },
            { 0xCF, "わ" },
            { 0xD0, "ロ" },
            { 0xD1, "ワ" },
            { 0xD2, "を" },
            { 0xD3, "ん" },
            { 0xD4, "ァ" },
            { 0xD5, "ア" },
            { 0xD6, "ィ" },
            { 0xD7, "イ" },
            { 0xD8, "ゥ" },
            { 0xD9, "ウ" },
            { 0xDA, "ェ" },
            { 0xDB, "エ" },
            { 0xDC, "ォ" },
            { 0xDD, "オ" },
            { 0xDE, "カ" },
            { 0xDF, "ガ" },
            { 0xE0, "キ" },
            { 0xE1, "ギ" },
            { 0xE2, "ク" },
            { 0xE3, "グ" },
            { 0xE4, "ケ" },
            { 0xE5, "ゲ" },
            { 0xE6, "コ" },
            { 0xE7, "ゴ" },
            { 0xE8, "サ" },
            { 0xE9, "ザ" },
            { 0xEA, "シ" },
            { 0xEB, "ジ" },
            { 0xEC, "ス" },
            { 0xED, "ズ" },
            { 0xEE, "セ" },
            { 0xEF, "ゼ" },
            { 0xF0, "ソ" },
            { 0xF1, "ゾ" },
            { 0xF2, "タ" },
            { 0xF3, "ダ" },
            { 0xF4, "チ" },
            { 0xF5, "ヂ" },
            { 0xF6, "ッ" },
            { 0xF7, "ツ" },
            { 0xF8, "ヅ" },
            { 0xF9, "テ" },
            { 0xFA, "デ" },
            { 0xFB, "<FB>" },
            { 0xFC, "<FC>" },
            { 0xFD, "<FD>" }
            #endregion
        };
        public static Dictionary<byte, string> FEByteToSpecialCharMap = new Dictionary<byte, string>
        {
            { 0x00, "<up>" },
            { 0x01, "<right>" },
            { 0x02, "<down>" },
            { 0x03, "<left>" },
            { 0x04, "<A>" },
            { 0x05, "<B>" },
            { 0x06, "<X>" },
            { 0x07, "<Y>" },
            { 0x08, "<R>" },
            { 0x09, "<L>" },
            { 0x0A, "<heart>" },
            { 0x0B, "<music>" },
            { 0x0C, "<star>" },
            { 0x0D, "太" },
            { 0x0E, "郎" },
            { 0x0F, "<circle>" },
            { 0x10, "<cross>" },
        };
    }

    public class Message
    {
        public ushort UnknownBytes { get; set; }
        public string Text { get; set; }

        public byte[] GetBytes()
        {
            var bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(UnknownBytes));

            for (int i = 0; i < Text.Length; i++)
            {
                if (Text[i] == '<')
                {
                    string op = string.Concat(Text.Substring(i).TakeWhile(c => c != '>'));
                    switch (op)
                    {
                        case "<00":
                            bytes.AddRange(new byte[] { 0x00 });
                            break;
                        case "<0x01":
                            bytes.AddRange(new byte[] { 0xFF, 0x01, 0x00 });
                            break;
                        case "<0x03":
                            bytes.AddRange(new byte[] { 0xFF, 0x03 });
                            break;
                        case "<0x0A":
                            bytes.AddRange(new byte[] { 0xFF, 0x0A, 0x00 });
                            break;
                        case "<0x0B":
                            bytes.AddRange(new byte[] { 0xFF, 0x0B, 0x01 });
                            break;
                        case "<0x0E":
                            bytes.AddRange(new byte[] { 0xFF, 0x0E });
                            break;
                        case "<0x0F01":
                            bytes.AddRange(new byte[] { 0xFF, 0x0F, 0x01 });
                            break;
                        case "<0x0F04":
                            bytes.AddRange(new byte[] { 0xFF, 0x0F, 0x04 });
                            break;
                        case "<0x0F05":
                            bytes.AddRange(new byte[] { 0xFF, 0x0F, 0x05 });
                            break;
                        case "<0x1003":
                            bytes.AddRange(new byte[] { 0xFF, 0x10, 0x03 });
                            break;
                        case "<0x1004":
                            bytes.AddRange(new byte[] { 0xFF, 0x10, 0x04 });
                            break;
                        case "<0x1100":
                            bytes.AddRange(new byte[] { 0xFF, 0x11, 0x00 });
                            break;
                        case "<0x1101":
                            bytes.AddRange(new byte[] { 0xFF, 0x11, 0x01 });
                            break;
                        case "<0xFB":
                            bytes.AddRange(new byte[] { 0xFB });
                            break;
                        case "<0xFC":
                            bytes.AddRange(new byte[] { 0xFC });
                            break;
                        case "<0xFD":
                            bytes.AddRange(new byte[] { 0xFD });
                            break;
                        case "<black":
                            bytes.AddRange(new byte[] { 0xFF, 0x20 });
                            break;
                        case "<red":
                            bytes.AddRange(new byte[] { 0xFF, 0x23 });
                            break;
                        case "<blue":
                            bytes.AddRange(new byte[] { 0xFF, 0x26 });
                            break;
                        case "<big":
                            bytes.AddRange(new byte[] { 0xFF, 0x33 });
                            break;
                        case "<longtab":
                            bytes.AddRange(new byte[] { 0xFF, 0x36 });
                            break;
                        case "<up":
                            bytes.AddRange(new byte[] { 0xFE, 0x00 });
                            break;
                        case "<right":
                            bytes.AddRange(new byte[] { 0xFE, 0x01 });
                            break;
                        case "<down":
                            bytes.AddRange(new byte[] { 0xFE, 0x02 });
                            break;
                        case "<left":
                            bytes.AddRange(new byte[] { 0xFE, 0x03 });
                            break;
                        case "<A":
                            bytes.AddRange(new byte[] { 0xFE, 0x04 });
                            break;
                        case "<B":
                            bytes.AddRange(new byte[] { 0xFE, 0x05 });
                            break;
                        case "<X":
                            bytes.AddRange(new byte[] { 0xFE, 0x06 });
                            break;
                        case "<Y":
                            bytes.AddRange(new byte[] { 0xFE, 0x07 });
                            break;
                        case "<R":
                            bytes.AddRange(new byte[] { 0xFE, 0x08 });
                            break;
                        case "<L":
                            bytes.AddRange(new byte[] { 0xFE, 0x09 });
                            break;
                        case "<heart":
                            bytes.AddRange(new byte[] { 0xFE, 0x0A });
                            break;
                        case "<music":
                            bytes.AddRange(new byte[] { 0xFE, 0x0B });
                            break;
                        case "<star":
                            bytes.AddRange(new byte[] { 0xFE, 0x0C });
                            break;
                        case "<circle":
                            bytes.AddRange(new byte[] { 0xFE, 0x0F });
                            break;
                        case "<cross":
                            bytes.AddRange(new byte[] { 0xFE, 0x10 });
                            break;
                        default:
                            // Regex checks
                            Match match0C = Regex.Match(op, @"<0C(\w{2})");
                            if (match0C.Success)
                            {
                                bytes.AddRange(new byte[] { 0xFF, 0x0C, byte.Parse(match0C.Groups[1].Value, NumberStyles.HexNumber) });
                            }
                            break;
                    }

                    i += op.Length;
                }
                else if (Text[i] == '\n')
                {
                    bytes.AddRange(new byte[] { 0xFF, 0x00 });
                }
                else if (Text[i] == '\t')
                {
                    bytes.AddRange(new byte[] { 0xFF, 0x35 });
                }
                else if (Text[i] == '太')
                {
                    bytes.AddRange(new byte[] { 0xFE, 0x0D });
                }
                else if (Text[i] == '郎')
                {
                    bytes.AddRange(new byte[] { 0xFE, 0x0E });
                }
                else if (ScriptFile.FEByteToSpecialCharMap.Values.Contains(Text[i].ToString()))
                {
                    bytes.Add(ScriptFile.FEByteToSpecialCharMap.First(c => c.Value == Text[i].ToString()).Key);
                }
                else
                {
                    bytes.Add(ScriptFile.ByteToCharMap.First(c => c.Value == Text[i].ToString()).Key);
                }
            }

            return bytes.ToArray();
        }

        public override string ToString()
        {
            return Text.Replace("\n", "\\n").Replace("\t", "\\t");
        }
    }
}
