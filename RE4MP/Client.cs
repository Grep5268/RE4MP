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
        public static async Task StartClient()
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
                //get data
                var test = new Dictionary<string, byte[]>();
                test.Add("pos_ally", new byte[] { 5, 6, 7, 8 });

                //write data to buffer
                AwesomeSockets.Buffers.Buffer.ClearBuffer(outBuf);
                AwesomeSockets.Buffers.Buffer.Add(outBuf, Utils.ObjectToByteArray(test));
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
                Console.WriteLine(string.Join(", ", res["pos_ally"]));

                //refresh rate
                Thread.Sleep(100);
            }
            
        }
    }
}
