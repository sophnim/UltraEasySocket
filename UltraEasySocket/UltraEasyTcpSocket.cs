// UltraEasySocket.Net by sophnim
// https://github.com/sophnim/UltraEasySocket.Net
// any questions: sophnim@gmail.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Threading;

namespace UltraEasySocket
{
    public class UltraEasyTcpSocket
    {
        SocketResourceManager socketResourceManager = new SocketResourceManager();
        ConcurrentDictionary<long, SocketSession> socketSessionIdDic = new ConcurrentDictionary<long, SocketSession>();
        ConcurrentDictionary<long, Socket> listenSocketDic = new ConcurrentDictionary<long, Socket>();

        readonly string encryptionKeyExchangeSuccess = "I got your encyption key";
        Action<CallbackEventType, Object, Object> userCallback;
        bool threadRun = true;
        List<Thread> threadList = new List<Thread>();
        DateTime utcNow = DateTime.UtcNow;
        int sessionReceiveBufferSize;
        int encryptLevel;
        string publicKey, privateKey;

        public string GetDebugInfo()
        {
            int total = 0;
            foreach (var session in socketSessionIdDic.Values)
            {
                total += session.receiveBufferStoredDataSize;
            }

            return total.ToString();
        }
        
        /*
        ecryptLevel
            0 : not encrypted (FASTEST)
            1 : simple xor encryption (FAST)
            2 : AES encryption (SLOW)
        */
        public UltraEasyTcpSocket(Action<CallbackEventType, Object, Object> userCallback, int sessionReceiveBufferSize = 8192, int encryptLevel = 0)
        {
            if ((encryptLevel < 0) || (encryptLevel > 2) || (null == userCallback) || (sessionReceiveBufferSize < 0))
            {
                throw new ArgumentException("Parameter Error");
            }            

            this.encryptLevel = encryptLevel;
            this.sessionReceiveBufferSize = sessionReceiveBufferSize;
            this.userCallback = userCallback;

            if (encryptLevel > 0)
            {
                Crypto.CreatePublicKeyAndPrivateKey(out this.publicKey, out this.privateKey);
            }

            var t = new Thread(SessionCloseThreadProc);
            t.Start();
            this.threadList.Add(t);
        }

        public void Terminate()
        {
            this.threadRun = false;

            foreach (var t in this.threadList)
            {
                t.Join();
            }
        }

        void SessionCloseThreadProc(Object param)
        {
            byte[] emptyData = new byte[0];
            while (this.threadRun)
            {
                this.utcNow = DateTime.UtcNow;
                foreach (var session in this.socketSessionIdDic.Values)
                {
                    if ((0 == session.pendingCount) && (this.utcNow.Subtract(session.ipTime).TotalMilliseconds > 100) && (!session.socket.Connected))
                    {
                        var sid = session.id;
                        var isUserKnowMyPresense = session.isUserCallbackHappen;
                        if (isUserKnowMyPresense)
                        {
                            this.userCallback(CallbackEventType.SESSION_CLOSED, session, null);
                        }

                        DisposeSocketSession(session);
                    }
                }

                Thread.Sleep(10);
            }
        }

        public int GetTotalSessionNum()
        {
            return socketSessionIdDic.Count;
        }

        public long StartListen(int portNum, int backlog = 5)
        {
            var listenSocket = this.socketResourceManager.CreateSocket();

            var ipep = new IPEndPoint(IPAddress.Any, portNum);
            listenSocket.Bind(ipep);
            listenSocket.Listen(backlog);

            for (var i = 0; i < backlog; i++)
            {
                var args = this.socketResourceManager.AllocSocketAsyncEventArgs(OnSocketIOCompleted);
                if (false == listenSocket.AcceptAsync(args))
                {
                    OnSocketIOCompleted(listenSocket, args);
                }
            }

            var listenID = listenSocket.Handle.ToInt64();
            listenSocketDic.TryAdd(listenID, listenSocket);
            return listenID;
        }

        public bool StopListen(long listenID)
        {
            Socket listenSocket;
            if (listenSocketDic.TryRemove(listenID, out listenSocket))
            {
                listenSocket.Close();
                return true;
            }
            else
            {
                return false;
            }
        }

