using AwesomeSockets.Domain.Sockets;
using AwesomeSockets.Sockets;

using System;
using System.Collections.Generic;
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

            while(true)
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
                    Tuple<int, EndPoint> received = AweSock.ReceiveMessage(server, inBuf);
                    AwesomeSockets.Buffers.Buffer.FinalizeBuffer(inBuf);

                    //parse response
                    var res = Utils.Deserialize(AwesomeSockets.Buffers.Buffer.GetBuffer(inBuf));
                    AwesomeSockets.Buffers.Buffer.ClearBuffer(inBuf);

                    //act on response
                    //Console.WriteLine(string.Join(", ", res["pos_ally"]));
                    this.HandleInputData(res, trainer);

                    //refresh rate
                    Thread.Sleep(50);
                }
                catch (Exception e)
                {
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

            outputData.Add("write_pos_ally", trainer.GET_POS_ALLY());

            return outputData;
        }

        private void HandleInputData(Dictionary<string, byte[]> data, Trainer trainer)
        {
            trainer.WRITE_POS_ALLY(data["write_pos_ally"]);
        }
    }
}
