using BepInEx;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ServerMessages
{
    [BepInPlugin("nu.zinal.plugins.servermessagesplugin", "Server Messages", "1.0.0.0")]
    public class ServerMessagesPlugin : BaseUnityPlugin
    {
        internal static ServerMessagesPlugin Instance { get; private set; }
        internal static BepInEx.Logging.ManualLogSource InstanceLogger { get; private set; }
        private readonly List<BaseMessage> Messages = new List<BaseMessage>();

        private static bool _RpcInstanceFound = false;

        public ServerMessagesPlugin()
        {
            Instance = this;
            InstanceLogger = Logger;
        }

        public static void LogDebug(object data)
        {
            if (InstanceLogger != null && Configs.ShowDebugMessages.Value)
                InstanceLogger.LogDebug(data);
        }

        private void Awake()
        {
            Configs.Init(this);

            MessageFactory.Init();
            LoadMessages();

            InvokeRepeating("SetupRpc", 0f, 5f);
        }

        private void SetupRpc()
        {
            if (ZNet.instance == null || ZRoutedRpc.instance == null)
                return;

            if (!_RpcInstanceFound)
            {
                _RpcInstanceFound = true;
                CancelInvoke("SetupRpc");

                if(ZNet.instance.IsServer())
                {
                    InvokeRepeating("SendFixedTimedMessages", Configs.ConfigCheckTimeout.Value, Configs.ConfigCheckTimeout.Value);

                    if (Messages.Any(m => m is TimedMessage))
                    {
                        float checkTime = Mathf.Clamp((float)((TimedMessage)Messages.Where(m => m is TimedMessage).OrderBy(m => ((TimedMessage)m).DurationBetween).FirstOrDefault()).DurationBetween.TotalSeconds / 2f, 10f, 59f);
                        InvokeRepeating("SendTimedMessages", 0f, checkTime);
                    }
                }
            }
        }

        private void SendTimedMessages()
        {
            LogDebug($"Checking {Messages.Count(m => m is TimedMessage && m.Enabled)} TimedMessages");
            foreach (BaseMessage msg in Messages.Where(m => m.Enabled && m.ShouldSend() && m is TimedMessage))
            {
                msg.SendMessage();
            }
        }

        private void SendFixedTimedMessages()
        {
            LogDebug($"Checking {Messages.Count(m => m is FixedTimedMessage && m.Enabled)} FixedTimedMessages");
            foreach (BaseMessage msg in Messages.Where(m => m.Enabled && m.ShouldSend() && m is FixedTimedMessage))
            {
                msg.SendMessage();
            }
        }

        private void LoadMessages()
        {
            String filePath = System.IO.Path.Combine(Paths.ConfigPath, "ServerMessages.xml");
            if(System.IO.File.Exists(filePath))
            {
                Logger.LogInfo("Old version of ServerMessages.xml found, converting to .json");
                var messages = DeserializeMessagesXml(filePath);

                String json = JsonConverter.ConvertToJson(messages);

                System.IO.File.WriteAllText(System.IO.Path.Combine(Paths.ConfigPath, "ServerMessages.json"), json);
                System.IO.File.Delete(filePath);
            }

            filePath = System.IO.Path.Combine(Paths.ConfigPath, "ServerMessages.json");
            if (System.IO.File.Exists(filePath))
            {
                String json = System.IO.File.ReadAllText(filePath);

                var messages = JsonConverter.ConvertTo<BaseMessage[]>(json);
                if (messages != null && messages.Any())
                {
                    if(Configs.ShowDebugMessages.Value)
                    {
                        Dictionary<BaseMessage.MessageTypes, int> msgTypes = new Dictionary<BaseMessage.MessageTypes, int>();
                        foreach(BaseMessage msg in messages)
                        {
                            if (!msgTypes.ContainsKey(msg.MessageType))
                                msgTypes[msg.MessageType] = 0;
                            msgTypes[msg.MessageType]++;
                        }

                        LogDebug($"Found {messages.Length} messages. Types: {String.Join(", ", msgTypes.Select(p => $"{p.Key}: {p.Value}"))}");
                    }
                    Messages.AddRange(messages);
                }
                else if (!messages.Any())
                    LogDebug($"No messages in config!");
                else
                    Logger.LogError($"Failed to deserialise messages config (ServerMessages.json)");
            }
            else
            {
                String json = Properties.Resources.ExampleMessages;
                System.IO.File.WriteAllText(filePath, json);
            }
        }

        private BaseMessage[] DeserializeMessagesXml(String filename)
        {
            String xml = System.IO.File.ReadAllText(filename, Encoding.UTF8);

            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml(xml);

            List<BaseMessage> messages = new List<BaseMessage>();

            int msgNr = 0;

            var messageNodes = doc.DocumentElement.SelectNodes("//Message");
            for(int i = 0; i < messageNodes.Count; i++)
            {
                System.Xml.XmlNode messageNode = messageNodes.Item(i);
                msgNr++;
                String errorMessage = null;
                BaseMessage msg = MessageFactory.GetMessage(messageNode, ref errorMessage);
                if (msg == null)
                {
                    if (String.IsNullOrEmpty(errorMessage))
                        Logger.LogWarning($"Unable to parse message {msgNr}: Unknown error.");
                    else
                        Logger.LogWarning($"Unable to parse message {msgNr}: {errorMessage}.");
                }

                if (msg != null)
                    messages.Add(msg);
            }


            return messages.ToArray();
        }
    }
}
