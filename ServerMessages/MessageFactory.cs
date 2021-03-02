using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ServerMessages
{
    public static class MessageFactory
    {
        private static Dictionary<String, Type> _MessageTypes = new Dictionary<String, Type>();
        private static Dictionary<String, MethodInfo> _MessageParseMethods = new Dictionary<String, MethodInfo>();

        public static void Init()
        {
            RegisterAssembly(Assembly.GetExecutingAssembly());
        }

        public static void RegisterAssembly(Assembly assembly)
        {
            try
            {
                Type[] messageTypes = assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(BaseMessage)) && t.IsPublic && !t.IsAbstract && t.IsClass).ToArray();
                foreach (Type type in messageTypes)
                    RegisterType(type);
            }
            catch(ReflectionTypeLoadException typeLoadEx)
            {
                UnityEngine.Debug.LogError($"Unable to load Registry {assembly.FullName}: {typeLoadEx.Message}");
                foreach (var ex in typeLoadEx.LoaderExceptions)
                    UnityEngine.Debug.LogError(ex);
            }
            catch(Exception ex)
            {
                UnityEngine.Debug.LogError(ex);
            }
        }

        public static void RegisterType(Type type)
        {
            if (!type.IsSubclassOf(typeof(BaseMessage)))
                throw new ArgumentException($"Type {type.Name} is not a subclass of BaseMessage", "type");

            if(!type.IsPublic)
                throw new ArgumentException($"Type {type.Name} is not public", "type");

            if (type.IsAbstract)
                throw new ArgumentException($"Type {type.Name} is an abstract class", "type");

            if (!type.IsClass)
                throw new ArgumentException($"Type {type.Name} is not a class", "type");

            MethodInfo parseMethod = type.GetMethods().Where(m => m.Name == "Parse").FirstOrDefault();
            if(parseMethod == null)
                throw new ArgumentException($"Type {type.Name} does not contain a Parse method.", "type");

            if(parseMethod.ReturnType != typeof(bool))
                throw new ArgumentException($"Parse method of {type.Name} doesn't return a boolean value.", "type");

            var parameters = parseMethod.GetParameters();


            if (parameters.Length != 3)
                throw new ArgumentException($"Parse method of {type.Name} has incorrect parameters. Should be BaseMessage, XmlNode and ref String.", "type");

            if (parameters[0].ParameterType != typeof(BaseMessage))
                throw new ArgumentException($"Parameter 1 in Parse method of {type.Name} is not of BaseMessage type.", "type");

            if (parameters[1].ParameterType != typeof(XmlNode))
                throw new ArgumentException($"Parameter 2 in Parse method of {type.Name} is not of System.Xml.XmlNode type.", "type");

            if (parameters[2].ParameterType.ToString() != "System.String&")
                throw new ArgumentException($"Parameter 3 in Parse method of {type.Name} is not of String type ({parameters[2].ParameterType}).", "type");


            if (!parameters[2].ParameterType.IsByRef || parameters[2].IsOut)
                throw new ArgumentException($"Parameter 3 in Parse method of {type.Name} is not passed by reference.", "type");

            _MessageParseMethods[type.Name] = parseMethod;
            _MessageTypes[type.Name] = type;
        }

        public static BaseMessage GetMessage(XmlNode messageNode, ref String errorMessage)
        {
            XmlNode typeNode = messageNode.SelectSingleNode(".//Type");
            if (typeNode == null)
            {
                errorMessage = "Missing Type element";
                return null;
            }


            if (!_MessageTypes.ContainsKey(typeNode.InnerText))
            {
                errorMessage = "Unknown message type";
                return null;
            }

            if (!_MessageParseMethods.ContainsKey(typeNode.InnerText))
            {
                errorMessage = "Unknown message type";
                return null;
            }

            BaseMessage msg = (BaseMessage)Activator.CreateInstance(_MessageTypes[typeNode.InnerText]);

            var enabledNode = messageNode.SelectSingleNode(".//Enabled");
            if (enabledNode == null)
            {
                errorMessage = "Missing Enabled element";
                return null;
            }
            msg.Enabled = "true".Equals(enabledNode.InnerText, StringComparison.OrdinalIgnoreCase);

            var senderNode = messageNode.SelectSingleNode(".//Sender");
            if (senderNode != null)
                msg.Sender = senderNode.InnerText;

            var textNode = messageNode.SelectSingleNode(".//Text");
            if (textNode == null)
            {
                errorMessage = "Missing Text element";
                return null;
            }
            msg.Text = textNode.InnerText;

            String invokeErrorMessage = null;
            object[] invokeParameters = new object[] { msg, messageNode, invokeErrorMessage };

            bool parsedCorrectly = (bool)_MessageParseMethods[typeNode.InnerText].Invoke(null, invokeParameters);
            if (!parsedCorrectly)
            {
                errorMessage = (String)invokeParameters[2];
                return null;
            }

            return msg;
        }
    }
}
