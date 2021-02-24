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
    public class FixedTimedMessage : BaseMessage
    {
        public Time Time { get; set; }

        private DateTime _LastSent = DateTime.Now;

        public override bool ShouldSend()
        {
            if(DateTime.Now.Hour == Time.Hour && DateTime.Now.Minute == Time.Minute)
            {
                double totalSecs = DateTime.Now.Subtract(_LastSent).TotalSeconds;
                //ServerMessagesPlugin.InstanceLogger.LogDebug($"Should send: {totalSecs}");
                if (totalSecs >= 50d)
                    return true;
            }

            return false;
        }

        public override void SendMessage()
        {
            base.SendMessage();
            _LastSent = DateTime.Now;
            //ServerMessagesPlugin.InstanceLogger.LogDebug("Sending FixedTimeMessage!");
        }

        public static bool Parse(BaseMessage message, XmlNode messageNode, ref String errorMessage)
        {
            if(message is FixedTimedMessage fixedTimedMessage)
            {
                var timeNode = messageNode.SelectSingleNode(".//Time");
                if (timeNode == null)
                {
                    errorMessage = "Missing Time element";
                    return false;
                }

                Time time;
                if(!Time.TryParse(timeNode.InnerText, out time))
                {
                    errorMessage = "Unable to parse Time element";
                    return false;
                }
                fixedTimedMessage.Time = time;

                return true;
            }

            return false;
        }
    }

    public class Time
    {
        public int Hour { get; set; }
        public int Minute { get; set; }
        public int Second { get; set; }

        public static bool TryParse(String str, out Time time)
        {
            time = null;

            String[] parts = str.Split(':');
            if (parts.Length > 3)
                return false;

            if (!int.TryParse(parts[0], out int hour))
                return false;
            if (hour < 0 || hour > 23)
                return false;

            if (!int.TryParse(parts[1], out int minute))
                return false;
            if (minute < 0 || minute > 59)
                return false;

            int second = 0;
            if(parts.Length > 2)
            {
                if (!int.TryParse(parts[2], out second))
                    return false;
                if (second < 0 || second > 59)
                    return false;
            }


            time = new Time() { Hour = hour, Minute = minute, Second = second };
            return true;
        }

        public override string ToString()
        {
            return $"{Hour:D2}:{Minute:D2}:{Second:D2}";
        }
    }
}
