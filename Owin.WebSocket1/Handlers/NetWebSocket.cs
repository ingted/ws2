using System;
using System.IO;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Owin.WebSocket.Extensions;

namespace Owin.WebSocket.Handlers
{
    class NetWebSocket: IWebSocket
    {
        private readonly TaskQueue mSendQueue;
        private readonly System.Net.WebSockets.WebSocket mWebSocket;

        public NetWebSocket(System.Net.WebSockets.WebSocket webSocket)
        {
            mWebSocket = webSocket;
            mSendQueue = new TaskQueue();
        }

        public TaskQueue SendQueue
        {
            get { return mSendQueue; }
        }

        public WebSocketCloseStatus? CloseStatus
        {
            get { return mWebSocket.CloseStatus; }
        }

        public string CloseStatusDescription
        {
            get { return mWebSocket.CloseStatusDescription; }
        }

        public Task SendText(ArraySegment<byte> data, bool endOfMessage, CancellationToken cancelToken)
        {
            return Send(data, WebSocketMessageType.Text, endOfMessage, cancelToken);
        }

        public Task SendBinary(ArraySegment<byte> data, bool endOfMessage, CancellationToken cancelToken)
        {
            return Send(data, WebSocketMessageType.Binary, endOfMessage, cancelToken);
        }

        public Task Send(ArraySegment<byte> data, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancelToken)
        {
            var sendContext = new SendContext(data, endOfMessage, messageType, cancelToken);

            return mSendQueue.Enqueue(
                async s =>
                {
                    await mWebSocket.SendAsync(s.Buffer, s.Type, s.EndOfMessage, s.CancelToken);
                },
                sendContext);
        }

        public Task Close(WebSocketCloseStatus closeStatus, string closeDescription, CancellationToken cancelToken)
        {
            return mWebSocket.CloseAsync(closeStatus, closeDescription, cancelToken);
        }
        
        public async Task<Tuple<ArraySegment<byte>, WebSocketMessageType>> ReceiveMessage(int maxMessageSize, CancellationToken cancelToken)
        {
            var buffer = new ArraySegment<byte>(new byte[1024 * 8]);
            var result = await mWebSocket.ReceiveAsync(buffer, cancelToken);

            if (result.EndOfMessage)
            {
                return Tuple.Create(new ArraySegment<byte>(buffer.Array, 0, result.Count), result.MessageType);
            }

            var stream = new MemoryStream(1024 * 8);
            stream.Write(buffer.Array, 0, result.Count);
            var opType = result.MessageType;
            do
            {
                result = await mWebSocket.ReceiveAsync(buffer, cancelToken);
                stream.Write(buffer.Array, 0, result.Count);

                if (stream.Length > maxMessageSize && !result.EndOfMessage)
                {
                    // ignore rest of incoming message
                    do
                    {
                        result = await mWebSocket.ReceiveAsync(buffer, cancelToken);
                    }
                    while (!result.EndOfMessage);
                    throw new InternalBufferOverflowException(
                        "The Buffer is to small to get the Websocket Message! Increase in the Constructor!");
                }
            }
            while (!result.EndOfMessage);

            stream.Seek(0, SeekOrigin.Begin);
            buffer = new ArraySegment<byte>(new byte[stream.Length]);
            stream.Read(buffer.Array, 0, (int)stream.Length);

            return Tuple.Create(buffer, opType);
        }
    }
}
