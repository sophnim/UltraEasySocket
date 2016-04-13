using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Concurrent;

namespace UltraEasySocket
{
    public enum SessionType
    {
        NOT_DEFINED,
        CONNECTED_SESSION,
        ACCEPTED_SESSION
    }

    public class SocketSession
    {
        public long id;
        public long readCount;
        public Socket socket;
        public int isSendAsyncCalled;
        public ConcurrentQueue<byte[]> sendQueue = new ConcurrentQueue<byte[]>();
        public byte[] receiveBuffer;
        public int receiveBufferStoredDataSize;
        public long pendingCount;
        public DateTime ipTime;
        public SessionType sessionType;
        public long userTokenNum;
        public string encryptionKey;
        public bool isUserCallbackHappen;

        public SocketSession(Socket socket, int receiveBufferSize = 8192)
        {
            Clear();
            this.socket = socket;
            this.receiveBuffer = new byte[receiveBufferSize];
        }

        public void Clear()
        {
            this.id = 0;
            this.isUserCallbackHappen = false;
            this.userTokenNum = 0;
            this.encryptionKey = string.Empty;
            this.readCount = 0;
            this.sessionType = SessionType.NOT_DEFINED;
            this.ipTime = DateTime.UtcNow;
            this.pendingCount = 0;
            this.isSendAsyncCalled = 0;
            this.receiveBufferStoredDataSize = 0;

            while (this.sendQueue.Count > 0)
            {
                byte[] qitem;
                this.sendQueue.TryDequeue(out qitem);
            }
        }

        public void IncPendingCount(DateTime utcNow)
        {
            this.ipTime = utcNow;
            var ret = Interlocked.Increment(ref pendingCount);
            //Console.WriteLine("IncPendingCount {0} {1}", socket.Handle, ret);
        }

        public void DecPendingCount(DateTime utcNow)
        {
            this.ipTime = utcNow;
            var ret = Interlocked.Decrement(ref pendingCount);
            //Console.WriteLine("DecPendingCount {0} {1}", socket.Handle, ret);
        }
    }
}
