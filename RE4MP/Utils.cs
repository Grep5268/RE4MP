using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace RE4MP
{
    public class Utils
    {
        // Convert an object to a byte array
        public static byte[] ObjectToByteArray(Object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        // Convert a byte array to an Object
        public static T Deserialize<T>(byte[] param)
        {
            using (MemoryStream ms = new MemoryStream(param))
            {
                IFormatter br = new BinaryFormatter();
                return (T)br.Deserialize(ms);
            }
        }

        public static string ByteArrayToString(byte[] ba, int offset = 0)
        {
            var newAddr = ConvertByteArrayToInt(ba) + offset;

            var addr = BitConverter.GetBytes(newAddr).Reverse().ToArray();
            StringBuilder hex = new StringBuilder(addr.Length * 2);
            foreach (byte b in addr)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public static int ConvertByteArrayToInt(byte[] b)
        {
            if(b.Length == 2)
            {
                return BitConverter.ToInt16(b, 0);
            }

            return BitConverter.ToInt32(b, 0);
        }

        public static byte[] ConvertIntToByteArrayToInt(int i)
        {
            byte[] intBytes = BitConverter.GetBytes(i);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(intBytes);
            return intBytes;
        }
    }
}
