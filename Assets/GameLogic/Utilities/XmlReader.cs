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

        public const string RootField = "root";
        public const string ItemField = "item";

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

        public bool HasField(string field) => HasField(this._doc, field);
        public bool HasField(List<string> fields) => HasField(this._doc, fields);

        public float GetFloat(string field) => GetFloat(this._doc, field);
        public float GetFloat(List<string> fields) => GetFloat(this._doc, fields);

        public float TryGetFloat(string field, float defaultValue) => TryGetFloat(this._doc, field, defaultValue);
        public float TryGetFloat(List<string> fields, float defaultValue) => TryGetFloat(this._doc, fields, defaultValue);

        public string GetString(string field) => GetString(this._doc, field);
        public string GetString(List<string> fields) => GetString(this._doc, fields);

        public List<string> GetStrings(string field) => GetStrings(this._doc, field);
        public List<string> GetStrings(List<string> fields) => GetStrings(this._doc, fields);

        public List<string> GetChildren(string field) => GetChildren(this._doc, field);
        public List<string> GetChildren(List<string> fields) => GetChildren(this._doc, fields);

        #endregion NonStatic

        public static float TryGetFloat(XmlDocument doc, List<string> fields, float defaultValue = 0f) =>
            TryGetFloat(doc, GetFieldPathFromStringList(fields), defaultValue);
        public static float TryGetFloat(XmlDocument doc, string fieldPath, float defaultValue = 0f)
        {
            if (HasField(doc, fieldPath))
                return GetFloat(doc, fieldPath);
            else
            {
                return defaultValue;
            }
        }

        public static bool HasField(XmlDocument doc, List<string> fields)
        {
            return HasField(doc, GetFieldPathFromStringList(fields));
        }

        public static bool HasField(XmlDocument doc, string fieldPath)
        {
            var path = GetXmlPathFromFieldPath(fieldPath);
            var node = doc.DocumentElement.SelectSingleNode(path);
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
            var path = GetXmlPathFromFieldPath(fieldPath);
            var node = doc.DocumentElement.SelectSingleNode(path);
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
            var path = GetXmlPathFromFieldPath(fieldPath);
            var nodes = doc.DocumentElement.SelectNodes(path);
            return (nodes == null) ? null : RecursiveNodeToString(nodes);
        }

        public static List<string> GetChildren(XmlDocument doc, List<string> fields) =>
            GetChildren(doc, GetFieldPathFromStringList(fields));

        public static List<string> GetChildren(XmlDocument doc, string fieldPath)
        {
            var strings = new List<string>();
            var path = GetXmlPathFromFieldPath(fieldPath);
            var nodes = doc.DocumentElement.SelectNodes(path);
            if (nodes != null)
            {
                foreach (XmlNode node in nodes)
                    foreach (XmlNode childNode in node.ChildNodes)
                        strings.Add(childNode.Name);
            }
            return strings;
        }

        public static List<string> RecursiveNodeToString(XmlNodeList nodes, string name = "")
        {
            var strings = new List<string>();
            foreach (XmlNode node in nodes)
                strings.AddRange(RecursiveNodeToString(node, name));
            return strings;
        }

        public static List<string> RecursiveNodeToString(XmlNode node, string name="")
        {
            var strings = new List<string>();
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
            var doc = ReadXmlDocument(path);
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

        private static string GetXmlPathFromFieldPath(string fieldPath) => $"/{RootField}/{fieldPath}";

        private static string GetFieldPathFromStringList(List<string> fields)
        {
            var fieldPath = new StringBuilder();
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
            var doc = new XmlDocument();
            doc.Load($"{_curDir}/{path}");
            return doc;
        }
    }
}
