using System;
namespace DoppelkopfServer.Models
{
    public class User
    {
        public User(string connectionID, string name)
        {
            ConnectionID = connectionID;
            Name = name;
            TableID = NO_TABLE;
        }

        public const int NO_TABLE = -1;

        public string ConnectionID { get; }
        public string Name { get; set; }
        public int TableID { get; set; }
    }
}
