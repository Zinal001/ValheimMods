using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ServerInventory
{
    [BepInPlugin("nu.zinal.plugins.serverinventoryplugin", "Server Inventory", "1.0.0.0")]
    public class ServerInventoryPlugin : BaseUnityPlugin
    {
        private static readonly System.Reflection.MethodInfo _GetPeerMethod = typeof(ZNet).GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).FirstOrDefault(m => m.Name == "GetPeer" && m.GetParameters()[0].ParameterType == typeof(ZRpc));
        public static ServerInventoryPlugin Instance { get; private set; }

        public Dictionary<String, Server_PlayerInfo> Server_Players = new Dictionary<String, Server_PlayerInfo>();

        public bool Client_InventorySent = false;

        internal IDB DB { get; private set; }

        public ServerInventoryPlugin()
        {
            Instance = this;
        }


        private void Awake()
        {
            HarmonyLib.Harmony.CreateAndPatchAll(typeof(Patches));

            InvokeRepeating("InitServer", 0f, 2f);
        }

        private void InitServer()
        {
            if (ZNet.instance == null)
                return;

            CancelInvoke("InitServer");
            Configs.Init(this);

            if (ZNet.instance.IsServer())
            {

                if (Configs.ShowDebugMessages.Value)
                    Logger.LogInfo("Got ZNet instance");


                //TODO: Choose DB depending on Configs.DatabaseType

                //DB = new DBClasses.JsonDatabase(System.IO.Path.Combine(Paths.PluginPath, "PlayerInventories.json"));
                DB = new DBClasses.XmlDatabase(System.IO.Path.Combine(Paths.PluginPath, "PlayerInventories.xml"));
            }
        }

        private void OnDestroy()
        {
            if(DB != null)
            {
                DB.Close();
                DB = null;
            }
        }

        public static void RPC_UpdateInventory(ZRpc rpc, ZPackage pkg)
        {
            String characterName = pkg.ReadString();
            int itemsCount = pkg.ReadInt();

            String steamId = rpc.GetSocket().GetEndPointString();

            List<Server_ItemData> playerItems = new List<Server_ItemData>();

            for (int i = 0; i < itemsCount; i++)
            {
                Server_ItemData data = new Server_ItemData
                {
                    Name = pkg.ReadString(),
                    Stack = pkg.ReadInt(),
                    Quality = pkg.ReadInt(),
                    Variant = pkg.ReadInt()
                };

                playerItems.Add(data);
            }

            String dbId = $"{steamId}_{characterName}";
            var dbPlayer = Instance.DB.GetPlayerById(dbId);

            if (dbPlayer == null)
            {
                dbPlayer = new DBClasses.DBPlayer()
                {
                    DBPlayerId = dbId,
                    SteamId = steamId,
                    CharacterName = characterName,
                    Items = new List<DBClasses.DBItem>()
                };

                foreach (var item in playerItems)
                    dbPlayer.Items.Add(new DBClasses.DBItem() { ItemName = item.Name, PlayerId = dbPlayer.DBPlayerId, StackCount = item.Stack, Quality = item.Quality, Variant = item.Variant });

                Instance.DB.InsertPlayer(dbPlayer);
            }
            else
                Instance.DB.UpdatePlayer(dbPlayer);
        }

        //CALLED ON SERVER, Should contain ZDOID and Inventory
        public static void RPC_CharacterIDX(ZRpc rpc, ZPackage pkg)
        {
            ZDOID characterID = pkg.ReadZDOID();

            ZNetPeer peer = (ZNetPeer)_GetPeerMethod.Invoke(ZNet.instance, new object[] { rpc });
            if(peer != null)
            {
                peer.m_characterID = characterID;
                if (Configs.ShowDebugMessages.Value)
                    ZLog.Log($"Got character ZDOID with inventory from {peer.m_playerName} : {characterID}!");
            }

            ZPackage inventoryPackage = pkg.ReadPackage();

            String characterName = inventoryPackage.ReadString();
            int itemsCount = inventoryPackage.ReadInt();

            String steamId = rpc.GetSocket().GetEndPointString();

            if (Configs.ShowDebugMessages.Value)
                Instance.Logger.LogInfo($"Getting player {characterName}'s ({steamId}) inventory...");

            List<Server_ItemData> playerItems = new List<Server_ItemData>();

            for (int i = 0; i < itemsCount; i++)
            {
                Server_ItemData data = new Server_ItemData
                {
                    Name = inventoryPackage.ReadString(),
                    Stack = inventoryPackage.ReadInt(),
                    Quality = inventoryPackage.ReadInt(),
                    Variant = inventoryPackage.ReadInt()
                };

                playerItems.Add(data);
            }

            if (Configs.ShowDebugMessages.Value)
                Instance.Logger.LogInfo($"Found {playerItems.Count} items in {characterName}'s inventory.");


            String dbId = $"{steamId}_{characterName}";

            var dbPlayer = Instance.DB.GetPlayerById(dbId);
            if (dbPlayer == null)
            {
                if (Configs.ShowDebugMessages.Value)
                    Instance.Logger.LogInfo($"{characterName} is a new character!");

                DBClasses.DBPlayer[] characters = Instance.DB.GetPlayersBySteamId(steamId);

                if(characters.Length >= Configs.MaxCharactersPerPlayer.Value)
                {
                    rpc.Invoke("Error", new object[] { (int)ZNet.ConnectionStatus.ErrorVersion });
                    return;
                }

                dbPlayer = new DBClasses.DBPlayer()
                {
                    DBPlayerId = dbId,
                    SteamId = steamId,
                    CharacterName = characterName,
                    Items = new List<DBClasses.DBItem>()
                };

                foreach (var item in playerItems)
                    dbPlayer.Items.Add(new DBClasses.DBItem() { ItemName = item.Name, PlayerId = dbPlayer.DBPlayerId, StackCount = item.Stack, Quality = item.Quality, Variant = item.Variant });

                Instance.DB.InsertPlayer(dbPlayer);
            }
            else
            {
                bool isSame = true;

                if (dbPlayer.Items.Count != playerItems.Count)
                    isSame = false;
                else
                {
                    for (int i = 0; i < dbPlayer.Items.Count; i++)
                    {
                        if (dbPlayer.Items[i].ItemName != playerItems[i].Name || dbPlayer.Items[i].StackCount != playerItems[i].Stack || dbPlayer.Items[i].Quality != playerItems[i].Quality || dbPlayer.Items[i].Variant != playerItems[i].Variant)
                        {
                            isSame = false;
                            break;
                        }
                    }
                }

                if (isSame)
                {
                    if (Configs.ShowDebugMessages.Value)
                        Instance.Logger.LogInfo($"{characterName} is still the same");
                    dbPlayer.LastLoggedIn = DateTime.Now;
                    Instance.DB.UpdatePlayer(dbPlayer);
                }
                else
                {
                    if (Configs.ShowDebugMessages.Value)
                        Instance.Logger.LogWarning($"{characterName} is NOT the same!");

                    rpc.Invoke("Error", new object[] { (int)ZNet.ConnectionStatus.ErrorVersion });
                }
            }
        }

        public class Server_PlayerInfo
        {
            public String Name;
            public String SteamId;
            public bool GotInventory = false;
            public DateTime Added = DateTime.Now;
        }

        private class Server_ItemData
        {
            public String Name;
            public int Stack;
            public int Quality;
            public int Variant;
        }
    }
}
