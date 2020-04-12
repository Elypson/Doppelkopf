using System;
namespace Doppelkopf.Models
{
    // subtypes for meta
    // name: means that user sends their new username with Text="<username>"
    //  (must come directly after attained token, otherwise no user is created)
    // claim: means that user reclaims his user, he should then pass the token with Text="<token>"
    // createTable: creates a new table with Text="<name>,<password>,<true/false=default>" and user is its founder (= admin),
    //  password can be left empty, hidden also
    // joinTable: attempt to join table with Text={"id":<TableID>[,"password":<password>]}
    // leaveTable: leaving table if sitting at one

    // subtypes for game
    // start: start the game
    public class ClientMessage : Message
    {
        // token is initially ConnectionID but is replaced with GUID Token later on
        public ClientMessage(Message message, string token) : base(message)
        {
            Token = token;
        }

        // token is assigned by server
        public string Token {set; get;}
    }
}