        public SocketSession TryConnect(string ipaddr, int portNum)
        {
            var socket = this.socketResourceManager.CreateSocket();
            var session = this.socketResourceManager.AllocSocketSession(socket, SessionType.CONNECTED_SESSION, this.sessionReceiveBufferSize);
            var args = this.socketResourceManager.AllocSocketAsyncEventArgs(OnSocketIOCompleted);
            args.RemoteEndPoint = new IPEndPoint(IPAddress.Parse(ipaddr), portNum);
            args.UserToken = session;

            AddToSocketSessionDic(session);

            bool asyncResult = true;
            try
            {
                session.IncPendingCount(this.utcNow);
                asyncResult = session.socket.ConnectAsync(args);
            }
            catch
            {
                session.DecPendingCount(this.utcNow); 
                this.socketResourceManager.CloseSocket(socket);
                this.socketResourceManager.FreeSocketSession(session);
                this.socketResourceManager.FreeSocketAsyncEventArgs(args);

                return null;
            }

            if (false == asyncResult)
            {
                OnSocketIOCompleted(socket, args);
            }

            return session;
        }

        public ReturnCode Send(SocketSession session, byte[] sendData)
        {
            return Send(session, sendData, doNotEncrypt: false);
        }

        ReturnCode Send(SocketSession session, byte[] sendData, bool doNotEncrypt = false)
        {
            if (!doNotEncrypt)
            {
                switch (this.encryptLevel)
                {
                    case 1: sendData = Crypto.SimpleXorEncrypt(sendData, session.encryptionKey); break;
                    case 2: sendData = Crypto.AESEncrypt(sendData, session.encryptionKey); break;
                }
            }

            var socket = session.socket;

            if (null == socket)
            {
                return ReturnCode.ERROR_INVALID_SESSIONID;
            }
            else if (false == socket.Connected)
            {
                return ReturnCode.ERROR_SESSION_CLOSED;
            }

            if (0 == Interlocked.CompareExchange(ref session.isSendAsyncCalled, 1, 0))
            {
                return _Send(session, socket, sendData);
            }
            else
            {
                session.sendQueue.Enqueue(sendData);
                VerifyMissingSend(session);
            }

            return ReturnCode.SUCCESS;
        }

        ReturnCode _Send(SocketSession session, Socket socket, byte[] sendData)
        {
            var sid = session.id;
            var args = this.socketResourceManager.AllocSocketAsyncEventArgs(OnSocketIOCompleted);
            args.UserToken = session;

            var list = this.socketResourceManager.AllocArraySegmentList();
            list.Add(new ArraySegment<byte>(BitConverter.GetBytes(sendData.Length)));
            list.Add(new ArraySegment<byte>(sendData));
            args.BufferList = list;

            bool asyncResult = true;
            try
            {
                session.IncPendingCount(this.utcNow);
                asyncResult = socket.SendAsync(args);
            }
            catch
            {
                session.DecPendingCount(this.utcNow);
                this.socketResourceManager.CloseSocket(socket);
                this.socketResourceManager.FreeSocketAsyncEventArgs(args);
                this.socketResourceManager.FreeArraySegmentList(list);
                return ReturnCode.ERROR_SESSION_CLOSED;
            }

            if (false == asyncResult)
            {
                OnSocketIOCompleted(socket, args);
            }

            return ReturnCode.SUCCESS;
        }

        public ReturnCode CloseSession(SocketSession session)
        {
            this.socketResourceManager.CloseSocket(session.socket);
            return ReturnCode.SUCCESS;
        }

        void AddToSocketSessionDic(SocketSession session)
        {
            this.socketSessionIdDic.TryAdd(session.id, session);
        }

        void RemoveFromSocketSessionDic(SocketSession session)
        {
            SocketSession ss;
            this.socketSessionIdDic.TryRemove(session.id, out ss);
        }


        void OnSocketIOCompleted(object sender, SocketAsyncEventArgs args)
        {
            var socket = sender as Socket;
            SocketSession session = null;

            if (args.UserToken != null)
            {
                session = args.UserToken as SocketSession;
                session.DecPendingCount(this.utcNow);
            }
            else if (args.LastOperation != SocketAsyncOperation.Accept)
            {
                var list = args.BufferList as List<ArraySegment<byte>>;
                if (null != list)
                {
                    this.socketResourceManager.FreeArraySegmentList(list);
                }
                
                this.socketResourceManager.FreeSocketAsyncEventArgs(args);
                return;
            }

            switch (args.LastOperation)
            {
                case SocketAsyncOperation.Accept:
                    OnAcceptAsyncCompleted(socket, args);
                    break;
                case SocketAsyncOperation.Connect:
                    OnConnectAsyncCompleted(session, socket, args);
                    break;
                case SocketAsyncOperation.Send:
                    OnSendAsyncCompleted(session, socket, args);
                    break;
                case SocketAsyncOperation.Receive:
                    OnReceiveAsyncCompleted(session, socket, args);
                    break;
            }
        }

