using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace DoppelkopfServer.Interfaces
{
    public interface IMainController
    {
        // accept a websocket request, create ClientConnectionController and publish it with MainController's event queue
        Task ManageWebSocketRequest(HttpContext context);
    }
}
