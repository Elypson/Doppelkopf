﻿using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using DoppelkopfServer.Models;
using Microsoft.AspNetCore.Http;

namespace DoppelkopfServer.Interfaces
{
    public interface IClientConnectionController
    {
        WebSocket Socket { get; }
        string ConnectionID { get; }

        event EventHandler MessageReceived;

        class MessageReceivedArgs : EventArgs
        {
            public ClientMessage Message { get; set; }
        }

        // accept socket from context and memorize socket
        Task Initialize(HttpContext context);

        // start client event loop
        Task Handle(HttpContext context);
    }
}