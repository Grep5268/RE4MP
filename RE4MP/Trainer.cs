using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Memory;
using System.Globalization;
using System.Threading;

namespace RE4MP
{
    public class Trainer
    {
        private const int LOCAL_POS_BYTE_COUNT = 20;

        private const int ENEMY_POS_BYTE_COUNT = 20;
        private const int ENEMY_HP_BYTE_COUNT = 2;

        public Mem MemLib = new Mem();

        private Dictionary<byte[], byte[]> remoteLocalEnemyPointerMap = new Dictionary<byte[], byte[]>();
        private Dictionary<byte[], byte[]> remoteEnemyPointerValueMap = new Dictionary<byte[], byte[]>(); //value is initial value

        private List<byte[]> localEnemyPointers = new List<byte[]>();

        public int timeSinceUpdate = 0;

        private bool differentAreas = true;

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

        public byte[] GET_LOCAL_AREA()
        {
            return ReadMemory(MemoryLocations.LOCAL_AREA, 2);
        }

        private byte[] prevAllyPos = null;
        private Timer posAllyTimer = null;

        public void HANDLE_DIFFERENT_AREAS(byte[] area)
        {
            if(area != null && Utils.ConvertByteArrayToInt(area) != Utils.ConvertByteArrayToInt(this.GET_LOCAL_AREA()))
            {
                prevAllyPos = null;

                if (posAllyTimer != null)
                {
                    posAllyTimer.Dispose();
                }

                posAllyTimer = null;

                remoteLocalEnemyPointerMap = new Dictionary<byte[], byte[]>();
                remoteEnemyPointerValueMap = new Dictionary<byte[], byte[]>();
                localEnemyPointers = new List<byte[]>();

                differentAreas = true;
            }
            else
            {
                differentAreas = false;
            }
        }

        public void WRITE_POS_ALLY(byte[] data)
        {
            if (differentAreas) return;

            if(prevAllyPos != null)
            {
                if (posAllyTimer != null)
                {
                    posAllyTimer.Dispose();
                }

                var prevPosCopy = new byte[LOCAL_POS_BYTE_COUNT];
                prevAllyPos.CopyTo(prevPosCopy, 0);

                //var interval = timeSinceUpdate / 1000.0;
                var interval = 15;
                var count = 0;

                posAllyTimer = new Timer((state) =>
                {
                    count++;
                    var newData = new byte[LOCAL_POS_BYTE_COUNT];
                    data.CopyTo(newData, 0);

                    //x
                    var newX = Utils.ConvertByteArrayToInt(new byte[] { newData[0], newData[1], newData[2], newData[3] });
                    var oldX = Utils.ConvertByteArrayToInt(new byte[] { prevPosCopy[0], prevPosCopy[1], prevPosCopy[2], prevPosCopy[3] });

                    var xDiff = Utils.ConvertIntToByteArrayToInt(oldX + ((newX - oldX) * count / 5));

                    newData[0] = xDiff[0];
                    newData[1] = xDiff[1];
                    newData[2] = xDiff[2];
                    newData[3] = xDiff[3];

                    //z
                    var newZ = Utils.ConvertByteArrayToInt(new byte[] { newData[8], newData[9], newData[10], newData[11] });
                    var oldZ = Utils.ConvertByteArrayToInt(new byte[] { prevPosCopy[8], prevPosCopy[9], prevPosCopy[10], prevPosCopy[11] });

                    var zDiff = Utils.ConvertIntToByteArrayToInt(oldZ + ((newZ - oldZ) * count / 5));

                    newData[8] = zDiff[0];
                    newData[9] = zDiff[1];
                    newData[10] = zDiff[2];
                    newData[11] = zDiff[3];

                    WriteMemory(MemoryLocations.ALLY_POS, newData);
                }
                , data, TimeSpan.FromMilliseconds(interval), TimeSpan.FromMilliseconds(60));
            }

            prevAllyPos = data;
        }

        public void WRITE_HP_ALLY(byte[] data)
        {
            WriteMemory(MemoryLocations.ALLY_HP, data);
        }

        public byte[] GET_LOCAL_POS()
        {
            return ReadMemory(MemoryLocations.LOCAL_POS, LOCAL_POS_BYTE_COUNT);
        }

        public byte[] GET_LOCAL_HP()
        {
            return ReadMemory(MemoryLocations.LOCAL_HP, 2);
        }

        public byte[] GET_POS_ENEMY_VALUE()
        {
            return ReadMemory(MemoryLocations.ENEMY_POS, ENEMY_POS_BYTE_COUNT);
        }

