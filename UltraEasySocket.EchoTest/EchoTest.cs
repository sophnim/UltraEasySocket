using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace UltraEasySocket.EchoTest
{
    public class EchoTest
    {
        public void Start()
        {
            int encryptLevel = 0;
            EchoServer server = new EchoServer(encryptLevel: encryptLevel);
            EchoClient client = new EchoClient(encryptLevel: encryptLevel);
            

            server.StartListen(12300);
            client.StartConnect("127.0.0.1", 12300, 100);
            

            while (true)
            {
                Console.WriteLine("{0}", server.GetStatus());
                Thread.Sleep(1000);
            }
        }
    }
}
