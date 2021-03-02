using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerMessages
{
    public abstract class SidedMod : UnityEngine.MonoBehaviour
    {
        public abstract Side ModSide { get; }


        protected void LogInfo(Object value)
        {
            ServerMessagesPlugin.InstanceLogger.LogInfo($"[{ModSide}] {value}");
        }

        protected void LogDebug(Object value)
        {
            ServerMessagesPlugin.InstanceLogger.LogDebug($"[{ModSide}] {value}");
        }

        protected void LogWarning(Object value)
        {
            ServerMessagesPlugin.InstanceLogger.LogWarning($"[{ModSide}] {value}");
        }

        public enum Side
        {
            Client = 0,
            Server = 1,
            DedicatedOnly = 2
        }
    }
}
