using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RE4MP
{
    public static class MemoryLocations
    {
        public static string ALLY_HP = "base+85BE98";
        public static string ALLY_POS = "base+8537E0,94";

        public static string ENEMY_POS = "base+00863D08,94";
        public static string ENEMY_POS_POINTER = "base+00863D08";

        public static string LOCAL_AREA = "base+85BE90";
        public static string LOCAL_HP = "base+85BE94";
        public static string LOCAL_POS = "base+007FCB08,94";
    }
}
