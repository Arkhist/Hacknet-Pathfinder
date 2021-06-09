using System;
using System.Text;
using System.Xml;
using BepInEx.Logging;
using HarmonyLib;

namespace Pathfinder.Util
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class XMLStorageAttribute : Attribute
    {
        public static string WriteToXml(object obj)
        {
            var thisType = obj.GetType();
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

                string val = (string)fieldInfo.GetValue(obj);
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

                string val = (string)getMethod.Invoke(obj, null);
                builder.Append($"{propertyInfo.Name}=\"{val}\"");
            }

            builder.Append("/>");
            
            return builder.ToString();
        }

        public static void ReadFromXml(XmlReader reader, object obj)
        {
            var thisType = obj.GetType();
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
                fieldInfo.SetValue(obj, val);
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
                setMethod.Invoke(obj, new object[] { val });
            }
        }
    }
}
