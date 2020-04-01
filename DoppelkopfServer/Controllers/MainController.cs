using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Collections.Concurrent;
using DoppelkopfServer.Models;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Linq;

namespace DoppelkopfServer.Controllers
{
    public interface IMainController
    {
        Task ManageWebSocketRequest(HttpContext context);
    }

    // ===

    public class MainController : IMainController
    {
        private List<ClientConnectionController> clientControllers = new List<ClientConnectionController>();
        private ConcurrentQueue<Message> clientMessages = new ConcurrentQueue<Message>();
        private Thread serverThread = new Thread(MainController.RunServer);
        private List<User> users = new List<User>();

        public MainController()
        {
            serverThread.Start(this);
        }

        public async Task ManageWebSocketRequest(HttpContext context)
        {
            var newController = new ClientConnectionController();
            newController.MessageReceived += new EventHandler((Object source, EventArgs rawArgs) =>
            {
                var args = (ClientConnectionController.MessageReceivedArgs)rawArgs;
                clientMessages.Enqueue(args.Message);
            });            

            clientControllers.Add(newController);
            await newController.Handle(context);

            Debug.Write("Client disconnected from " + clientControllers.Count);
            clientControllers.Remove(newController);
            Debug.WriteLine(" to " + clientControllers.Count);
        }

        public static void RunServer(object parameter)
        {
            var mainController = (MainController) parameter;

            while (true)
            {
                Thread.Sleep(1);

                Message message;

                while(mainController.clientMessages.TryDequeue(out message))
                {
                    switch(message.Type)
                    {
                        case Message.MessageType.META:
                            mainController.HandleMetaMessage(message);
                            break;

                        case Message.MessageType.CHAT:
                            mainController.HandleChatMessage(message);
                            break;

                        case Message.MessageType.GAME:
                            mainController.HandleGameMessage(message);
                            break;
                    }
                }
                
            }
        }

        private void SendTo(Message message, IEnumerable<ClientConnectionController> recipients)
        {
            foreach (var recipient in recipients)
            {
                recipient.SendSync(message);
            }
        }

        private void HandleChatMessage(Message message)
        {
            throw new NotImplementedException();
        }

        private void HandleGameMessage(Message message)
        {
            throw new NotImplementedException();
        }

        private void HandleMetaMessage(Message message)
        {
            switch (message.SubType)
            {
                case "name":
                    // does user exist?
                    var existingUser = users.Where(user => user.ConnectionID == message.Token).FirstOrDefault(null);
                    Message responseToAll;
                    if (existingUser != null)
                    {
                        string oldUserName = existingUser.Name;
                        existingUser.Name = message.Text;
                        responseToAll = new Message
                        {
                            Type = Message.MessageType.META,
                            SubType = "rename",
                            Text = oldUserName,
                            Username = existingUser.Name
                        };
                    }
                    else
                    {
                        User user = new User(message.Token, message.Text);
                        users.Add(user);

                        responseToAll = new Message
                        {
                            Type = Message.MessageType.META,
                            SubType = "join",
                            Text = user.Name
                        };
                    }

                    SendTo(message, clientControllers);

                    break;
                }
            }
        }
    }
}
