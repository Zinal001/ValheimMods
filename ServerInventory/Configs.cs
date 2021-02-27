using BepInEx;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerInventory
{
    internal static class Configs
    {
        public static ConfigEntry<int> MaxCharactersPerPlayer { get; private set; }




        public static ConfigEntry<bool> ShowDebugMessages { get; private set; }

        public static void Init(BaseUnityPlugin plugin)
        {
            MaxCharactersPerPlayer = plugin.Config.Bind("Server", "Max Characters Per Player", 1, "How many characters can the player use on the server?");


            ShowDebugMessages = plugin.Config.Bind("Common", "Debug Messages", false, "Show debug messages in console?");
        }
    }
}
