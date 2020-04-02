using System;
using System.Collections.Generic;
using DoppelkopfServer.Controllers;
using DoppelkopfServer.Models;

namespace DoppelkopfServer.Interfaces
{
    public interface IMessageService
    {
        void HandleMessage(List<User> users, List<ClientConnectionController> clientConnectionControllers, ClientMessage message);
    }
}
