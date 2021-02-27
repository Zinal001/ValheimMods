using System;
using System.Collections.Generic;
using System.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace ServerInventory.DBClasses
{
    class JsonDatabase : IDB
    {
        private static readonly Dictionary<String, System.Reflection.PropertyInfo> _PlayerFields;
        private static readonly Dictionary<String, System.Reflection.PropertyInfo> _ItemFields;

        public String SavePath { get; private set; }

        //private Dictionary<String, DBPlayer> _Players;
        private PlayerCollection _Players;
        private int _ItemCounter = 0;

        private bool _Saving = false;

        static JsonDatabase()
        {
            _PlayerFields = new Dictionary<String, System.Reflection.PropertyInfo>();
            _ItemFields = new Dictionary<String, System.Reflection.PropertyInfo>();

            foreach (System.Reflection.PropertyInfo prop in typeof(DBPlayer).GetProperties().Where(p => p.CanWrite && p.CanRead))
            {
                System.ComponentModel.BrowsableAttribute browsableAttribute = (System.ComponentModel.BrowsableAttribute)prop.GetCustomAttributes(typeof(System.ComponentModel.BrowsableAttribute), true).FirstOrDefault();
                if (browsableAttribute != null && !browsableAttribute.Browsable)
                    continue;

                String keyName = prop.Name;

                System.ComponentModel.DisplayNameAttribute displayNameAttribute = (System.ComponentModel.DisplayNameAttribute)prop.GetCustomAttributes(typeof(System.ComponentModel.DisplayNameAttribute), true).FirstOrDefault();
                if (displayNameAttribute != null && !String.IsNullOrEmpty(displayNameAttribute.DisplayName))
                    keyName = displayNameAttribute.DisplayName;

                _PlayerFields[keyName] = prop;
            }


            foreach (System.Reflection.PropertyInfo prop in typeof(DBItem).GetProperties().Where(p => p.CanWrite && p.CanRead))
            {
                System.ComponentModel.BrowsableAttribute browsableAttribute = (System.ComponentModel.BrowsableAttribute)prop.GetCustomAttributes(typeof(System.ComponentModel.BrowsableAttribute), true).FirstOrDefault();
                if (browsableAttribute != null && !browsableAttribute.Browsable)
                    continue;

                String keyName = prop.Name;

                System.ComponentModel.DisplayNameAttribute displayNameAttribute = (System.ComponentModel.DisplayNameAttribute)prop.GetCustomAttributes(typeof(System.ComponentModel.DisplayNameAttribute), true).FirstOrDefault();
                if (displayNameAttribute != null && !String.IsNullOrEmpty(displayNameAttribute.DisplayName))
                    keyName = displayNameAttribute.DisplayName;

                _ItemFields[keyName] = prop;
            }
        }

        public JsonDatabase(String savePath)
        {
            SavePath = savePath;
            //_Players = new Dictionary<String, DBPlayer>();
            _Players = new PlayerCollection();

            Load();
        }

        private void Load()
        {
            if(System.IO.File.Exists(SavePath))
            {
                String json = System.IO.File.ReadAllText(SavePath, Encoding.UTF8);

                //DBPlayer[] players = JsonUtility.FromJson<DBPlayer[]>(json);
                PlayerCollection playersColl = JsonUtility.FromJson<PlayerCollection>(json);

                UnityEngine.Debug.Log($"Loading Json Database: {json}");
                _Players = playersColl;
                _Players.LoadFromArray();
                /*foreach (var player in players)
                    _Players[player.DBPlayerId] = player;*/

                /*if(JsonValue.Parse(json) is JsonArray playerArr)
                {
                    foreach(JsonValue playerVal in playerArr)
                    {
                        if(playerVal is JsonObject playerObj)
                        {
                            DBPlayer player = ParseAsPlayer(playerObj);
                            if (player != null)
                                _Players[player.DBPlayerId] = player;
                        }
                    }
                }*/
            }
        }

        private void Save()
        {
            if (_Saving)
                return;

            _Saving = true;
            try
            {
                _Players.UpdateArray();
                String json = JsonUtility.ToJson(_Players, true);
                UnityEngine.Debug.Log($"Saving {_Players.PlayersArray.Length} players to Json Database: {json}");
                //System.IO.File.WriteAllText(SavePath, json);

                /*JsonArray playerArr = new JsonArray();
                foreach (var player in _Players.Values)
                {
                    JsonObject playerObj = new JsonObject
                    {
                        ["DBPlayerId"] = player.DBPlayerId,
                        ["CharacterName"] = player.CharacterName,
                        ["SteamId"] = player.SteamId,
                        ["LastLoggedIn"] = player.LastLoggedIn,
                        ["Created"] = player.Created,
                    };

                    JsonArray itemsArray = new JsonArray();

                    foreach (var item in player.Items)
                    {
                        JsonObject itemObj = new JsonObject()
                        {
                            ["DbItemId"] = item.DbItemId,
                            ["ItemName"] = item.ItemName,
                            ["PlayerId"] = item.PlayerId,
                            ["StackCount"] = item.StackCount,
                            ["Quality"] = item.Quality,
                            ["Variant"] = item.Variant
                        };

                        itemsArray.Add(itemObj);
                    }

                    playerObj["Items"] = itemsArray;
                    playerArr.Add(playerObj);
                }

                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(SavePath, false, Encoding.UTF8))
                    playerArr.Save(writer);*/
            }
            catch { }
            finally
            {
                _Saving = false;
            }
        }

        public void Close()
        {
            Save();
        }

        public DBPlayer GetPlayerById(string dbId)
        {
            return _Players.Players.FirstOrDefault(p => p.DBPlayerId == dbId);
            /*if (_Players.ContainsKey(dbId))
                return _Players[dbId];

            return null;*/
        }

        public DBItem[] GetPlayerItems(DBPlayer player)
        {
            return player.Items.ToArray();
        }

        public DBPlayer[] GetPlayersBySteamId(string steamId)
        {
            return _Players.Players.Where(p => p.SteamId == steamId).ToArray();
            //return _Players.Values.Where(v => v.SteamId == steamId).ToArray();
        }

        public bool InsertPlayer(DBPlayer player)
        {
            _Players.Players.Add(player);
            //_Players[player.DBPlayerId] = player;
            Save();
            return true;
        }

        public bool UpdatePlayer(DBPlayer player)
        {
            for(int i = 0; i < _Players.Players.Count; i++)
            {
                if(_Players.Players[i].DBPlayerId == player.DBPlayerId)
                {
                    _Players.Players[i] = player;
                    Save();
                    return true;
                }
            }

            return false;

            /*_Players[player.DBPlayerId] = player;
            Save();*/
        }

        private DBPlayer ParseAsPlayer(JsonObject playerObj)
        {
            foreach (String requiredField in _PlayerFields.Values.Select(f => f.Name))
            {
                if (!playerObj.ContainsKey(requiredField))
                    return null;
            }

            DBPlayer inst = new DBPlayer()
            {
                DBPlayerId = playerObj["DBPlayerId"],
                CharacterName = playerObj["CharacterName"],
                SteamId = playerObj["SteamId"],
                LastLoggedIn = playerObj["LastLoggedIn"],
                Created = playerObj["Created"],
                Items = new List<DBItem>()
            };

            if (playerObj["Items"].JsonType != JsonType.Array)
                return null;

            foreach (JsonValue itemVal in playerObj["Items"])
            {
                if (itemVal is JsonObject itemObj)
                {
                    DBItem item = ParseAsItem(itemObj);
                    if (item == null)
                        return null;//TODO: Check Configs.JsonStrictParsing

                    inst.Items.Add(item);
                }
            }

            return inst;
        }

        private DBItem ParseAsItem(JsonObject itemObj)
        {
            foreach (String requiredField in _ItemFields.Values.Select(f => f.Name))
            {
                if (!itemObj.ContainsKey(requiredField))
                    return null;
            }

            DBItem item = new DBItem()
            {
                DbItemId = _ItemCounter++,
                ItemName = itemObj["ItemName"],
                PlayerId = itemObj["PlayerId"],
                StackCount = itemObj["StackCount"],
                Quality = itemObj["Quality"],
                Variant = itemObj["Variant"]
            };

            return item;
        }
    }
}