        void OnAcceptAsyncCompleted(Socket listenSocket, SocketAsyncEventArgs args)
        {
            bool asyncResult;
            if (args.SocketError != SocketError.Success)
            {
                this.userCallback(CallbackEventType.ACCEPT_FAIL, listenSocket.Handle.ToInt64(), null);
            }
            else
            {
                var acceptSocket = args.AcceptSocket;
                var acceptArgs = this.socketResourceManager.AllocSocketAsyncEventArgs(OnSocketIOCompleted);
                var acceptSession = this.socketResourceManager.AllocSocketSession(acceptSocket, SessionType.ACCEPTED_SESSION, this.sessionReceiveBufferSize);
                acceptSession.userTokenNum = listenSocket.Handle.ToInt64();
                acceptArgs.UserToken = acceptSession;

                AddToSocketSessionDic(acceptSession);

                acceptArgs.SetBuffer(acceptSession.receiveBuffer, 0, acceptSession.receiveBuffer.Length);

                var sid = acceptSession.id;

                if (this.encryptLevel > 0)
                {
                    Send(acceptSession, Encoding.UTF8.GetBytes(this.publicKey), true); // send public key to peer: not execute callback here
                }
                else
                {
                    this.userCallback(CallbackEventType.ACCEPT_SUCCESS, listenSocket.Handle.ToInt64(), acceptSession);
                    acceptSession.isUserCallbackHappen = true;
                }

                if (true == acceptSocket.Connected)
                {
                    bool rflag = true;
                    asyncResult = true;
                    try
                    {
                        acceptSession.IncPendingCount(this.utcNow); 
                        asyncResult = acceptSocket.ReceiveAsync(acceptArgs);
                    }
                    catch
                    {
                        acceptSession.DecPendingCount(this.utcNow);
                        this.socketResourceManager.CloseSocket(acceptSocket);
                        this.socketResourceManager.FreeSocketAsyncEventArgs(acceptArgs);
                        rflag = false;
                    }

                    if ((false == asyncResult) && (rflag))
                    {
                        OnSocketIOCompleted(acceptSession.socket, acceptArgs);
                    }
                }
            }

            // start next accept
            args.AcceptSocket = null;
            args.SetBuffer(null, 0, 0);

            asyncResult = true;
            try
            {
                asyncResult = listenSocket.AcceptAsync(args);
            }
            catch
            {
                this.userCallback(CallbackEventType.ACCEPT_FAIL, listenSocket.Handle.ToInt64(), null);
                this.socketResourceManager.CloseSocket(listenSocket);
                this.socketResourceManager.FreeSocketAsyncEventArgs(args);
                return;
            }

            if (false == asyncResult)
            {
                OnSocketIOCompleted(listenSocket, args);
            }
        }

        void OnConnectAsyncCompleted(SocketSession session, Socket socket, SocketAsyncEventArgs args)
        {
            var sid = session.id;

            if (args.SocketError != SocketError.Success)
            {
                this.socketResourceManager.CloseSocket(socket);
                this.socketResourceManager.FreeSocketAsyncEventArgs(args);
                return;
            }

            if (this.encryptLevel > 0)
            {
                // wait for public key receive
            }
            else
            {
                this.userCallback(CallbackEventType.CONNECT_SUCCESS, session, null);
                session.isUserCallbackHappen = true;
            }

            if (socket.Connected)
            {
                bool asyncResult = true;
                try
                {
                    session.IncPendingCount(this.utcNow);

                    args.SetBuffer(session.receiveBuffer, 0, session.receiveBuffer.Length);
                    asyncResult = socket.ReceiveAsync(args);
                }
                catch
                {
                    session.DecPendingCount(this.utcNow);
                    this.socketResourceManager.CloseSocket(socket);
                    this.socketResourceManager.FreeSocketAsyncEventArgs(args);
                    return;
                }

                if (false == asyncResult)
                {
                    OnSocketIOCompleted(socket, args);
                }
            }
        }

