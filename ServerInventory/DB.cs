using BepInEx;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerInventory
{
    /*internal static class DB
    {
        public static LiteDatabase PlayerDatabase { get; private set; }
        public static ILiteCollection<DBPlayer> Players { get; private set; }
        public static ILiteCollection<DBItem> Items { get; private set; }

        public static void Init()
        {
            PlayerDatabase = new LiteDatabase(System.IO.Path.Combine(Paths.PluginPath, "PlayerDatabase.db"));
            Players = PlayerDatabase.GetCollection<DBPlayer>("players");
            Items = PlayerDatabase.GetCollection<DBItem>("items");
        }

        public static DBPlayer GetPlayerById(String dbId)
        {
            return Players.Include(p => p.Items).FindOne(p => p.DBPlayerId == dbId);
        }

        public static DBPlayer[] GetPlayersBySteamId(String steamId)
        {
            return Players.Include(p => p.Items).Find(p => p.SteamId == steamId).ToArray();
        }

        public static void InsertPlayer(DBPlayer player)
        {
            Players.Insert(player);
        }

        public static void UpdatePlayer(DBPlayer player)
        {
            Players.Update(player);
        }

        public static void Dispose()
        {
            if (PlayerDatabase != null)
            {
                PlayerDatabase.Dispose();
                PlayerDatabase = null;
            }
        }

        

        
    }*/
}
