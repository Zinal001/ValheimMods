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
    public class BaseMessage
    {
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

            if (Configs.ShowMessagesInConsole.Value)
                ServerMessagesPlugin.InstanceLogger.LogInfo($"[{Sender}] {Text}");
        }
    }
}
