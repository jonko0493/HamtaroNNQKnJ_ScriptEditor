using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamtaroNNQKnJ_ScriptEditor
{
    public class Helpers
    {

        public static bool IsLessThanNextPointer(List<int> pointers, int i, int messageIndex, byte[] data)
        {
            if (messageIndex < pointers.Count - 1)
            {
                return i < pointers[messageIndex + 1];
            }
            else
            {
                return i < data.Length;
            }
        }
    }
}