        public byte[] GET_POS_ENEMY_POINTER()
        {
            var addr = ReadMemory(MemoryLocations.ENEMY_POS_POINTER, 4);

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
            if (differentAreas) return;

            if (!remoteLocalEnemyPointerMap.Any(x => x.Key.SequenceEqual(serverAddr)))
            {
                var clientPointer = GET_POS_ENEMY_POINTER();
                if (clientPointer == null || remoteLocalEnemyPointerMap.Any(x => x.Value.SequenceEqual(clientPointer)) || serverAddr == null || serverAddr.All(x => x.Equals(0)) || clientPointer.All(x => x.Equals(0)))
                {
                    return;
                }

                if (!remoteEnemyPointerValueMap.Any(x => x.Key.SequenceEqual(serverAddr)))
                {
                    remoteEnemyPointerValueMap.Add(serverAddr, data);
                }

                var pointerValue = ReadMemory(Utils.ByteArrayToString(clientPointer, 0x94), ENEMY_POS_BYTE_COUNT);

                var mappedServerAddr = remoteEnemyPointerValueMap.FirstOrDefault(x => x.Key.Take(2).ToArray().SequenceEqual(clientPointer.Take(2).ToArray()));
                if (!mappedServerAddr.Equals(default(KeyValuePair<byte[], byte[]>)))
                {
                    remoteLocalEnemyPointerMap.Add(mappedServerAddr.Key, clientPointer);
                }
            }
        }


        private Dictionary<byte[], byte[]> enemyPrevPositionMap = new Dictionary<byte[], byte[]>();
        private Dictionary<byte[], Timer> enemyPositionTimerMap = new Dictionary<byte[], Timer>();

        public void WRITE_ENEMY_POSITIONS_CLIENT(Dictionary<byte[], byte[]> positionMap)
        {
            if (differentAreas) return;

            foreach (var pos in positionMap)
            {
                if(remoteLocalEnemyPointerMap.Any(x => x.Key.SequenceEqual(pos.Key)))
                {
                    var localPointer = remoteLocalEnemyPointerMap.FirstOrDefault(x => x.Key.SequenceEqual(pos.Key)).Value;

                    if (enemyPrevPositionMap.Any(x => x.Key.SequenceEqual(localPointer)))
                    {
                        if (enemyPositionTimerMap.Any(x => x.Key.SequenceEqual(localPointer)))
                        {
                            enemyPositionTimerMap.FirstOrDefault(x => x.Key.SequenceEqual(localPointer)).Value.Dispose();
                            enemyPositionTimerMap = enemyPositionTimerMap.Where(x => !x.Key.SequenceEqual(localPointer))
                                 .ToDictionary(pair => pair.Key,
                                               pair => pair.Value);
                        }

                        var prevEnemyPos = enemyPrevPositionMap.FirstOrDefault(x => x.Key.SequenceEqual(localPointer)).Value;

                        var prevPosCopy = new byte[ENEMY_POS_BYTE_COUNT];
                        prevEnemyPos.CopyTo(prevPosCopy, 0);

                        //var interval = timeSinceUpdate / 1000.0;
                        var interval = 10;
                        var count = 0;

                        enemyPositionTimerMap.Add(localPointer, new Timer((state) =>
                        {
                            count++;
                            var newData = new byte[ENEMY_POS_BYTE_COUNT];
                            pos.Value.CopyTo(newData, 0);

                            //x
                            var newX = Utils.ConvertByteArrayToInt(new byte[] { newData[0], newData[1], newData[2], newData[3] });
                            var oldX = Utils.ConvertByteArrayToInt(new byte[] { prevPosCopy[0], prevPosCopy[1], prevPosCopy[2], prevPosCopy[3] });

                            var xDiff = Utils.ConvertIntToByteArrayToInt(oldX + ((newX - oldX) * count / 5));

                            newData[0] = xDiff[0];
                            newData[1] = xDiff[1];
                            newData[2] = xDiff[2];
                            newData[3] = xDiff[3];


                            //z
                            var newZ = Utils.ConvertByteArrayToInt(new byte[] { newData[8], newData[9], newData[10], newData[11] });
                            var oldZ = Utils.ConvertByteArrayToInt(new byte[] { prevPosCopy[8], prevPosCopy[9], prevPosCopy[10], prevPosCopy[11] });

                            var zDiff = Utils.ConvertIntToByteArrayToInt(oldZ + ((newZ - oldZ) * count / 5));

                            newData[8] = zDiff[0];
                            newData[9] = zDiff[1];
                            newData[10] = zDiff[2];
                            newData[11] = zDiff[3];

                            WriteMemory(Utils.ByteArrayToString(localPointer, 0x94), newData);
                        }
                        , pos.Value, TimeSpan.FromMilliseconds(interval), TimeSpan.FromMilliseconds(50)));
                    }

                    enemyPrevPositionMap = enemyPrevPositionMap.Where(x => !x.Key.SequenceEqual(localPointer))
                                 .ToDictionary(pair => pair.Key,
                                               pair => pair.Value);

                    enemyPrevPositionMap.Add(localPointer, pos.Value);
                }
            }
        }

        public void WRITE_ENEMY_HP_CLIENT(Dictionary<byte[], byte[]> hpMap)
        {
            if (differentAreas) return;

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
            if (differentAreas) return;

            foreach (var hp in hpMap)
            {
                if (localEnemyPointers.Any(x => x.SequenceEqual(hp.Key)) 
                    && Utils.ConvertByteArrayToInt(ReadMemory(Utils.ByteArrayToString(hp.Key, 0x112c), ENEMY_HP_BYTE_COUNT)) > Utils.ConvertByteArrayToInt(hp.Value))
                {
                    WriteMemory(Utils.ByteArrayToString(hp.Key, 0x112c), hp.Value);
                }
            }
        }
    }
}
