using System;
using System.Collections.Generic;
using Doppelkopf.Models;
using static Doppelkopf.Controllers.GameController;

namespace Doppelkopf.Interfaces
{
    public interface IGameController
    {
        int TableID { get; }
        string Name { get; }
        string Password { get; }
        bool Hidden { get; }
        List<User> Administrators { get; }

        State HandleMessage(ClientMessage message);
    }
}
