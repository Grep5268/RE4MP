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

        private Dictionary<byte[], byte[]> serverClientEnemyPointerMap = new Dictionary<byte[], byte[]>();

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
            MemLib.writeMemory(address, "int", data.ToString());
        }

        private void WriteMemory(string address, byte[] data)
        {
            MemLib.writeBytes(address, data ?? new byte[0]);
        }

        private void WriteNop(string address, int length)
        {
            var nopData = new byte[length];

            for(var i = 0; i < length; i++)
            {
                nopData[i] = 0x90;
            }

            MemLib.writeBytes(address, nopData);
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
            return ReadMemory("base+007FDB08,96", 18);
        }

        public byte[] GET_POS_ENEMY_VALUE()
        {
            return ReadMemory("base+00867594,94", 20);
        }

        public byte[] GET_POS_ENEMY_POINTER()
        {
            return ReadMemory("base+00867594", 4);
        }

        public void WRITE_POS_ENEMY(byte[] serverAddr, byte[] data)
        {
            FREEZE_ENEMY_POINTERS();

            if (!serverClientEnemyPointerMap.Any(x => x.Key.SequenceEqual(serverAddr)))
            {
                var clientPointer = GET_POS_ENEMY_POINTER();
                if (serverClientEnemyPointerMap.Any(x => x.Value.SequenceEqual(clientPointer)))
                {
                    UNFREEZE_ENEMY_POINTERS();
                    return;
                }

                serverClientEnemyPointerMap.Add(serverAddr, clientPointer);
            }

            WriteMemory("base+00867594", serverClientEnemyPointerMap.FirstOrDefault(x => x.Key.SequenceEqual(serverAddr)).Value);
            WriteMemory("base+00867594,94", data);
            UNFREEZE_ENEMY_POINTERS();
        }

        public void FREEZE_ENEMY_POINTERS()
        {
            WriteNop("base+2B19F2", 6);
        }

        public void UNFREEZE_ENEMY_POINTERS()
        {
            WriteMemory("base+2B19F2", new byte[] { 0x89, 0x35, 0x94, 0x75, 0xA7, 0x01 });
            
        }
    }
}
