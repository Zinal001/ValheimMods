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
        [Newtonsoft.Json.JsonProperty(Order = 0)]
        public abstract MessageTypes MessageType { get; }

        [Newtonsoft.Json.JsonProperty(Order = 2)]
        public String Sender { get; set; } = "Server";

        [Newtonsoft.Json.JsonProperty(Order = 3)]
        public String Text { get; set; }

        [Newtonsoft.Json.JsonProperty(Order = 1)]
        public bool Enabled { get; set; } = true;


        [Newtonsoft.Json.JsonProperty(Order = 10)]
        public bool ShowOnHud { get; set; } = true;

        [Newtonsoft.Json.JsonProperty(Order = 11)]
        public String HorizontalHudAlignment { get; set; } = "center";

        [Newtonsoft.Json.JsonProperty(Order = 12)]
        public String VerticalHudAlignment { get; set; } = "top";
        

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

            if(ShowOnHud)
            {
                ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "Announcement", new object[] {
                    HorizontalHudAlignment,
                    VerticalHudAlignment,
                    Sender,
                    Text
                });
            }

            if (Configs.ShowMessagesInConsole.Value)
                ServerMessagesPlugin.InstanceLogger.LogInfo($"[{Sender}] {Text}");
                //ServerMessagesPlugin.InstanceLogger.LogInfo($"[{Sender}] {Text}");
        }

        public enum MessageTypes
        {
            TimedMessage = 0,
            FixedTimedMessage = 1
        }
    }
}
