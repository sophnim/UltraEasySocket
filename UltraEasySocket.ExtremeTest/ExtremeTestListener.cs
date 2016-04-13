using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;

using UltraEasySocket;

namespace UltraEasySocket.ExtremeTest
{
    public class ExtremeTestListener
    {
        UltraEasySocket.UltraEasyTcpSocket ultraES;
        long receivedDataCount;
        
        ConcurrentDictionary<SocketSession, long> acceptSessionIdDic = new ConcurrentDictionary<SocketSession, long>();
        bool broadcastThreadRun = true;
        
        List<Thread> threadList = new List<Thread>();


        public ExtremeTestListener(int encryptLevel = 0)
        {
            // Create UltraEasySocket instance : Register socket event callback function
            this.ultraES = new UltraEasyTcpSocket(new Action<CallbackEventType, object, object>(OnSocketEventCallback), encryptLevel: encryptLevel);
        }

        ~ExtremeTestListener()
        {
            this.broadcastThreadRun = false;

            foreach (var t in this.threadList)
            {
                t.Join();
            }
        }

        public void StartListen(int portNum)
        {
            this.ultraES.StartListen(portNum, 100);
            
            
            // starts broadcast threads            
            for (var i = 1; i <= 2; i++)
            {
                var t = new Thread(BroadcastThreadProc);
                t.Start();
                threadList.Add(t);
            }
            
        }

        void BroadcastThreadProc(Object param)
        {
            var broadcastMsg = Encoding.UTF8.GetBytes("This is Broadcast Message");
            while (this.broadcastThreadRun)
            {
                foreach (var sessionID in acceptSessionIdDic.Keys)
                {
                    this.ultraES.Send(sessionID, broadcastMsg);
                }
                Thread.Sleep(1);
            }
        }

        public string GetStatus()
        {
            return string.Format("Listener: Total Session={0} ReceivedData={1} {2}", this.ultraES.GetTotalSessionNum(), this.receivedDataCount, this.ultraES.GetDebugInfo());
        }

        public void OnSocketEventCallback(CallbackEventType eventType, object eventFrom, Object param)
        {
            long temp;
            switch (eventType)
            {
                case CallbackEventType.ACCEPT_FAIL: // some AcceptAsync() function fails
                    // fromID : StartListen() return value: listenID
                    Console.WriteLine("Accept Failed!");
                    break;


                case CallbackEventType.ACCEPT_SUCCESS: // Accepted new session
                    // fromID : StartListen() return value: listenID
                    // param : (long)acceptedSessionID
                    var acceptedSession = param as SocketSession;
                    //Console.WriteLine("Listener Accept {0}", acceptedSessionID);
                    this.acceptSessionIdDic.TryAdd(acceptedSession, 0);
                    break;


                case CallbackEventType.SESSION_RECEIVE_DATA: // Session received data
                                                             // fromID : sessionID that received data
                                                             // param : received byte array : (byte[])
                                                             // send back

                    var session = eventFrom as SocketSession;
                    var msg = Encoding.UTF8.GetString((byte[])param);

                    this.ultraES.Send(session, (byte[])param);
                    var cnt = Interlocked.Increment(ref this.receivedDataCount);
                    
                    
                    // disconnect session occasionally
                    if (cnt % 2500 == 0)
                    {
                        Task.Run(() =>
                        {
                            //Console.WriteLine("Listener Disconnect {0}", fromID);
                            this.ultraES.CloseSession(session);
                        });
                    }
                    
                    break;


                case CallbackEventType.SESSION_CLOSED: // Session has been closed
                    var closedSession = eventFrom as SocketSession;
                    this.acceptSessionIdDic.TryRemove(closedSession, out temp);
                    //Console.WriteLine("Listener Closed {0}", fromID);
                    break;
            }
        }
    }
}
