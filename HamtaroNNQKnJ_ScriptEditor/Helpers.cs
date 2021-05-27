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
    }
}
