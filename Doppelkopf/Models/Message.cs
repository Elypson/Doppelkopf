using System;
namespace DoppelkopfServer.Models
{
    // Message can come from server (ServerMessage) or client (Message that is extended by Server with Token to get ClientMessage),
    // details are in the respective files
    public class Message
    {
        public Message() { }

        // copy constructor
        public Message(Message message)
        {
            Type = message.Type;
            SubType = message.SubType;
            Text = message.Text;
        }

        public enum MessageType { META, CHAT, GAME };

        // Type can be set by both server and client with different meanings for the other fields
        public MessageType Type { get; set; }

        // Subtype contains a specific command or subtype that depends on the Type and might also be empty in some cases
        public string SubType { get; set; }

        // Text contains additional information or just Text in case of chats
        public string Text { get; set; }
    }

    // if MessageType is CHAT, then normal messages will be passed around, but only either globally or within the room
    // SubType is not used so far
    // from server: Username is set to user name of the user
    // from client: only Text matters
}
 