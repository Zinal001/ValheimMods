using ServerInventory.DBClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerInventory
{
    interface IDB
    {
        DBPlayer GetPlayerById(String dbId);
        DBPlayer[] GetPlayersBySteamId(String steamId);

        DBItem[] GetPlayerItems(DBPlayer player);

        bool InsertPlayer(DBPlayer player);
        bool UpdatePlayer(DBPlayer player);

        void Close();
    }
}
