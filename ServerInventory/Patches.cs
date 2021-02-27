using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace ServerInventory
{
    public class Patches
    {

        #region Server Patches
        [HarmonyPatch(typeof(ZNet), "RPC_CharacterID")]
        [HarmonyPostfix()]
        static void ZNet_RPC_CharacterID(ZNet __instance, ZRpc rpc, ZDOID characterID)
        {
            if (Configs.ShowDebugMessages.Value)
                UnityEngine.Debug.Log($"Got standard CharacterID");

            System.Reflection.MethodInfo _GetPeerMethod = _GetPeerMethod = typeof(ZNet).GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).FirstOrDefault(m => m.Name == "GetPeer" && m.GetParameters()[0].ParameterType == typeof(ZRpc));
            ZNetPeer peer = (ZNetPeer)_GetPeerMethod.Invoke(__instance, new object[] { rpc });

            if (peer != null)
            {
                UnityEngine.Debug.LogWarning($"{peer.m_playerName} didn't send inventory when connected.");
                //__instance.RemotePrint(peer.m_rpc, "Missing mod Server Inventory");
                //peer.m_rpc.Invoke("ShowMessage", new object[] { (int)MessageHud.MessageType.Center, "Missing Mod Server Inventory", 0, null });
                peer.m_rpc.Invoke("Error", new object[] { (int)ZNet.ConnectionStatus.ErrorVersion });
                //__instance.Kick(peer.m_playerName);
            }
        }

        [HarmonyPatch(typeof(ZNet), "RPC_PeerInfo")]
        [HarmonyPostfix()]
        static void ZNet_RPC_PeerInfo(ZNet __instance, ZRpc rpc, ZPackage pkg)
        {
            if (__instance.IsServer())
                rpc.Register<ZPackage>("CharacterIDX", ServerInventoryPlugin.RPC_CharacterIDX);
        }
        #endregion

        #region Client Patches
        [HarmonyPatch(typeof(ZNet), "SetCharacterID")]
        [HarmonyPrefix()]
        static bool ZNet_SetCharacterID(ZNet __instance, ZDOID id, ISocket ___m_hostSocket, List<ZNetPeer> ___m_peers)
        {
            ZPackage characterIdPackage = new ZPackage();
            characterIdPackage.Write(id);

            Player myPlayer = Player.m_localPlayer;
            if (myPlayer != null)
            {
                Inventory myInventory = myPlayer.GetInventory();
                if (myInventory != null)
                {
                    characterIdPackage.Write(Helper.PackageInventory(myPlayer.GetPlayerName(), myInventory));
                    if (Configs.ShowDebugMessages.Value)
                        UnityEngine.Debug.Log("Sent inventory to server.");
                }
                else if (Configs.ShowDebugMessages.Value)
                    UnityEngine.Debug.Log("Player inventory was null!");
            }
            else if (Configs.ShowDebugMessages.Value)
                UnityEngine.Debug.Log("Player was null!");

            ___m_peers[0].m_rpc.Invoke("CharacterIDX", new object[] { characterIdPackage });

            return false;
        }

        [HarmonyPatch(typeof(Game), "SavePlayerProfile")]
        [HarmonyPrefix()]
        static void Game_SavePlayerProfile(Game __instance, bool setLogoutPoint)
        {
            if(ZNet.instance != null && Player.m_localPlayer)
            {
                ZRpc serverRpc = ZNet.instance.GetServerRPC();
                if(serverRpc != null)
                {
                    if (Configs.ShowDebugMessages.Value)
                        UnityEngine.Debug.Log("Saving inventory");
                    Inventory playerInventory = Player.m_localPlayer.GetInventory();
                    ZPackage inventoryPackage = Helper.PackageInventory(Player.m_localPlayer.GetPlayerName(), playerInventory);
                    serverRpc.Invoke("UpdateInventory", new object[] { inventoryPackage });
                }
            }
        }
        #endregion

    }
}
