using System;
using System.Xml;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using BepInEx.Logging;
using HarmonyLib;
using Hacknet;

namespace Pathfinder.Daemon
{
    public abstract class BaseDaemon : Hacknet.Daemon
    {
        public BaseDaemon(Computer computer, string serviceName, OS opSystem) : base(computer, serviceName, opSystem)
        {
            this.name = Identifier;
        }

        public virtual string Identifier => this.GetType().Name;

        public override string getSaveString()
        {
            var thisType = this.GetType();
            var attribType = typeof(XMLStorageAttribute);

            var builder = new StringBuilder();

            builder.Append($"<{thisType.Name} ");
            
            foreach (var fieldInfo in AccessTools.GetDeclaredFields(thisType))
            {
                if (fieldInfo.GetCustomAttributes(attribType, false).Length < 1)
                    continue;
                if (fieldInfo.IsStatic || fieldInfo.FieldType != typeof(string))
                {
                    Logger.Log(LogLevel.Error, $"Invalid field for XML storage: {fieldInfo.Name}");
                    continue;
                }

                string val = (string)fieldInfo.GetValue(this);
                builder.Append($"{fieldInfo.Name}=\"{val}\"");
            }
            foreach (var propertyInfo in AccessTools.GetDeclaredProperties(thisType))
            {
                if (propertyInfo.GetCustomAttributes(attribType, false).Length < 1)
                    continue;
                
                var getMethod = propertyInfo.GetGetMethod();
                var setMethod = propertyInfo.GetSetMethod();
                if (getMethod.IsStatic || setMethod.IsStatic || propertyInfo.PropertyType != typeof(string))
                {
                    Logger.Log(LogLevel.Error, $"Invalid property for XML storage: {propertyInfo.Name}");
                    continue;
                }

                string val = (string)getMethod.Invoke(this, null);
                builder.Append($"{propertyInfo.Name}=\"{val}\"");
            }

            builder.Append("/>");

            var a = builder.ToString();
            
            return builder.ToString();
        }

        public virtual void LoadFromXml(XmlReader reader)
        {
            var thisType = this.GetType();
            var attribType = typeof(XMLStorageAttribute);

            foreach (var fieldInfo in AccessTools.GetDeclaredFields(thisType))
            {
                if (fieldInfo.GetCustomAttributes(attribType, false).Length < 1)
                    continue;
                if (fieldInfo.IsStatic || fieldInfo.FieldType != typeof(string))
                {
                    Logger.Log(LogLevel.Error, $"Invalid field for XML storage: {fieldInfo.Name}");
                    continue;
                }

                string val = reader.GetAttribute(fieldInfo.Name);
                fieldInfo.SetValue(this, val);
            }
            foreach (var propertyInfo in AccessTools.GetDeclaredProperties(thisType))
            {
                if (propertyInfo.GetCustomAttributes(attribType, false).Length < 1)
                    continue;
                
                var getMethod = propertyInfo.GetGetMethod();
                var setMethod = propertyInfo.GetSetMethod();
                if (getMethod.IsStatic || setMethod.IsStatic || propertyInfo.PropertyType != typeof(string))
                {
                    Logger.Log(LogLevel.Error, $"Invalid property for XML storage: {propertyInfo.Name}");
                    continue;
                }

                string val = reader.GetAttribute(propertyInfo.Name);
                setMethod.Invoke(this, new object[] { val });
            }
        }
    }
}