        void OnSendAsyncCompleted(SocketSession session, Socket socket, SocketAsyncEventArgs args)
        {
            var sid = session.id;
            if ((args.SocketError != SocketError.Success) || (0 == args.BytesTransferred))
            {
                this.socketResourceManager.CloseSocket(socket);
                var list = args.BufferList as List<ArraySegment<byte>>;
                this.socketResourceManager.FreeArraySegmentList(list);
                this.socketResourceManager.FreeSocketAsyncEventArgs(args);
                return;
            }

            byte[] qitem;
            if (session.sendQueue.TryPeek(out qitem))
            {
                var nlist = args.BufferList as List<ArraySegment<byte>>;
                nlist.Clear();
                while (session.sendQueue.TryDequeue(out qitem))
                {
                    nlist.Add(new ArraySegment<byte>(BitConverter.GetBytes(qitem.Length)));
                    nlist.Add(new ArraySegment<byte>(qitem));
                }
                args.BufferList = nlist;

                bool asyncResult = true;
                try
                {
                    session.IncPendingCount(this.utcNow);
                    asyncResult = socket.SendAsync(args);
                }
                catch
                {
                    session.DecPendingCount(this.utcNow);
                    this.socketResourceManager.CloseSocket(socket);
                    var list = args.BufferList as List<ArraySegment<byte>>;
                    this.socketResourceManager.FreeArraySegmentList(list);
                    this.socketResourceManager.FreeSocketAsyncEventArgs(args);
                    return;
                }

                if (false == asyncResult)
                {
                    OnSocketIOCompleted(session.socket, args);
                }
            }
            else
            {
                Interlocked.CompareExchange(ref session.isSendAsyncCalled, 0, 1);
                this.socketResourceManager.FreeSocketAsyncEventArgs(args);
                VerifyMissingSend(session);
            }
        }

        void VerifyMissingSend(SocketSession session)
        {
            if ((0 == session.isSendAsyncCalled) && (session.sendQueue.Count > 0))
            {
                byte[] emptyData = new byte[0];
                Send(session, emptyData, true);
            }
        }

        void OnReceiveAsyncCompleted(SocketSession session, Socket socket, SocketAsyncEventArgs args)
        {
            var sid = session.id;
            if ((args.SocketError != SocketError.Success) || (0 == args.BytesTransferred))
            {
                this.socketResourceManager.CloseSocket(socket);
                this.socketResourceManager.FreeSocketAsyncEventArgs(args);
                return;
            }

            session.IncPendingCount(this.utcNow); // to prevent disconnect while receive data processing
            var ret = ProcessReceivedData(session, socket, args);
            session.DecPendingCount(this.utcNow);

            if (false == ret)
            {
                this.socketResourceManager.CloseSocket(socket);
                this.socketResourceManager.FreeSocketAsyncEventArgs(args);
                return;
            }

            bool asyncResult = true;
            try
            {
                session.IncPendingCount(this.utcNow);
                asyncResult = socket.ReceiveAsync(args);
            }
            catch
            {
                session.DecPendingCount(this.utcNow);
                this.socketResourceManager.CloseSocket(socket);
                this.socketResourceManager.FreeSocketAsyncEventArgs(args);
                return;
            }

            if (false == asyncResult)
            {
                OnSocketIOCompleted(socket, args);
            }
        }

        bool AssignEncyptionKey(SocketSession session, byte[] data)
        {
            try
            {
                session.encryptionKey = Crypto.RSADecrypt(Encoding.UTF8.GetString(data), this.privateKey);
                return true;
            }
            catch
            {
                return false;
            }
        }

        byte[] SimpleXorEncrypt(SocketSession session, byte[] data)
        {
            try
            {
                return Crypto.SimpleXorEncrypt(data, session.encryptionKey);
            }
            catch
            {
                return null;
            }
        }

        byte[] AESDecrypt(SocketSession session, byte[] data)
        {
            try
            {
                return Crypto.AESDecrypt(data, session.encryptionKey);
            }
            catch
            {
                return null;
            }
        }

