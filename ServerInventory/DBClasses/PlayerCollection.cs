using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerInventory.DBClasses
{
    [Serializable]
    public class PlayerCollection
    {
        public List<DBPlayer> Players = new List<DBPlayer>();

        public DBPlayer[] PlayersArray = new DBPlayer[0];

        internal void LoadFromArray()
        {
            Players = PlayersArray.ToList();
        }

        internal void UpdateArray()
        {
            PlayersArray = Players.ToArray();
        }
    }
}
