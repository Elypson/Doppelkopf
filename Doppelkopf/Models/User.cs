using System;
namespace Doppelkopf.Models
{
    public class User : IComparable
    {
        public User(string token, string name)
        {
            this.Token = token;
            Name = name;
            TableID = NO_TABLE;
            Online = true;
        }

        public const int NO_TABLE = -1;

        public string Token { get; }
        public string Name { get; set; }
        public int TableID { get; set; }
        public bool Online { get; set; }

        public int CompareTo(object obj)
        {
            if(obj == null)
            {
                return 1;
            }

            User otherUser = (User)obj;
            if(otherUser == null)
            {
                throw new ArgumentException("obj is not a User");
            }

            return Token.CompareTo(otherUser.Token);
        }
    }
}
