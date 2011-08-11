using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Net;
using System.Net.Sockets;

namespace System.Net.Sockets
{
    /// <summary>
    /// ソケットのReactive拡張
    /// </summary>
    public static class RxSocket
    {
        /// <summary>
        /// ソケットへの接続を配信します。
        /// </summary>
        /// <param name="socket">ソケット</param>
        /// <returns></returns>
        public static IObservable<Socket> RxAccept(this Socket socket)
        {
            var async = Observable.FromAsyncPattern(
                (callback, state) => socket.BeginAccept(callback, state),
                result => socket.EndAccept(result));
            return Observable.Defer(async);
        }

        /// <summary>
        /// IPアドレスへのリッスンを行ない、接続を配信します。
        /// </summary>
        /// <param name="port"></param>
        /// <param name="ipAddress"></param>
        /// <param name="backlog"></param>
        /// <returns></returns>
        public static IObservable<Socket> RxAccept(int port, string ipAddress = null, int backlog = 64)
        {
            var ip = IPAddress.Any;
            if (!string.IsNullOrWhiteSpace(ipAddress)) ip = IPAddress.Parse(ipAddress);
            var endPoint = new IPEndPoint(ip, port);
            var socket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Blocking = false;
            socket.Bind(endPoint);
            socket.Listen(backlog);
            return socket
                .RxAccept()
                .Finally(() =>
                {
                    socket.Close();
                });
        }


        /// <summary>
        /// ソケットからの受信を配信します。
        /// </summary>
        /// <param name="socket">ソケット</param>
        /// <param name="maxSize">一度に受信する最大サイズ</param>
        /// <param name="flags">ソケットのフラグ</param>
        /// <returns></returns>
        public static IObservable<byte[]> RxReceive(this Socket socket, int maxSize, SocketFlags flags = SocketFlags.None)
        {
            var buf = new byte[maxSize];
            var async = Observable.FromAsyncPattern(
                (callback, state) => socket.BeginReceive(buf, 0, maxSize, flags, callback, state),
                result =>
                {
                    var size = socket.EndReceive(result);
                    return buf.Copy(size);
                });
            return Observable
                .Defer(async)
                .Catch<byte[], ObjectDisposedException>(ex =>
                {
                    return Observable.Empty<byte[]>();
                });
        }

        private static byte[] Copy(this byte[] src, int length)
        {
            var dst = new byte[length];
            Array.Copy(src, dst, length);
            return dst;
        }

        /// <summary>
        /// byte[]データをソケットに非同期送信します。
        /// </summary>
        /// <param name="rxBuf">送信データ</param>
        /// <param name="socket">ソケット</param>
        /// <param name="flags">ソケットのフラグ</param>
        /// <returns></returns>
        public static IObservable<int> RxSend(this IObservable<byte[]> rxBuf, Socket socket, SocketFlags flags = SocketFlags.None)
        {
            var async = Observable.FromAsyncPattern<byte[], int>(
                (buf, callback, state) => socket.BeginSend(buf, 0, buf.Length, flags, callback, state),
                result => socket.EndSend(result));
            return rxBuf.SelectMany(buf => async(buf));
        }

    }
}