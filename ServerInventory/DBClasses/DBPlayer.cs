using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerInventory.DBClasses
{
    [Serializable]
    public class DBPlayer
    {
        [BsonId(false)]
        public String DBPlayerId;
        public String SteamId;
        public String CharacterName;
        public DateTime LastLoggedIn = DateTime.Now;
        public DateTime Created = DateTime.Now;

        public List<DBItem> Items;
    }
}
