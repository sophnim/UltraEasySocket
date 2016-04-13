using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace UltraEasySocket.EchoTest
{
    public class EchoServer
    {
        UltraEasySocket.UltraEasyTcpSocket ultraES;
        long echoCount = 0;
        

        public EchoServer(int encryptLevel = 0)
        {
            // Create UltraEasySocket instance : Register socket event callback function
            this.ultraES = new UltraEasyTcpSocket(new Action<CallbackEventType, object, object>(OnSocketEventCallback), sessionReceiveBufferSize: 8192, encryptLevel: encryptLevel);
        }
        
        public void StartListen(int portNum)
        {
            this.ultraES.StartListen(portNum, 100);
        }

        public string GetStatus()
        {
            var txt = string.Format("EchoServer: Total Session={0} EchoCount={1}", this.ultraES.GetTotalSessionNum(), this.echoCount);
            this.echoCount = 0;
            return txt;
        }

        public void OnSocketEventCallback(CallbackEventType eventType, object eventFrom, object param)
        {
            switch (eventType)
            {
                case CallbackEventType.ACCEPT_FAIL: // some AcceptAsync() function fails
                    // fromID : StartListen() return value: listenID
                    Console.WriteLine("Accept Failed!");
                    break;


                case CallbackEventType.ACCEPT_SUCCESS: // Accepted new session
                    // fromID : StartListen() return value: listenID
                    // param : (SocketSession)acceptedSessionID
                    // var acceptedSession = param as SocketSession;
                    //Console.WriteLine("Listener Accept {0}", acceptedSessionID);
                    break;


                case CallbackEventType.SESSION_RECEIVE_DATA: // Session received data
                                                             // fromID : sessionID that received data
                                                             // param : received byte array : (byte[])
                                                             // send back

                    var session = eventFrom as SocketSession;
                    this.ultraES.Send(session, (byte[])param);
                    Interlocked.Increment(ref this.echoCount);
                    break;


                case CallbackEventType.SESSION_CLOSED: // Session has been closed
                    var closedSession = eventFrom as SocketSession;
                    //Console.WriteLine("Listener Closed {0}", fromID);
                    break;
            }
        }
    }
}
