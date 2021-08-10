using System;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using BepInEx.Logging;
using HarmonyLib;
using Pathfinder.Util.XML;

namespace Pathfinder.Util
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class XMLStorageAttribute : Attribute
    {
        public bool IsContent;
        
        public XMLStorageAttribute(bool isContent = false)
        {
            IsContent = isContent;
        }
        
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public;
        
        public static XElement WriteToElement(object obj)
        {
            var thisType = obj.GetType();
            var attribType = typeof(XMLStorageAttribute);

            var element = new XElement(thisType.Name);

            bool hasSetContent = false;
            
            foreach (var fieldInfo in thisType.GetFields(Flags))
            {
                var attribs = fieldInfo.GetCustomAttributes(attribType, false);
                if (attribs.Length < 1)
                    continue;
                var attrib = (XMLStorageAttribute)attribs[0];
                if (fieldInfo.IsStatic || fieldInfo.FieldType != typeof(string))
                {
                    Logger.Log(LogLevel.Error, $"Invalid field for XML storage: {fieldInfo.Name}");
                    continue;
                }

                string val = (string)fieldInfo.GetValue(obj);
                if (attrib.IsContent)
                {
                    if (hasSetContent)
                    {
                        Logger.Log(LogLevel.Error, "Already set the content of this element");
                        continue;
                    }
                    
                    element.SetValue(val);
                    hasSetContent = true;
                }
                else
                {
                    element.SetAttributeValue(fieldInfo.Name, val);
                }
            }
            foreach (var propertyInfo in thisType.GetProperties(Flags))
            {
                var attribs = propertyInfo.GetCustomAttributes(attribType, false);
                if (attribs.Length < 1)
                    continue;
                var attrib = (XMLStorageAttribute)attribs[0];
                
                var getMethod = propertyInfo.GetGetMethod();
                var setMethod = propertyInfo.GetSetMethod();
                if ((getMethod?.IsStatic ?? true) || (setMethod?.IsStatic ?? true) || propertyInfo.PropertyType != typeof(string))
                {
                    Logger.Log(LogLevel.Error, $"Invalid property for XML storage: {propertyInfo.Name}");
                    continue;
                }

                string val = (string)getMethod.Invoke(obj, null);
                if (attrib.IsContent)
                {
                    if (hasSetContent)
                    {
                        Logger.Log(LogLevel.Error, "Already set the content of this element");
                        continue;
                    }
                    
                    element.SetValue(val);
                    hasSetContent = true;
                }
                else
                {
                    element.SetAttributeValue(propertyInfo.Name, val);
                }
            }

            return element;
        }

        public static void ReadFromElement(ElementInfo info, object obj)
        {
            var thisType = obj.GetType();
            var attribType = typeof(XMLStorageAttribute);

            bool hasSetContent = false;
            
            foreach (var fieldInfo in thisType.GetFields(Flags))
            {
                var attribs = fieldInfo.GetCustomAttributes(attribType, false);
                if (attribs.Length < 1)
                    continue;
                
                var attrib = (XMLStorageAttribute)attribs[0];
                if (fieldInfo.FieldType != typeof(string))
                {
                    Logger.Log(LogLevel.Error, $"Invalid field for XML storage: {fieldInfo.Name}");
                    continue;
                }

                if (attrib.IsContent)
                {
                    if (hasSetContent)
                    {
                        Logger.Log(LogLevel.Error, "Already got the content of this element");
                        continue;
                    }
                    
                    fieldInfo.SetValue(obj, info.Content ?? fieldInfo.GetValue(obj));
                    hasSetContent = true;
                }
                else
                {
                    fieldInfo.SetValue(obj, info.Attributes.GetString(fieldInfo.Name, (string)fieldInfo.GetValue(obj)));
                }
            }
            foreach (var propertyInfo in thisType.GetProperties(Flags))
            {
                var attribs = propertyInfo.GetCustomAttributes(attribType, false);
                if (attribs.Length < 1)
                    continue;
                var attrib = (XMLStorageAttribute)attribs[0];
                
                var getMethod = propertyInfo.GetGetMethod();
                var setMethod = propertyInfo.GetSetMethod();
                if ((getMethod?.IsStatic ?? true) || (setMethod?.IsStatic ?? true) || propertyInfo.PropertyType != typeof(string))
                {
                    Logger.Log(LogLevel.Error, $"Invalid property for XML storage: {propertyInfo.Name}");
                    continue;
                }

                if (attrib.IsContent)
                {
                    if (hasSetContent)
                    {
                        Logger.Log(LogLevel.Error, "Already got the content of this element");
                        continue;
                    }

                    setMethod.Invoke(obj, new object[] { info.Content ?? getMethod.Invoke(obj, null) });
                }
                setMethod.Invoke(obj, new object[] { info.Attributes.GetString(propertyInfo.Name, (string)getMethod.Invoke(obj, null)) });
            }
        }
    }
}