        bool ProcessReceivedData(SocketSession session, Socket socket, SocketAsyncEventArgs args)
        {
            var dataSize = session.receiveBufferStoredDataSize + args.BytesTransferred;
            var readPos = 0;
            var sid = session.id;

            long readCount = long.MaxValue;
            if (session.readCount < 10)
            {
                readCount = Interlocked.Increment(ref session.readCount);
            }

            while (dataSize >= 4)
            {
                if (!socket.Connected)
                {
                    return false;
                }

                var packetSize = BitConverter.ToInt32(session.receiveBuffer, readPos);
                if (packetSize < 0)
                {
                    // wrong packet size
                    Console.WriteLine("wrong packet size!!!");
                    return false;
                }

                dataSize -= 4;
                readPos += 4;

                if (dataSize >= packetSize)
                {
                    if (packetSize > 0)
                    {
                        var seg = new ArraySegment<byte>(session.receiveBuffer, readPos, packetSize);

                        if (this.encryptLevel > 0)
                        {
                            switch (session.sessionType)
                            {
                                case SessionType.ACCEPTED_SESSION:
                                    switch (readCount)
                                    {
                                        case 1: // peer encryption key received. decrypt & store
                                            if (!AssignEncyptionKey(session, seg.ToArray<byte>())) return false;
                                            
                                            Send(session, Encoding.UTF8.GetBytes(this.encryptionKeyExchangeSuccess), true);
                                            this.userCallback(CallbackEventType.ACCEPT_SUCCESS, session.userTokenNum, sid);
                                            session.isUserCallbackHappen = true;
                                            break;

                                        default:
                                            // decrypt with session.encrytionKey and callback
                                            switch (this.encryptLevel)
                                            {
                                                case 1:
                                                    var sxe = SimpleXorEncrypt(session, seg.ToArray<byte>());
                                                    if (sxe == null) return false;
                                                    this.userCallback(CallbackEventType.SESSION_RECEIVE_DATA, session, sxe);
                                                    break;

                                                case 2:
                                                    var aee = AESDecrypt(session, seg.ToArray<byte>());
                                                    if (aee == null) return false;
                                                    this.userCallback(CallbackEventType.SESSION_RECEIVE_DATA, session, aee);
                                                    break;
                                            }
                                            break;
                                    }
                                    break;

                                case SessionType.CONNECTED_SESSION:
                                    switch (readCount)
                                    {
                                        case 1: // public key received. send my encryption key
                                            session.encryptionKey = Crypto.CreateRandomString32(0);
                                            string ecdata = string.Empty;
                                            try
                                            {
                                                ecdata = Crypto.RSAEncrypt(session.encryptionKey, Encoding.UTF8.GetString(seg.ToArray<byte>()));
                                            }
                                            catch
                                            {
                                                return false;
                                            }
                                            Send(session, Encoding.UTF8.GetBytes(ecdata), true);
                                            break;

                                        case 2: // connection established
                                            if (!Encoding.UTF8.GetString(seg.ToArray<byte>()).Equals(this.encryptionKeyExchangeSuccess)) return false;
                                            this.userCallback(CallbackEventType.CONNECT_SUCCESS, session, null);
                                            session.isUserCallbackHappen = true;
                                            break;

                                        default:
                                            // decrypt with session.encrytionKey and callback
                                            switch (this.encryptLevel)
                                            {
                                                case 1:
                                                    var sxe = SimpleXorEncrypt(session, seg.ToArray<byte>());
                                                    if (sxe == null) return false;
                                                    this.userCallback(CallbackEventType.SESSION_RECEIVE_DATA, session, sxe);
                                                    break;

                                                case 2:
                                                    var aee = AESDecrypt(session, seg.ToArray<byte>());
                                                    if (aee == null) return false;
                                                    this.userCallback(CallbackEventType.SESSION_RECEIVE_DATA, session, aee);
                                                    break;
                                            }
                                            break;
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            this.userCallback(CallbackEventType.SESSION_RECEIVE_DATA, session, seg.ToArray<byte>());
                        }
                    }

                    dataSize -= packetSize;
                    readPos += packetSize;
                }
                else
                {
                    dataSize += 4;
                    readPos -= 4;
                    break;
                }
            }

            session.receiveBufferStoredDataSize = dataSize;
            if (dataSize > 0)
            {
                Buffer.BlockCopy(session.receiveBuffer, readPos, session.receiveBuffer, 0, dataSize);
                args.SetBuffer(session.receiveBuffer, dataSize, session.receiveBuffer.Length - dataSize);
            }
            else
            {
                args.SetBuffer(session.receiveBuffer, 0, session.receiveBuffer.Length);
            }

            return true;
        }

        void DisposeSocketSession(SocketSession session)
        {
            RemoveFromSocketSessionDic(session);
            this.socketResourceManager.FreeSocketSession(session);
        }
    }
}
