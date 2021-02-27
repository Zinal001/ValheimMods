using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerInventory.DBClasses
{
    class XmlDatabase : IDB
    {
        private static readonly System.Xml.Serialization.XmlSerializer _XmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(DBPlayer[]));
        public String SavePath { get; private set; }

        private Dictionary<String, DBPlayer> _Players;

        private bool _Saving = false;

        public XmlDatabase(String savePath)
        {
            SavePath = savePath;
            //_Players = new Dictionary<String, DBPlayer>();
            _Players = new Dictionary<String, DBPlayer>();

            Load();
        }

        private void Load()
        {
            try
            {
                using (System.IO.StreamReader reader = new System.IO.StreamReader(SavePath, Encoding.UTF8))
                {
                    DBPlayer[] players = (DBPlayer[])_XmlSerializer.Deserialize(reader);
                    UnityEngine.Debug.Log($"Found {players.Length} players in XmlDatabase");
                    foreach (var player in players)
                        _Players[player.DBPlayerId] = player;
                }
            }
            catch { }
        }

        private void Save()
        {
            if (_Saving)
                return;

            _Saving = true;
            try
            {
                UnityEngine.Debug.Log($"Writing {_Players.Count} players into XML database");
                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(SavePath, false, Encoding.UTF8))
                    _XmlSerializer.Serialize(writer, _Players.Values.ToArray());
            }
            catch { }
            finally
            {
                _Saving = false;
            }
        }

        public void Close()
        {

        }

        public DBPlayer GetPlayerById(string dbId)
        {
            if (_Players.ContainsKey(dbId))
                return _Players[dbId];

            return null;
        }

        public DBItem[] GetPlayerItems(DBPlayer player)
        {
            return player.Items.ToArray();
        }

        public DBPlayer[] GetPlayersBySteamId(string steamId)
        {
            return _Players.Values.Where(p => p.SteamId == steamId).ToArray();
        }

        public bool InsertPlayer(DBPlayer player)
        {
            _Players[player.DBPlayerId] = player;
            Save();
            return true;
        }

        public bool UpdatePlayer(DBPlayer player)
        {
            _Players[player.DBPlayerId] = player;
            Save();
            return true;
        }
    }
}
