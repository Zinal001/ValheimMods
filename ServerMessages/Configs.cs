using BepInEx;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerMessages
{
    internal static class Configs
    {
        internal static String[] ValidHorizontalPositions = new string[] { "left", "center", "middle", "right" };
        internal static String[] ValidVerticalPositions = new string[] { "top", "center", "middle", "bottom" };

        public static ConfigEntry<int> ConfigCheckTimeout { get; private set; }
        public static ConfigEntry<bool> ShowMessagesInConsole { get; private set; }

        public static ConfigEntry<float> WorldTextXPosition { get; private set; }
        public static ConfigEntry<float> WorldTextYPosition { get; private set; }
        public static ConfigEntry<float> WorldTextZPosition { get; private set; }

        public static ConfigEntry<bool> HudTextEnabled { get; private set; }
        public static ConfigEntry<String> HudTextHorizontalPosition { get; private set; }
        public static ConfigEntry<String> HudTextVerticalPosition { get; private set; }

        public static ConfigEntry<bool> ShowDebugMessages { get; private set; }

        public static void Init(BaseUnityPlugin plugin)
        {
            ConfigCheckTimeout = plugin.Config.Bind("General", "CheckTimeout", 60, "How often should the server check for fixed-timed messages");
            ShowMessagesInConsole = plugin.Config.Bind("General", "ShowMessagesInConsole", true, "Show messages when sent in console");

            WorldTextXPosition = plugin.Config.Bind("World Text Position", "X", 0f, "The X-position of the text in the world.");
            WorldTextYPosition = plugin.Config.Bind("World Text Position", "Y", 9999f, "The Y-position of the text in the world.");
            WorldTextZPosition = plugin.Config.Bind("World Text Position", "Z", 0f, "The Z-position of the text in the world.");

            HudTextEnabled = plugin.Config.Bind("Hud Text", "Enabled", true, "Should the messages be visible on the players hud? (Required the client-sided mod to be installed for a player to see them)");
            HudTextHorizontalPosition = plugin.Config.Bind("Hud Text", "Horizontal Position", "center", "The horizontal position of the hud text (if enabled), valid positions: left, center, right");
            HudTextVerticalPosition = plugin.Config.Bind("Hud Text", "Vertical Position", "top", "The vertical position of the hud text (if enabled), valid positions: top, center, bottom");

            ShowDebugMessages = plugin.Config.Bind("Debug", "Debug Messages", false, "Show debug messages in console?");
        }
    }
}
