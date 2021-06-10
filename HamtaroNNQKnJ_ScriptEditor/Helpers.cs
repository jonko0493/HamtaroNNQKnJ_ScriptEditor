using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HamtaroNNQKnJ_ScriptEditor
{
    public static class Helpers
    {

        public static bool IsLessThanNextPointer(List<int> pointers, int i, int messageIndex, byte[] data)
        {
            return i < NextPointer(pointers, messageIndex, data);
        }

        public static int NextPointer(List<int> pointers, int index, byte[] data)
        {
            if (index < pointers.Count - 1)
            {
                return pointers[index + 1];
            }
            else
            {
                return data.Length;
            }
        }

        public static BitmapImage GetBitmapImageFromBitmap(Bitmap bitmap)
        {
            BitmapImage bitmapImage = new BitmapImage();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, ImageFormat.Png);
                memoryStream.Position = 0;
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
            }
            return bitmapImage;
        }

        public static int MatchLength(this IEnumerable<byte> array, IEnumerable<byte> other)
        {
            int matchLength = 0;
            for (int i = 1; i <= array.Count() && i <= other.Count(); i++)
            {
                if (array.Take(i).SequenceEqual(other.Take(i)))
                {
                    matchLength = i;
                }
                else
                {
                    break;
                }
            }
            return matchLength;
        }
    }
}
