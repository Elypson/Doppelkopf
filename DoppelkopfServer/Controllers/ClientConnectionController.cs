using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DoppelkopfServer.Models;
using Microsoft.AspNetCore.Http;

namespace DoppelkopfServer.Controllers
{
    public class ClientConnectionController
    {
        private WebSocket socket;

        public event EventHandler MessageReceived;

        public class MessageReceivedArgs : EventArgs
        {
            public Message Message {get; set;}
        }

        public bool Open()
        {
            return socket.State == WebSocketState.Open || socket.State == WebSocketState.CloseReceived;
        }

        public void SendSync(Message message, bool final = true)
        {
            if (socket != null && Open())
            {
                lock (socket)
                {
                    socket?.SendAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message)),
                        WebSocketMessageType.Text, final, CancellationToken.None).Wait();
                }
            }
        }

        public async Task Initialize(HttpContext context)
        {
            socket = await context.WebSockets.AcceptWebSocketAsync();
        }

        public async Task Handle(HttpContext context)
        {
            var buffer = new byte[1024 * 4];

            WebSocketReceiveResult result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                try
                {
                    Message message = JsonSerializer.Deserialize<Message>(Encoding.UTF8.GetString(buffer, 0, result.Count));
                    message.Token = context.Connection.Id;

                    MessageReceived?.Invoke(this, new MessageReceivedArgs { Message = message });
                }
                catch(JsonException e)
                {
                    Debug.WriteLine(e);
                    // ignore invalid messages
                }
            }

            await socket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
    }
}
