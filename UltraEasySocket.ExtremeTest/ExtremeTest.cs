using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace UltraEasySocket.ExtremeTest
{
    public class ExtremeTest
    {

        public void Start()
        {
            int encryptLevel = 0;// Listener and Connector should have same encryption level.

            var etl = new ExtremeTestListener(encryptLevel);
            etl.StartListen(12300);

            var etc = new ExtremeTestConnector(encryptLevel);
            etc.StartConnect("127.0.0.1", 12300, 1000);
            
            while (true)
            {
                Console.WriteLine("{0} {1}", etl.GetStatus(), etc.GetStatus());
                Thread.Sleep(1000);                
            }
            
        }
    }
}
