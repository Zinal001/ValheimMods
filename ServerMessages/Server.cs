using BepInEx;
using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ServerMessages
{
    public class Server : SidedMod
    {
        public override Side ModSide => Side.Server;

        internal static ServerMessagesPlugin Instance { get; private set; }

        private readonly List<BaseMessage> Messages = new List<BaseMessage>();


        private void Awake()
        {
            ServerMessagesPlugin.InstanceLogger.LogInfo("[Server] Awake");
            MessageFactory.Init();
            LoadMessages();
        }

        public void OnEnable()
        {
            ServerMessagesPlugin.InstanceLogger.LogInfo("[Server] OnEnable");

            InvokeRepeating("SendFixedTimedMessages", Configs.ConfigCheckTimeout.Value, Configs.ConfigCheckTimeout.Value);

            if (Messages.Any(m => m is TimedMessage))
            {
                float checkTime = Mathf.Clamp((float)((TimedMessage)Messages.Where(m => m is TimedMessage).OrderBy(m => ((TimedMessage)m).DurationBetween).FirstOrDefault()).DurationBetween.TotalSeconds / 2f, 10f, 59f);
                InvokeRepeating("SendTimedMessages", 0f, checkTime);
            }
        }

        public void OnDisable()
        {
            ServerMessagesPlugin.InstanceLogger.LogInfo("[Server] OnDisable");
            CancelInvoke("SendFixedTimedMessages");
            CancelInvoke("SendTimedMessages");
        }

        private void LogDebug2(object value)
        {
            if (Configs.ShowDebugMessages.Value)
                LogDebug(value);
        }

        private void SendTimedMessages()
        {
            LogDebug2($"Checking {Messages.Count(m => m is TimedMessage && m.Enabled)} TimedMessages");
            foreach (BaseMessage msg in Messages.Where(m => m.Enabled && m.ShouldSend() && m is TimedMessage))
            {
                msg.SendMessage();
            }
        }

        private void SendFixedTimedMessages()
        {
            LogDebug2($"Checking {Messages.Count(m => m is FixedTimedMessage && m.Enabled)} FixedTimedMessages");
            foreach (BaseMessage msg in Messages.Where(m => m.Enabled && m.ShouldSend() && m is FixedTimedMessage))
            {
                msg.SendMessage();
            }
        }

        private void LoadMessages()
        {
            String filePath = System.IO.Path.Combine(Paths.ConfigPath, "ServerMessages.xml");
            if (System.IO.File.Exists(filePath))
            {
                ServerMessagesPlugin.InstanceLogger.LogInfo("Old version of ServerMessages.xml found, converting to .json");
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
                    if (Configs.ShowDebugMessages.Value)
                    {
                        Dictionary<BaseMessage.MessageTypes, int> msgTypes = new Dictionary<BaseMessage.MessageTypes, int>();
                        foreach (BaseMessage msg in messages)
                        {
                            if (!msgTypes.ContainsKey(msg.MessageType))
                                msgTypes[msg.MessageType] = 0;
                            msgTypes[msg.MessageType]++;
                        }

                        LogDebug2($"Found {messages.Length} messages. Types: {String.Join(", ", msgTypes.Select(p => $"{p.Key}: {p.Value}"))}");
                    }
                    Messages.AddRange(messages);
                }
                else if (!messages.Any())
                    LogDebug2($"No messages in config!");
                else
                    ServerMessagesPlugin.InstanceLogger.LogError($"Failed to deserialise messages config (ServerMessages.json)");
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
            for (int i = 0; i < messageNodes.Count; i++)
            {
                System.Xml.XmlNode messageNode = messageNodes.Item(i);
                msgNr++;
                String errorMessage = null;
                BaseMessage msg = MessageFactory.GetMessage(messageNode, ref errorMessage);
                if (msg == null)
                {
                    if (String.IsNullOrEmpty(errorMessage))
                        ServerMessagesPlugin.InstanceLogger.LogWarning($"Unable to parse message {msgNr}: Unknown error.");
                    else
                        ServerMessagesPlugin.InstanceLogger.LogWarning($"Unable to parse message {msgNr}: {errorMessage}.");
                }

                if (msg != null)
                    messages.Add(msg);
            }


            return messages.ToArray();
        }
    }
}
