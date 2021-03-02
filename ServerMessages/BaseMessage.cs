using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;

namespace ServerMessages
{
    [Serializable]
    public abstract class BaseMessage
    {
        public abstract MessageTypes MessageType { get; }

        public String Sender { get; set; } = "Server";

        public String Text { get; set; }

        public bool Enabled { get; set; } = true;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual bool ShouldSend()
        {
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void SendMessage()
        {
            ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "ChatMessage", new object[] {
                new Vector3(Configs.WorldTextXPosition.Value, Configs.WorldTextYPosition.Value, Configs.WorldTextZPosition.Value),
                2,
                Sender,
                Text
            });

            if(Configs.HudTextEnabled.Value)
            {
                ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "Announcement", new object[] {
                    Configs.HudTextHorizontalPosition.Value,
                    Configs.HudTextVerticalPosition.Value,
                    Sender,
                    Text
                });
            }
            

            if (Configs.ShowMessagesInConsole.Value)
                ServerMessagesPlugin.InstanceLogger.LogInfo($"[{Sender}] {Text}");
        }

        public enum MessageTypes
        {
            TimedMessage = 0,
            FixedTimedMessage = 1
        }
    }
}
