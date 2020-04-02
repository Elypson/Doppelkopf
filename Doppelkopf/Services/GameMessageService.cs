using System;
using System.Collections.Generic;
using Doppelkopf.Controllers;
using Doppelkopf.Interfaces;
using Doppelkopf.Models;

namespace Doppelkopf.Services
{
    public class GameMessageService : IGameMessageService
    {
        private readonly ISendService sendService;

        public GameMessageService(ISendService _sendService)
        {
            sendService = _sendService;
        }

        public void HandleMessage(List<User> users, List<ClientConnectionController> clientConnectionControllers, ClientMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
