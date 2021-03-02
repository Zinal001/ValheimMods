using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ServerMessages_Client
{
    [BepInPlugin("nu.zinal.plugins.servermessagesclientplugin", "Server Messages - Client", "1.0.0.0")]
    public class ClientPlugin : BaseUnityPlugin
    {

        private GameObject _MessageObj;
        private Text _MessageText;

        private bool _MessagesInitialized = false;

        private static List<Message> _Messages = new List<Message>();

        void OnEnable()
        {
            InvokeRepeating("InitZNet", 0f, 5f);
        }

        void OnDisable()
        {
            _MessagesInitialized = false;
            CancelInvoke("InitZNet");
            CancelInvoke("DisplayMessages");

            if (_MessageObj != null)
                Destroy(_MessageObj);
        }

        public static void RPC_Announcement(long sender, String horizontalAlignment, String verticalAlignment, String senderText, String text)
        {
            _Messages.Add(new Message() { Sender = sender, HorizontalAlignment = horizontalAlignment, VerticalAlignment = verticalAlignment, SenderText = senderText, Text = text });
            Debug.Log("Added announcement");
        }

        private void InitZNet()
        {
            if (ZNet.instance == null || ZRoutedRpc.instance == null)
                return;

            CancelInvoke("InitZNet");

            if (!ZNet.instance.IsServer() || !ZNet.instance.IsDedicated())
            {
                var otherText = Chat.instance.m_output;
                ZRoutedRpc.instance.Register<String, String, String, String>("Announcement", new RoutedMethod<string, string, string, string>.Method(RPC_Announcement));
                Logger.LogInfo($"ServerMessages-Client loaded");

                _MessageObj = new GameObject("AnnouncementText") { layer = 0 };
                _MessageObj.transform.SetParent(Hud.instance.m_rootObject.transform.parent.parent.parent);

                _MessageText = _MessageObj.AddComponent<Text>();
                _MessageText.raycastTarget = false;
                _MessageText.alignment = TextAnchor.MiddleCenter;
                _MessageText.horizontalOverflow = HorizontalWrapMode.Overflow;
                _MessageText.verticalOverflow = VerticalWrapMode.Overflow;
                _MessageText.resizeTextForBestFit = true;
                _MessageText.resizeTextMaxSize = 20;
                _MessageText.resizeTextMinSize = 8;
                _MessageText.color = Color.yellow;
                _MessageText.text = "<b>Hello</b> World!";
                _MessageText.resizeTextForBestFit = false;
                _MessageText.enabled = false;
                _MessageText.font = otherText.font;
                _MessageText.material = otherText.material;
                _MessageText.supportRichText = true;

                _MessageText.transform.position = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);

                _MessageObj.AddComponent<Shadow>();

                _MessagesInitialized = true;
                InvokeRepeating("DisplayMessages", 0f, 5f);
            }
        }

        private void DisplayMessages()
        {
            if (_MessageObj == null || _MessageText == null || !_MessagesInitialized)
                return;

            if (ZNet.instance == null || ZRoutedRpc.instance == null)
                return;

            if (_MessageText.enabled)
            {
                if (_MessageText.canvasRenderer.GetColor().a <= 0.05f)
                {
                    Logger.LogDebug("hiding announcement");
                    _Messages.RemoveAt(0);
                    _MessageText.enabled = false;
                }
            }

            if (!_MessageText.enabled && _Messages.Any())
            {
                _MessageText.text = $"[{_Messages[0].SenderText}] {_Messages[0].Text}";
                float x = Screen.width;
                float y = Screen.height;

                int textPosition = 0;

                if ("left".Equals(_Messages[0].HorizontalAlignment, StringComparison.OrdinalIgnoreCase))
                    x *= 0.1f;
                else if ("center".Equals(_Messages[0].HorizontalAlignment, StringComparison.OrdinalIgnoreCase) || "middle".Equals(_Messages[0].HorizontalAlignment, StringComparison.OrdinalIgnoreCase))
                {
                    x *= 0.5f;
                    textPosition = 1;
                }
                else if ("right".Equals(_Messages[0].HorizontalAlignment, StringComparison.OrdinalIgnoreCase))
                {
                    x *= 0.9f;
                    textPosition = 2;
                }

                if ("top".Equals(_Messages[0].VerticalAlignment, StringComparison.OrdinalIgnoreCase))
                    y *= 0.9f;
                else if ("center".Equals(_Messages[0].VerticalAlignment, StringComparison.OrdinalIgnoreCase) || "middle".Equals(_Messages[0].VerticalAlignment, StringComparison.OrdinalIgnoreCase))
                {
                    y *= 0.5f;
                    textPosition += 3;
                }
                else if ("bottom".Equals(_Messages[0].VerticalAlignment, StringComparison.OrdinalIgnoreCase))
                {
                    y *= 0.1f;
                    textPosition += 6;
                }

                _MessageText.color = Color.yellow;
                _MessageText.transform.position = new Vector3(x, y, 0);
                _MessageText.alignment = (TextAnchor)textPosition;
                _MessageText.enabled = true;
                _MessageText.CrossFadeAlpha(0f, 20f, true);
                Logger.LogDebug("Displaying announcement");
            }
        }


        private class Message
        {
            public long Sender;
            public String HorizontalAlignment;
            public String VerticalAlignment;
            public String SenderText;
            public String Text;
        }
    }
}
