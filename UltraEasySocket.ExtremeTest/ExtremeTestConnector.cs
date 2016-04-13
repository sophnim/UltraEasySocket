using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace UltraEasySocket.ExtremeTest
{
    public class ExtremeTestConnector
    {
        string ip;
        int portNum;
        UltraEasySocket.UltraEasyTcpSocket ultraES;
        long receivedDataCount;
        long replyRecvCount;
        Random random = new Random();

        ConcurrentDictionary<SocketSession, long> sendNumDic = new ConcurrentDictionary<SocketSession, long>();

        public ExtremeTestConnector(int encryptLevel = 0)
        {
            // Create UltraEasySocket instance : Register socket event callback function
            this.ultraES = new UltraEasyTcpSocket(new Action<CallbackEventType, object, object>(OnSocketEventCallback), encryptLevel: encryptLevel);
        }

        public string GetStatus()
        {
            return string.Format("Connector: Total Session={0} ReceivedData={1},{2}", this.ultraES.GetTotalSessionNum(), this.receivedDataCount, this.replyRecvCount);
        }

        public void StartConnect(string ip, int portNum, int connectionNum)
        {
            this.ip = ip;
            this.portNum = portNum;

            for (var i = 1; i <= connectionNum; i++)
            {
                this.ultraES.TryConnect(ip, portNum);
            }
        }

        void OnSocketEventCallback(CallbackEventType eventType, object eventFrom, Object param)
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
                        int start = 0;
                        lock (random)
                        {
                            start = random.Next();
                        }
                        var sendMsg = Encoding.UTF8.GetBytes(string.Format("{0}", start));
                        sendNumDic.TryAdd(session, start);

                        this.ultraES.Send(session, sendMsg);
                    }
                    break;


                case CallbackEventType.SESSION_RECEIVE_DATA: // Session received data
                                                             // fromID : sessionID that received data
                                                             // param : received byte array : (byte[])
                    {
                        var session = eventFrom as SocketSession;
                        var recvMsg = Encoding.UTF8.GetString((byte[])param);

                        long v, x;
                        if (Int64.TryParse(recvMsg, out x))
                        {
                            this.sendNumDic.TryRemove(session, out v);
                            if (v != x)
                            {
                                Console.WriteLine("Error! reply not match sessionID={0} v={1} x={2}", eventFrom, v, x);
                            }
                            else
                            {
                                v++;
                                sendNumDic.TryAdd(session, v);
                                var replyMsg = string.Format("{0}", v);
                                this.ultraES.Send(session, Encoding.UTF8.GetBytes(replyMsg));
                                Interlocked.Increment(ref this.replyRecvCount);
                            }
                        }
                        else if (recvMsg.Equals("This is Broadcast Message"))
                        {

                        }
                        else
                        {
                            // If this happen, Critical Error!
                            Console.WriteLine("Error! unknown msg: {0} {1}", ((byte[])param).Length, recvMsg);
                        }

                        var cnt = Interlocked.Increment(ref this.receivedDataCount);

                        
                        // disconnect session occasionally
                        if (cnt % 3500 == 0)
                        {
                            Task.Run(() =>
                            {
                            //Console.WriteLine("Listener Disconnect {0}", fromID);
                            this.ultraES.CloseSession(session);
                            });
                        }
                        
                    }
                    break;


                case CallbackEventType.SESSION_CLOSED: // Session has been closed
                    {
                        var session = eventFrom as SocketSession;
                        long v;
                        this.sendNumDic.TryRemove(session, out v);
                        //Console.WriteLine("Connector Closed {0}", fromID);
                        // reconnect
                        this.ultraES.TryConnect(this.ip, this.portNum);
                    }
                    break;
            }
        }
    }
}
