using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RE4MP
{
    public class Program
    {
        public Dictionary<string, byte[]> Commands;

        //port is 11000
        static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                var trainer = new Trainer();
                trainer.Initialize();

                Console.WriteLine("Are you the 'server' or 'client'? (type server or client)");
                if (Console.ReadLine().Equals("server"))
                {
                    var server = new Server();
                    server.StartServer(trainer);
                }
                else
                {
                    var client = new Client();
                    await client.StartClient(trainer);
                }
            }).GetAwaiter().GetResult();
        }
    }
}
