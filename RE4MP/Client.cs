using AwesomeSockets.Domain.Sockets;
using AwesomeSockets.Sockets;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace RE4MP
{
    public class Client
    {
        public async Task StartClient(Trainer trainer)
        {
            //Request server IP and port number
            Console.WriteLine("Please enter the server IP in the format 192.168.0.1 and press return:");
            string ip = Console.ReadLine();

            Console.WriteLine("Please enter the server port and press return:");
            string port = Console.ReadLine();

            ISocket server = AweSock.TcpConnect(ip, int.Parse(port));

            var inBuf = AwesomeSockets.Buffers.Buffer.New(99999);
            var outBuf = AwesomeSockets.Buffers.Buffer.New(99999);

            this.SetupClientTrainer(trainer);

            while (true)
            {
                try
                {
                    //get data
                    var outputData = this.GetOutputData(trainer);

                    //write data to buffer
                    AwesomeSockets.Buffers.Buffer.ClearBuffer(outBuf);
                    AwesomeSockets.Buffers.Buffer.Add(outBuf, Utils.ObjectToByteArray(outputData));
                    AwesomeSockets.Buffers.Buffer.FinalizeBuffer(outBuf);

                    //send data
                    int bytesSent = AweSock.SendMessage(server, outBuf);
                    Console.WriteLine("sent data");

                    //get response
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();
                    
                    Tuple<int, EndPoint> received = AweSock.ReceiveMessage(server, inBuf);
                    stopWatch.Stop();

                    TimeSpan ts = stopWatch.Elapsed;
                    trainer.timeSinceUpdate = ts.Milliseconds;

                    AwesomeSockets.Buffers.Buffer.FinalizeBuffer(inBuf);

                    //parse response
                    var res = Utils.Deserialize<Dictionary<string, byte[]>>(AwesomeSockets.Buffers.Buffer.GetBuffer(inBuf));
                    AwesomeSockets.Buffers.Buffer.ClearBuffer(inBuf);

                    //act on response
                    //Console.WriteLine(string.Join(", ", res["pos_ally"]));
                    this.HandleInputData(res, trainer);

                    //refresh rate
                    Thread.Sleep(50);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);

                    AwesomeSockets.Buffers.Buffer.ClearBuffer(outBuf);
                    AwesomeSockets.Buffers.Buffer.ClearBuffer(inBuf);

                    trainer.Initialize();
                    Thread.Sleep(50);
                }
            }
            
        }

        private Dictionary<string, byte[]> GetOutputData(Trainer trainer)
        {
            var outputData = new Dictionary<string, byte[]>();

            outputData.Add("write_pos_ally", trainer.GET_LOCAL_POS());

            outputData.Add("write_hp_ally", trainer.GET_LOCAL_HP());

            outputData.Add("hp_enemy_data", Utils.ObjectToByteArray(trainer.GET_HP_ENEMY_DATA_FOR_SERVER()));
            outputData.Add("hp_gigante", trainer.GET_GIGANTE_HP());

            outputData.Add("ally_area", trainer.GET_LOCAL_AREA());

            return outputData;
        }

        private void HandleInputData(Dictionary<string, byte[]> data, Trainer trainer)
        {
            trainer.HANDLE_DIFFERENT_AREAS(data["ally_area"]);
            trainer.WRITE_POS_ALLY(data["write_pos_ally"]);
            trainer.WRITE_HP_ALLY(data["write_hp_ally"]);

            if (data.ContainsKey("write_pos_enemy_pointer"))
            {
                trainer.MAP_ENEMY_POINTER(data["write_pos_enemy_pointer"], data["write_pos_enemy_value"]);
            }

            trainer.WRITE_ENEMY_POSITIONS_CLIENT(Utils.Deserialize<Dictionary<byte[], byte[]>>(data["pos_enemy_data"]));
            trainer.WRITE_ENEMY_HP_CLIENT(Utils.Deserialize<Dictionary<byte[], byte[]>>(data["hp_enemy_data"]));

            trainer.WRITE_GIGANTE_HP(data["hp_gigante"]);
            trainer.WRITE_GIGANTE_POS(data["pos_gigante"]);
        }

        private void SetupClientTrainer(Trainer trainer)
        {
            // set up memory here
        }
    }
}
