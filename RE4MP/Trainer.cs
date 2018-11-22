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
        private const int ENEMY_POS_BYTE_COUNT = 20;
        private const int ENEMY_HP_BYTE_COUNT = 2;

        public Mem MemLib = new Mem();

        private Dictionary<byte[], byte[]> remoteLocalEnemyPointerMap = new Dictionary<byte[], byte[]>();
        private Dictionary<byte[], byte[]> remoteEnemyPointerValueMap = new Dictionary<byte[], byte[]>(); //value is initial value

        private List<byte[]> localEnemyPointers = new List<byte[]>();

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

        public void WRITE_HP_ALLY(byte[] data)
        {
            WriteMemory("base+85F718", data);
        }

        public byte[] GET_POS_ALLY()
        {
            return ReadMemory("base+007FDB08,96", 18);
        }

        public byte[] GET_HP_ALLY()
        {
            return ReadMemory("base+85F714", 2);
        }

        public byte[] GET_POS_ENEMY_VALUE()
        {
            return ReadMemory("base+00867594,94", ENEMY_POS_BYTE_COUNT);
        }

        public byte[] GET_POS_ENEMY_POINTER()
        {
            var addr = ReadMemory("base+00867594", 4);

            if(addr == null || addr.All(x => x == 0))
            {
                return null;
            }

            if (!localEnemyPointers.Any(x => x.SequenceEqual(addr)))
            {
                localEnemyPointers.Add(addr);
            }

            return addr;
        }

        public Dictionary<byte[], byte[]> GET_POS_ENEMY_DATA()
        {
            var data = new Dictionary<byte[], byte[]>();

            foreach(var addr in localEnemyPointers)
            {
                data.Add(addr, ReadMemory(Utils.ByteArrayToString(addr, 0x94), ENEMY_POS_BYTE_COUNT));
            }

            return data;
        }

        public Dictionary<byte[], byte[]> GET_HP_ENEMY_DATA_FOR_CLIENT()
        {
            var data = new Dictionary<byte[], byte[]>();

            foreach (var addr in localEnemyPointers)
            {
                data.Add(addr, ReadMemory(Utils.ByteArrayToString(addr, 0x112c), ENEMY_HP_BYTE_COUNT));
            }

            return data;
        }

        public Dictionary<byte[], byte[]> GET_HP_ENEMY_DATA_FOR_SERVER()
        {
            var data = new Dictionary<byte[], byte[]>();

            foreach (var addr in remoteLocalEnemyPointerMap)
            {
                data.Add(addr.Key, ReadMemory(Utils.ByteArrayToString(addr.Value, 0x112c), ENEMY_HP_BYTE_COUNT));
            }

            return data;
        }

        public void MAP_ENEMY_POINTER(byte[] serverAddr, byte[] data)
        {
            if (!remoteLocalEnemyPointerMap.Any(x => x.Key.SequenceEqual(serverAddr)))
            {
                var clientPointer = GET_POS_ENEMY_POINTER();
                if (remoteLocalEnemyPointerMap.Any(x => x.Value.SequenceEqual(clientPointer)) || serverAddr.All(x => x.Equals(0)) || clientPointer.All(x => x.Equals(0)))
                {
                    return;
                }

                if (!remoteEnemyPointerValueMap.Any(x => x.Key.SequenceEqual(serverAddr)))
                {
                    remoteEnemyPointerValueMap.Add(serverAddr, data);
                }

                var pointerValue = ReadMemory(Utils.ByteArrayToString(clientPointer, 0x94), 18);

                var mappedServerAddr = remoteEnemyPointerValueMap.FirstOrDefault(x => Math.Abs(pointerValue[3] - x.Value[3]) <= 4 && Math.Abs(pointerValue[10] - x.Value[10]) <= 3);
                if (!mappedServerAddr.Equals(default(KeyValuePair<byte[], byte[]>)))
                {
                    remoteLocalEnemyPointerMap.Add(mappedServerAddr.Key, clientPointer);
                }
            }
        }

        public void WRITE_ENEMY_POSITIONS_CLIENT(Dictionary<byte[], byte[]> positionMap)
        {
            foreach(var pos in positionMap)
            {
                if(remoteLocalEnemyPointerMap.Any(x => x.Key.SequenceEqual(pos.Key)))
                {
                    WriteMemory(Utils.ByteArrayToString(remoteLocalEnemyPointerMap.FirstOrDefault(x => x.Key.SequenceEqual(pos.Key)).Value, 0x94), pos.Value);
                }
            }
        }

        public void WRITE_ENEMY_HP_CLIENT(Dictionary<byte[], byte[]> hpMap)
        {
            foreach (var hp in hpMap)
            {
                if (remoteLocalEnemyPointerMap.Any(x => x.Key.SequenceEqual(hp.Key))
                    && Utils.ConvertByteArrayToInt(ReadMemory(Utils.ByteArrayToString(remoteLocalEnemyPointerMap.FirstOrDefault(x => x.Key.SequenceEqual(hp.Key)).Value, 0x112c), ENEMY_HP_BYTE_COUNT)) 
                        > Utils.ConvertByteArrayToInt(hp.Value))
                {
                    WriteMemory(Utils.ByteArrayToString(remoteLocalEnemyPointerMap.FirstOrDefault(x => x.Key.SequenceEqual(hp.Key)).Value, 0x112c), hp.Value);
                }
            }
        }

        public void WRITE_ENEMY_HP_SERVER(Dictionary<byte[], byte[]> hpMap)
        {
            foreach (var hp in hpMap)
            {
                if (localEnemyPointers.Any(x => x.SequenceEqual(hp.Key)) 
                    && Utils.ConvertByteArrayToInt(ReadMemory(Utils.ByteArrayToString(hp.Key, 0x112c), ENEMY_HP_BYTE_COUNT)) > Utils.ConvertByteArrayToInt(hp.Value))
                {
                    WriteMemory(Utils.ByteArrayToString(hp.Key, 0x112c), hp.Value);
                }
            }
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
