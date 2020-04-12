using System.Collections.Generic;
using Doppelkopf.Controllers;
using Doppelkopf.Models;

namespace Doppelkopf.Interfaces
{
    public interface IMetaMessageService
    {
        void HandleMessage(List<User> users, List<ClientConnectionController> clientConnectionControllers, List<IGameController> gameControllers, IUserPermissionService userPermissionService, ClientMessage message);
    }
}