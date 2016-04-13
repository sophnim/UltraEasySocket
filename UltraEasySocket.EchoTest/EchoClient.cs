using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
    

namespace UltraEasySocket.EchoTest
{
    public class EchoClient
    {
        UltraEasySocket.UltraEasyTcpSocket ultraES;
        bool threadRun = true;
        ConcurrentDictionary<SocketSession, int> sessionDic = new ConcurrentDictionary<SocketSession, int>();


        public EchoClient(int encryptLevel = 0)
        {
            // Create UltraEasySocket instance : Register socket event callback function
            this.ultraES = new UltraEasyTcpSocket(new Action<CallbackEventType, object, object>(OnSocketEventCallback), encryptLevel: encryptLevel);
        }

        public void StartConnect(string ip, int portNum, int connectionNum)
        {
            for (var i = 1; i <= connectionNum; i++)
            {
                this.ultraES.TryConnect(ip, portNum);
            }
        }

        void OnSocketEventCallback(CallbackEventType eventType, object eventFrom, object param)
        {
            switch (eventType)
            {
                case CallbackEventType.CONNECT_FAIL: // TryConnect() failed
                    // fromID : TryConnect() return value: sessionID
                    // param : (SocketError)
                    Console.WriteLine("Connect to Server Failed!");
                    break;


                case CallbackEventType.CONNECT_SUCCESS: // TryConnect() succeed
                    // fromID : TryConnect() return value: SessionID
                    {
                        var session = eventFrom as SocketSession;
                        this.sessionDic.TryAdd(session, 0);

                        var sendMsg = Encoding.UTF8.GetBytes("1234");
                        this.ultraES.Send(session, sendMsg);
                    }
                    break;


                case CallbackEventType.SESSION_RECEIVE_DATA: // Session received data
                                                             // fromID : sessionID that received data
                                                             // param : received byte array : (byte[])
                    {
                        var session = eventFrom as SocketSession;
                        var sendMsg = Encoding.UTF8.GetBytes("1234");
                        this.ultraES.Send(session, sendMsg);
                    }
                    break;


                case CallbackEventType.SESSION_CLOSED: // Session has been closed
                    {
                        var session = eventFrom as SocketSession;
                        int v;
                        this.sessionDic.TryRemove(session, out v);
                    }
                    break;
            }
        }
    }
}
