using System;
namespace Doppelkopf.Models
{
    public class Player : IComparable
    {
        // user that player is based on
        public User User { set; get; }

        // sitting out?
        public bool SittingOut { set; get; }

        public int CompareTo(object obj)
        {
            return User.CompareTo(obj);
        }
    }
}
