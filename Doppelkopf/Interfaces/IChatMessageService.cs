using System;
using System.Collections.Generic;
using Doppelkopf.Controllers;
using Doppelkopf.Models;

namespace Doppelkopf.Interfaces
{
    public interface IChatMessageService
    {
        void HandleMessage(List<User> users, List<IClientConnectionController> clientConnectionControllers, IUserPermissionService userPermissionService, ClientMessage message);
    }
}
