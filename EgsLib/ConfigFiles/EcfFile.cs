using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace EgsLib.ConfigFiles
{
    public interface IEcfChild
    {
        string Name { get; }
        IReadOnlyDictionary<string, string> Properties { get; }
    }

    public interface IEcfObject
    {
        /// <summary>
        /// The object's type
        /// </summary>
        string Type { get; }

        /// <summary>
        /// Fields that describe the object, found on the same line as object Type
        /// </summary>
        IReadOnlyDictionary<string,string> Fields { get; }

        /// <summary>
        /// Properties of the object
        /// </summary>
        IReadOnlyDictionary<string, string> Properties { get; }

        /// <summary>
        /// Child objects
        /// </summary>
        IReadOnlyCollection<IEcfChild> Children { get; }
    }

    public class EcfFile
    {
        private EcfObject _currentObject = null;
        private EcfChild _currentChild = null;

        public string FilePath { get; }
        public string FileName => Path.GetFileName(FilePath);

        public EcfFile(string filePath)
        {
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));

            if (!File.Exists(FilePath))
                throw new FileNotFoundException("ECF file not found", FilePath);
        }

        public IEnumerable<IEcfObject> ParseObjects()
        {
            var lines = ReadFile(FilePath);

            foreach (var line in lines.Select(CleanLine).Where(l => l != null))
            {
                if (ReadStart(line))
                    continue;

                if (ReadEnd(line, out IEcfObject obj))
                {
                    if (obj != null)
                        yield return obj;

                    continue;
                }
                
                if(ReadProperty(line))
                    continue;

                throw new FormatException("Failed to parse line");
            }
        }

        private static string[] ReadFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("ECF file not found", filePath);

            var contents = File.ReadAllText(filePath);

            // Remove comments: /* */, #, @, =
            // Why is @ a valid comment for these files?!
            contents = Regex.Replace(
                contents,
                @"(?:/\*[^*]*\*+(?:[^/*][^*]*\*+)*)/|(?:^[\s]*[#|@|=].+$)|(?:^//.+$)",
                "",
                RegexOptions.Multiline | RegexOptions.Compiled);

            return contents.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        }

        /// <summary>
        /// Returns a cleaned line or null
        /// </summary>
        private static string CleanLine(string input)
        {
            var output = input.Trim();

            // ignore comments
            var comments = new[] { "#", "@", "=", "/*", "//" };
            if (comments.Any(x => output.StartsWith(x)))
                return null;

            // remove end of line comments
            var parts = output.Split(new[] { "#", "@", "//" }, StringSplitOptions.None);
            
            return !string.IsNullOrWhiteSpace(parts[0]) ? parts[0] : null;
        }

        /// <summary>
        /// Reads an object start or returns false
        /// </summary>
        /// <exception cref="FormatException">Thrown when line is invalid</exception>
        private bool ReadStart(string line)
        {
            if (line[0] != '{')
                return false;

            // Break up `{ Type Prop1: Value, Prop2: Value`
            // Or `{ Child Unit2`
            var parts = line.Split(' ', '\t');

            if (parts.Length < 3)
                throw new FormatException("Start of object is invalid");

            var type = parts[1];
            var extra = string.Join(" ", parts.Skip(2));

            if(type == "Child")
            {
                if (_currentChild != null)
                    throw new FormatException("Can't start new child, one is already being processed");

                _currentChild = new EcfChild(extra);
            }
            else
            {
                if (_currentObject != null)
                    throw new FormatException("Can't start new object, one is already being processed");

                var fields = ReadFields(extra).ToDictionary(x => x.Key, x => x.Value);
                _currentObject = new EcfObject(type, fields);
            }

            return true;
        }

        private static IEnumerable<KeyValuePair<string, string>> ReadFields(string text)
        {
            var entries = text.SplitWithQuotes(',');
            foreach (var entry in entries)
            {
                if (SplitEntry(entry, out string key, out string value))
                    yield return new KeyValuePair<string, string>(key, value);
            }
        }

        private static bool SplitEntry(string text, out string key, out string value)
        {
            var parts = text.Split(':');

            if (parts.Length < 2)
                throw new FormatException("Object entry is invalid");

            key = parts[0].Trim();
            value = string.Join(":", parts.Skip(1)).Trim();

            return true;
        }

        /// <summary>
        /// Reads the end of an object or child. When a child is read <paramref name="obj">obj/paramref> is null.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool ReadEnd(string line, out IEcfObject obj)
        {
            obj = null;

            if (line[0] != '}')
            {
                return false;
            }

            if(_currentObject == null)
                throw new FormatException("Found end of object before start");

            if(_currentChild != null)
            {
                _currentObject.AddChild(_currentChild);
                _currentChild = null;
            }
            else
            {
                obj = _currentObject;
                _currentObject = null;
            }

            return true;
        }

        /// <summary>
        /// Reads an object entry or returns false
        /// </summary>
        /// <exception cref="FormatException">Thrown when the line is invalid</exception>
        private bool ReadProperty(string line)
        {
            if (!SplitEntry(line, out string key, out string value))
                return false;

            if (_currentChild != null)
                _currentChild.AddProperty(key, value);
            else
                _currentObject.AddProperty(key, value);
            
            return true;
        }

        private class EcfChild : IEcfChild
        {
            private readonly Dictionary<string, string> _entries;

            public string Name { get; }
            public IReadOnlyDictionary<string, string> Properties => _entries;

            public EcfChild(string name)
            {
                Name = name;
                _entries = new Dictionary<string, string>();
            }

            public void AddProperty(string key, string value)
            {
                _entries.Add(key, value);
            }
        }

        private class EcfObject : IEcfObject
        {
            private readonly Dictionary<string, string> _entries;
            private readonly List<IEcfChild> _children;

            public string Type { get; }
            public IReadOnlyDictionary<string, string> Fields { get; }
            public IReadOnlyDictionary<string, string> Properties => _entries;
            public IReadOnlyCollection<IEcfChild> Children => _children;

            public EcfObject(string type, IReadOnlyDictionary<string, string> fields)
            {
                Type = type;
                Fields = fields;
                _entries = new Dictionary<string, string>();
                _children = new List<IEcfChild>();
            }

            public void AddProperty(string key, string value)
            {
                _entries.Add(key, value);
            }

            public void AddChild(IEcfChild child)
            {
                _children.Add(child);
            }
        }
    }
}