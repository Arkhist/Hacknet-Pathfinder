using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using static System.Xml.XmlNodeType;

namespace PathfinderPatcher
{
    public static class TaskReader
    {
        //private static ICollection<NestedTypeTaskItem> readNestedT

        private static Tuple<string, AccessLevel> getNamedTaskFromElement(XmlTextReader reader)
        {
            if (!reader.MoveToAttribute("name"))
            {
                throw new FormatException(
                    $"TaskList XSD violation: Missing `name` attribute on `{reader.Name}` element{reader.LineInfoForExcept()}");
            }

            string targetName = reader.ReadContentAsString();
            return new Tuple<string, AccessLevel>(targetName, getAccessLevelFromElement(reader));
        }

        private static AccessLevel getAccessLevelFromElement(XmlTextReader reader)
        {
            if (!reader.MoveToAttribute("access"))
            {
                throw new FormatException(
                    $"TaskList XSD violation: Missing `access` attribute on `{reader.Name}` element{reader.LineInfoForExcept()}");
            }

            if (!Enum.TryParse(reader.ReadContentAsString(), ignoreCase: true, out AccessLevel typeLevel))
            {
                throw new FormatException(
                    $"TaskList XSD violation: Invalid `access` attribute value on `type` element.{reader.LineInfoForExcept()}");
            }

            return typeLevel;
        }

        private static GenericTypeTaskItem readTypeTasks(string containingNamespace, XmlTextReader reader)
        {
            GenericTypeTaskItem retVal;

            {
                if (!reader.MoveToAttribute("name"))
                {
                    throw new FormatException(
                        $"TaskList XSD violation: Missing `name` attribute on `type` element{reader.LineInfoForExcept()}");
                }

                string name = reader.ReadContentAsString();

                AccessLevel? typeLevel = null;
                if (reader.MoveToAttribute("access"))
                {
                    if (Enum.TryParse(reader.ReadContentAsString(), ignoreCase: true, out AccessLevel _typeLevel))
                        typeLevel = _typeLevel;
                    else
                    {
                        throw new FormatException(
                            $"TaskList XSD violation: Invalid `access` attribute value on `type` element.{reader.LineInfoForExcept()}");
                    }
                }

                retVal = containingNamespace == null
                    ? (GenericTypeTaskItem) new NestedTypeTaskItem(name, typeLevel)
                    : new TypeTaskItem(containingNamespace + "." + name, typeLevel);
            }

            while (!reader.EOF)
            {
                reader.Read();
                switch (reader.NodeType)
                {
                    case Element:
                        bool permitsContent = false;
                        string targetName;
                        AccessLevel targetLevel;
                        switch (reader.Name)
                        {
                            case "method":
                                (targetName, targetLevel) = getNamedTaskFromElement(reader);
                                retVal.addMethodModification(targetName, targetLevel);
                                break;
                            case "field":
                                (targetName, targetLevel) = getNamedTaskFromElement(reader);
                                retVal.addFieldModification(targetName, targetLevel);
                                break;
                            case "type":
                                permitsContent = true;
                                retVal.addTypeModification(readTypeTasks(null, reader));
                                break;
                            case "allFields":
                                retVal.newDefaultFieldAccess = getAccessLevelFromElement(reader);
                                break;
                            case "allMethods":
                                retVal.newDefaultMethodAccess = getAccessLevelFromElement(reader);
                                break;
                            case "allTypes":
                                retVal.newDefaultTypeAccess = getAccessLevelFromElement(reader);
                                break;
                        }

                        reader.MoveToElement();
                        if (!permitsContent && !reader.IsEmptyElement)
                        {
                            throw new FormatException(
                                $"TaskList XSD violation: No content permitted.{reader.LineInfoForExcept()}");
                        }

                        break;
                    case EndElement:
                        /* all other elements must be empty, therefore... */
                        Debug.Assert(reader.Name == "type",
                            "Reader at a non-`type` EndElement. This should not happen.");
                        return retVal;
                }
            }

            throw new FormatException("Unexpected end of file. This should never occur.");
        }

        public static List<TypeTaskItem> readTaskListFile(string filename)
        {
            XmlTextReader reader = new XmlTextReader(filename);

            var retVal = new List<TypeTaskItem>();

            /* skip to (and in the loop, past) root element */
            do
            {
                reader.Read();
            } while (reader.NodeType != Element);

            var namespaceChain = new LinkedList<string>();
            string namespaceCache = null;

            while (!reader.EOF)
            {
                reader.Read();
                switch (reader.NodeType)
                {
                    case Element:
                        switch (reader.Name)
                        {
                            case "namespace":
                                if (!reader.MoveToAttribute("name"))
                                {
                                    throw new FormatException(
                                        $"TaskList XSD violation: Missing `name` attribute on `namespace` element{reader.LineInfoForExcept()}");
                                }

                                namespaceChain.AddLast(reader.ReadContentAsString());
                                namespaceCache = null;
                                break;
                            case "type":
                                if (namespaceCache == null)
                                {
                                    namespaceCache = string.Join(".", namespaceChain);
                                }

                                retVal.Add((TypeTaskItem) readTypeTasks(namespaceCache, reader));

                                break;
                            default:
                                throw new FormatException(
                                    $"TaskList XSD violation: Unexpected `{reader.Name}` element{reader.LineInfoForExcept()}");
                        }

                        break;
                    case EndElement:
                        /* if the namespace chain is empty and an element has ended, then said element *must* be the document root, per XSD and per our throws. */
                        if (namespaceChain.Count == 0) return retVal;
                        Debug.Assert(reader.Name == "namespace",
                            "Reader at a non-`namespace` EndElement. This should not happen.");
                        namespaceChain.RemoveLast();
                        namespaceCache = null;
                        break;
                }
            }

            throw new FormatException("Unexpected end of file. This should never occur.");
        }
    }
}
