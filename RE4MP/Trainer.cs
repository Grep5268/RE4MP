using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Memory;
using System.Globalization;

namespace RE4MP
{
    public class Trainer
    {
        public Mem MemLib = new Mem();

        public void Initialize()
        {
            MemLib.OpenProcess("bio4");
        }

        public void processCommand(string command, byte[] data)
        {
            switch(command)
            {
                case "write_pos_ally":
                    WRITE_POS_ALLY(data);
                    break;
                default:
                    break;
            }
        }

        private void WriteMemory(string address, int data)
        {
            var x = ReadMemory(address, 0);
            MemLib.writeMemory(address, "int", data.ToString());
        }

        private void WriteMemory(string address, byte[] data)
        {
            MemLib.writeBytes(address, data);
        }

        public byte[] ReadMemory(string address, int bytes)
        {
            return MemLib.readBytes(address, bytes);
        }

        public void WRITE_POS_ALLY(byte[] data)
        {
            //new byte[] { 55, 197, 217, 75, 230, 197, 133, 125, 177, 70 }
            WriteMemory("base+857060,96", data);
        }

        public byte[] GET_POS_ALLY()
        {
            return ReadMemory("base+857060,96", 10);
        }

        
    }
}
