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
    public class TimedMessage : BaseMessage
    {
        public DateTime? StartAt { get; set; } = null;

        public TimeSpan DurationBetween { get; set; }

        public DateTime? EndAt { get; set; } = null;

        [NonSerialized]
        private DateTime LastSent = DateTime.Now;

        public override void SendMessage()
        {
            base.SendMessage();

            LastSent = DateTime.Now;
        }

        public override bool ShouldSend()
        {
            if (StartAt.HasValue && DateTime.Now < StartAt.Value)
            {
                return false;
            }

            if (EndAt.HasValue && DateTime.Now > EndAt.Value)
            {
                return false;
            }

            bool shouldSend = DateTime.Now.Subtract(LastSent) >= DurationBetween;

            return shouldSend;
        }

        public static bool Parse(BaseMessage message, XmlNode messageNode, ref String errorMessage)
        {
            if(message is TimedMessage timedMessage)
            {
                var startAtNode = messageNode.SelectSingleNode(".//Start_At");
                if (startAtNode != null && !String.IsNullOrEmpty(startAtNode.InnerText) && !"null".Equals(startAtNode.InnerText))
                    timedMessage.StartAt = DateTime.Parse(startAtNode.InnerText);

                var durationNode = messageNode.SelectSingleNode(".//Duration_Between");
                if (durationNode == null)
                {
                    errorMessage = "Missing Duration_Between element";
                    return false;
                }
                timedMessage.DurationBetween = TimeSpan.Parse(durationNode.InnerText);

                var endAtNode = messageNode.SelectSingleNode(".//End_At");
                if (endAtNode != null && !String.IsNullOrEmpty(endAtNode.InnerText) && !"null".Equals(endAtNode.InnerText))
                    timedMessage.EndAt = DateTime.Parse(endAtNode.InnerText);

                return true;
            }

            return false;
        }
    }
}
