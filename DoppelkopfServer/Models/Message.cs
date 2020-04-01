using System;
namespace DoppelkopfServer.Models
{
    public struct Message
    {
        public enum MessageType { META, CHAT, GAME };

        // Type can be set by both server and client with different meanings for the other fields
        public MessageType Type { get; set; }

        // Subtype contains a specific command or subtype that depends on the Type and might also be empty in some cases
        public string SubType { get; set; }

        // Text contains additional information or just Text in case of chats
        public string Text { get; set; }

        // Token is only used by server
        public string Token { get; set; }

        // Username is only sent by server
        public string Username { get; set; }
    }

    // if MessageType is META, then Subtype has further information
    // from server:
    // token: means that user should memorize the token provided by Text in case of connection issues, so they can reclaim a player
    // join: means that a user has joined with name provided in Text
    // rename: means that a user has renamed, where Username contains the new name and Text contains the old name

    // from client:
    // name: means that user sends their new username with Text="<username>"
    //  (must come directly after attained token, otherwise no user is created)
    // claim: means that user reclaims his user, he should then pass the token with Text="<token>"
}
