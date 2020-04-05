using System;
namespace Doppelkopf.Models
{
    public class User : IComparable
    {
        public User(string connectionID, string name)
        {
            ConnectionID = connectionID;
            Name = name;
            TableID = NO_TABLE;
            Online = true;
        }

        public const int NO_TABLE = -1;

        public string ConnectionID { get; }
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

            return ConnectionID.CompareTo(otherUser.ConnectionID);
        }
    }
}
