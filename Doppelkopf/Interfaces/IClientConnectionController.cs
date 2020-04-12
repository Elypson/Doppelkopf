using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Doppelkopf.Models;
using Microsoft.AspNetCore.Http;

namespace Doppelkopf.Interfaces
{
    public interface IClientConnectionController
    {
        WebSocket Socket { get; }
        string Token { get; }
        bool Initialized { get; }

        event EventHandler MessageReceived;

        class MessageReceivedArgs : EventArgs
        {
            public ClientMessage Message { get; set; }
        }

        // accept socket from context and memorize socket
        Task InitializeAsync(HttpContext context);

        // start client event loop
        Task HandleAsync(HttpContext context);

        // create a new Token because this connection belongs to a new user
        public string CreateToken();

        // set a Token because this connection belongs to an existing user
        public void ResetToken(string Token);
    }
}
