using System;
using Doppelkopf.Models;

namespace Doppelkopf.Controllers
{
    partial class GameController
    {
        private State handleShufflingState(ClientMessage message)
        {
            return State.Premove;
        }
    }
}
