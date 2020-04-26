using System;
using System.Collections.Generic;
using Doppelkopf.Controllers;
using Doppelkopf.Models;

namespace Doppelkopf.Interfaces
{
    public interface IUserPermissionService
    {
        // use to check if user has token (check does NOT test if user is named yet)
        public bool IsMessageFromTokenizedUser(List<IClientConnectionController> clientConnectionControllers, ClientMessage message);

        // use to check if message is from a named user (this INCLUDES being a tokenized user)
        public bool IsMessageFromNamedUser(List<User> users, ClientMessage message);        
    }
}
