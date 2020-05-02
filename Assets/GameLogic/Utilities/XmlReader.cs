using System;
using System.Text;
using System.Collections.Generic;
using System.Xml;

namespace Utilities.XmlReader
{
    public struct XmlNodeData
    {
        public string Name;
        public string Data;
        public List<XmlNodeData> Children;

        public XmlNodeData(string name, string data, List<XmlNodeData> children)
        {
            this.Name = name;
            this.Data = data;
            this.Children = children;
        }
    }

    public class XmlReader
    {
        // TODO: add maximum number of currently open XmlDocuments, order these by open time, i.e. cache

        private static Dictionary<string, XmlDocument> _openDocs = new Dictionary<string, XmlDocument>();

        // all paths passed to this class are relative to current directory
        // ex: path "/assets/xml_defs/stats.xml"
        private static string _curDir = System.IO.Directory.GetCurrentDirectory();

        #region NonStatic
        private XmlDocument _doc;

        // this class helps with accessing the static documents
        public XmlReader(string path)
        {
            this._doc = GetXmlDoc(path);
        }

        public bool HasField(string field) { return HasField(this._doc, field); }
        public bool HasField(List<string> fields) { return HasField(this._doc, fields); }
        public float GetFloat(string field) { return GetFloat(this._doc, field); }
        public float GetFloat(List<string> fields) { return GetFloat(this._doc, fields); }
        public string GetString(string field) { return GetString(this._doc, field); }
        public string GetString(List<string> fields) { return GetString(this._doc, fields); }
        public List<string> GetStrings(string field) { return GetStrings(this._doc, field); }
        public List<string> GetStrings(List<string> fields) { return GetStrings(this._doc, fields); }
        public List<string> GetChildren(string field) { return GetChildren(this._doc, field); }
        public List<string> GetChildren(List<string> fields) { return GetChildren(this._doc, fields); }
        #endregion NonStatic

        public static bool HasField(XmlDocument doc, List<string> fields)
        {
            return HasField(doc, GetFieldPathFromStringList(fields));
        }
        public static bool HasField(XmlDocument doc, string fieldPath)
        {
            XmlNode node = doc.DocumentElement.SelectSingleNode($"/{fieldPath}");
            return node != null;
        }

        public static float GetFloat(XmlDocument doc, List<string> fields)
        {
            return GetFloat(doc, GetFieldPathFromStringList(fields));
        }

        public static float GetFloat(XmlDocument doc, string fieldPath)
        {
            var s = GetString(doc, fieldPath);
            return float.Parse(s);
        }

        public static string GetString(XmlDocument doc, List<string> fields)
        {
            return GetString(doc, GetFieldPathFromStringList(fields));
        }

        public static string GetString(XmlDocument doc, string fieldPath)
        {
            XmlNode node = doc.DocumentElement.SelectSingleNode($"/{fieldPath}");
            if (node != null)
            {
                return node.InnerText;
            }
            else
                return "";
        }

        public static List<string> GetStrings(XmlDocument doc, List<string> fields)
        {
            return GetStrings(doc, GetFieldPathFromStringList(fields));
        }

        public static List<string> GetStrings(XmlDocument doc, string fieldPath)
        {
            XmlNodeList nodes = doc.DocumentElement.SelectNodes($"/{fieldPath}");
            return (nodes == null) ? null : RecursiveNodeToString(nodes);
        }

        public static List<string> GetChildren(XmlDocument doc, List<string> fields)
        {
            return GetChildren(doc, GetFieldPathFromStringList(fields));
        }

        public static List<string> GetChildren(XmlDocument doc, string fieldPath)
        {
            List<string> strings = new List<string>();
            XmlNodeList nodes = doc.DocumentElement.SelectNodes($"/{fieldPath}");
            if (nodes != null)
            {
                foreach (XmlNode node in nodes)
                    foreach (XmlNode childNode in node.ChildNodes)
                        strings.Add(childNode.Name);
                
            }
            return strings;
        }

        public static List<String> RecursiveNodeToString(XmlNodeList nodes, string name = "")
        {
            List<string> strings = new List<string>();
            foreach (XmlNode node in nodes)
                strings.AddRange(RecursiveNodeToString(node, name));
            return strings;
        }

        public static List<String> RecursiveNodeToString(XmlNode node, string name="")
        {
            List<string> strings = new List<string>();
            if (node.HasChildNodes)
                foreach (XmlNode child in node.ChildNodes)
                    foreach (var s in RecursiveNodeToString(child))
                        strings.Add(s);
            else if (name == "" || node.Name == name)
                strings.Add(node.InnerText);
            return strings;
        }

        public static XmlDocument AddNewXmlDoc(string path)
        {
            XmlDocument doc = ReadXmlDocument(path);
            _openDocs.Add(path, doc);
            return doc;
        }

        public static void ReloadOpenDocs()
        {
            foreach (var path in _openDocs.Keys)
            {
                _openDocs[path] = ReadXmlDocument(path);
            }
        }

        public static void ClearOpenDocs()
        {
            _openDocs = new Dictionary<string, XmlDocument>();
        }

        private static XmlDocument GetXmlDoc(string path)
        {
            if (_openDocs.ContainsKey(path))
                return _openDocs[path];
            else
                return AddNewXmlDoc(path);
        }

        private static string GetFieldPathFromStringList(List<string> fields)
        {
            // TODO: verify '/' at the end
            StringBuilder fieldPath = new StringBuilder();
            for (int i = 0; i < fields.Count; i++)
            {
                fieldPath.Append(fields[i]);
                if (i < fields.Count - 1)
                    fieldPath.Append("/");
            }
            return fieldPath.ToString();
        }

        private static XmlDocument ReadXmlDocument(string path)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load($"{_curDir}/{path}");
            return doc;
        }
    }
}
