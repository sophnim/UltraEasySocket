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
    public class SocketResourceManager
    {
        long socketSessionID = 0;
        ConcurrentBag<SocketAsyncEventArgs> argsPool = new ConcurrentBag<SocketAsyncEventArgs>();
        ConcurrentQueue<SocketSession> socketSessionPool = new ConcurrentQueue<SocketSession>();
        ConcurrentBag<List<ArraySegment<byte>>> arraySegmentListPool = new ConcurrentBag<List<ArraySegment<byte>>>();

        public string GetInfo()
        {
            return string.Format("argsPool:{0} socketSessionPool:{1}", argsPool.Count, socketSessionPool.Count);
        }

        public Socket CreateSocket()
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            return socket;
        }

        public void CloseSocket(Socket socket)
        {
            if ((null != socket) && socket.Connected)
            {
                try
                {
                    socket.Shutdown(SocketShutdown.Both);
                }
                catch
                {

                }
                try
                {
                    socket.Close();
                }
                catch
                {

                }
            }
        }

        
        public List<ArraySegment<byte>> AllocArraySegmentList()
        {
            List<ArraySegment<byte>> list;
            if (false == this.arraySegmentListPool.TryTake(out list))
            {
                list = new List<ArraySegment<byte>>();
            }
            else
            {
                list.Clear();
            }
            return list;
        }

        public void FreeArraySegmentList(List<ArraySegment<byte>> list)
        {
            list.Clear();
            this.arraySegmentListPool.Add(list);
        }
        

        public SocketAsyncEventArgs AllocSocketAsyncEventArgs(EventHandler<SocketAsyncEventArgs> ioCompleteEventHandler)
        {
            
            SocketAsyncEventArgs args = null;
            if (false == this.argsPool.TryTake(out args))
            {
                args = new SocketAsyncEventArgs();
                args.SetBuffer(null, 0, 0);
                args.BufferList = null;
                args.Completed += new EventHandler<SocketAsyncEventArgs>(ioCompleteEventHandler);
            }
            else
            {
                args.SetBuffer(null, 0, 0);
                args.BufferList = null;
            }

            args.UserToken = null;

            return args;
        }

        public void FreeSocketAsyncEventArgs(SocketAsyncEventArgs args)
        {
            args.BufferList = null;
            args.SetBuffer(null, 0, 0);
            this.argsPool.Add(args);
        }

        public SocketSession AllocSocketSession(Socket socket, SessionType sessionType, int receiveBufferSize = 8192)
        {
            SocketSession session = null;
            if (false == this.socketSessionPool.TryDequeue(out session))
            {
                session = new SocketSession(socket, receiveBufferSize);
            }
            else
            {
                session.Clear();
                session.socket = socket;
            }

            session.sessionType = sessionType;
            session.id = Interlocked.Increment(ref this.socketSessionID);
            return session;
        }

        public void FreeSocketSession(SocketSession session)
        {
            session.id = 0;
            this.socketSessionPool.Enqueue(session);
        }
    }
}
