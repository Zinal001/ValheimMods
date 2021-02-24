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

            Configs.Init(this);

            MessageFactory.Init();
            LoadMessages();


        }

        private void Awake()
        {
            InvokeRepeating("SetupRpc", 0f, 5f);
        }

        private void SetupRpc()
        {
            if (ZRoutedRpc.instance == null)
                return;

            if (!_RpcInstanceFound)
            {
                _RpcInstanceFound = true;
                CancelInvoke("SetupRpc");

                InvokeRepeating("SendFixedTimedMessages", Configs.ConfigCheckTimeout.Value, Configs.ConfigCheckTimeout.Value);

                TimedMessage shorestTimedMessage = Messages.Where(m => m is TimedMessage).OrderBy(m => ((TimedMessage)m).DurationBetween.TotalSeconds).FirstOrDefault() as TimedMessage;
                if(shorestTimedMessage != null)
                    InvokeRepeating("SendTimedMessages", 0f, (float)(shorestTimedMessage.DurationBetween.TotalSeconds / 2d));

            }
        }

        private void SendTimedMessages()
        {
            foreach (BaseMessage msg in Messages.Where(m => m.Enabled && m.ShouldSend() && m is TimedMessage))
            {
                msg.SendMessage();
            }
        }

        private void SendFixedTimedMessages()
        {
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
                try
                {
                    var messages = DeserializeMessages(filePath);
                    if (messages != null && messages.Any())
                    {
                        Logger.LogDebug($"Found {messages.Length} messages");
                        Messages.AddRange(messages);
                    }
                    else if(!messages.Any())
                        Logger.LogDebug($"No messages in config!");
                    else
                        Logger.LogError($"Failed to deserialise messages config (ServerMessages.xml)");
                }
                catch(Exception ex)
                {
                    Logger.LogError(ex);
                }
            }
            else
            {
                System.IO.File.WriteAllText(filePath, Properties.Resources.ExampleMessages);
                /*TimedMessage exampleMsg = new TimedMessage() { Text = "This is an example timed message", Enabled = false, DurationBetween = TimeSpan.FromMinutes(1), StartAt = DateTime.Now };
                BaseMessage[] messageArray = new BaseMessage[] { exampleMsg };
                SerializeMessages(messageArray, filePath);*/
            }
        }

        private BaseMessage[] DeserializeMessages(String filename)
        {
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.Load(filename);

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

        private static void SerializeMessages(BaseMessage[] messages, String filename)
        {
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();

            var root = doc.AppendChild(doc.CreateElement("Messages"));

            foreach(BaseMessage message in messages)
            {
                System.Xml.XmlNode msgNode = doc.CreateElement("Message");

                System.Xml.XmlNode typeNode = doc.CreateElement("Type");
                typeNode.InnerText = message.GetType().Name;
                msgNode.AppendChild(typeNode);

                System.Xml.XmlNode enabledNode = doc.CreateElement("Enabled");
                enabledNode.InnerText = message.Enabled ? "True" : "False";
                msgNode.AppendChild(enabledNode);

                System.Xml.XmlNode senderNode = doc.CreateElement("Sender");
                senderNode.InnerText = message.Sender;
                msgNode.AppendChild(senderNode);

                System.Xml.XmlNode textNode = doc.CreateElement("Text");
                textNode.InnerText = message.Text;
                msgNode.AppendChild(textNode);

                if(message is TimedMessage timedMessage)
                {
                    System.Xml.XmlNode startAtNode = doc.CreateElement("Start_At");
                    startAtNode.InnerText = timedMessage.StartAt.HasValue ? timedMessage.StartAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : "";
                    msgNode.AppendChild(startAtNode);

                    System.Xml.XmlNode durationNode = doc.CreateElement("Duration_Between");
                    durationNode.InnerText = timedMessage.DurationBetween.ToString("hh\\:mm\\:ss");
                    msgNode.AppendChild(durationNode);

                    System.Xml.XmlNode endAtNode = doc.CreateElement("End_At");
                    endAtNode.InnerText = timedMessage.EndAt.HasValue ? timedMessage.EndAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : "";
                    msgNode.AppendChild(endAtNode);
                }
                else if(message is FixedTimedMessage fixedTimedMessage)
                {
                    System.Xml.XmlNode timeNode = doc.CreateElement("Time");
                    timeNode.InnerText = fixedTimedMessage.Time.ToString();
                    msgNode.AppendChild(timeNode);
                }


                root.AppendChild(msgNode);
            }

            doc.Save(filename);
        }
    }
}
