using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Doppelkopf.Interfaces;
using Doppelkopf.Models;
using Microsoft.AspNetCore.Http;

namespace Doppelkopf.Controllers
{
    public class ClientConnectionController : IClientConnectionController
    {
        public WebSocket Socket { protected set; get; }
        public string ConnectionID { protected set; get; }

        public event EventHandler MessageReceived;

        public async Task InitializeAsync(HttpContext context)
        {
            Socket = await context.WebSockets.AcceptWebSocketAsync();
            ConnectionID = context.Connection.Id;
        }

        public async Task HandleAsync(HttpContext context)
        {
            var buffer = new byte[1024 * 4];

            WebSocketReceiveResult result;
            try
            {
                result = await Socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            catch(WebSocketException)
            {
                return; // if connection broke immediately
            }

            while (!result.CloseStatus.HasValue)
            {
                try
                {
                    Message message = JsonSerializer.Deserialize<Message>(Encoding.UTF8.GetString(buffer, 0, result.Count));
                    var clientMessage = new ClientMessage(message, context.Connection.Id);

                    MessageReceived?.Invoke(this, new IClientConnectionController.MessageReceivedArgs { Message = clientMessage });
                }
                catch(JsonException e)
                {
                    Debug.WriteLine(e);
                    // ignore invalid messages
                }

                try
                {
                    result = await Socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }
                catch (WebSocketException) // leave loop gracefully
                {
                    return;
                }
            }

            await Socket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
    }
}
