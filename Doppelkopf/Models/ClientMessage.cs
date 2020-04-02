using System;
namespace DoppelkopfServer.Models
{
    // name: means that user sends their new username with Text="<username>"
    //  (must come directly after attained token, otherwise no user is created)
    // claim: means that user reclaims his user, he should then pass the token with Text="<token>"
    public class ClientMessage : Message
    {
        public ClientMessage(Message message, string token) : base(message)
        {
            Token = token;
        }

        // token is assigned by server
        public string Token {set; get;}
    }
}
