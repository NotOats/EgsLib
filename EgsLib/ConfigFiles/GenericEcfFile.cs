using System;
using System.Collections.Generic;
using System.Linq;

namespace EgsLib.ConfigFiles
{
    public class GenericEcfObject
    {        
        public string Type { get; set; }
        public string Identifier { get; }
        public IReadOnlyDictionary<string, string> Entries { get; }
        public IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> Children { get; }

        internal GenericEcfObject(
            string type, string identifier, 
            Dictionary<string, string> entries, 
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> children)
        {
            Type = type;
            Identifier = identifier;
            Entries = entries;
            Children = children;
        }
    }

    public class GenericEcfFile : BaseEcfFile
    {
        private Dictionary<string, TempObject> _tempObjects = new Dictionary<string, TempObject>();
        public IReadOnlyCollection<GenericEcfObject> Objects { get; }

        public GenericEcfFile(string filePath) : base(filePath)
        {
            Objects = _tempObjects.Select(x => x.Value.Convert()).ToList();
        }

        protected override void HandleChildEntry(string objectIdentifier, string childIdentifier, string key, string value)
        {
            if (_tempObjects.TryGetValue(objectIdentifier, out TempObject obj)
                && obj.Children.TryGetValue(childIdentifier, out Dictionary<string, string> dict))
            {
                dict.Add(key, value);
            }
            else
            {
                throw new Exception("Invalid child entry");
            }
        }

        protected override void HandleEntry(string identifier, string key, string value)
        {
            if(_tempObjects.TryGetValue(identifier, out TempObject obj))
                obj.Entries.Add(key, value);
            else
                throw new Exception("Invalid entry");
        }

        protected override void HandleNewChild(string objectIdentifier, string childIdentifier)
        {
            if(_tempObjects.TryGetValue(objectIdentifier, out TempObject obj))
                obj.Children.Add(childIdentifier, new Dictionary<string, string>());
            else
                throw new Exception("Invalid child object");
        }

        protected override void HandleNewObject(string type, string identifier)
        {
            if (_tempObjects.ContainsKey(identifier))
            {
                Console.WriteLine($"{FileName} has duplicate entry {identifier}");
                _tempObjects[identifier] = new TempObject(type, identifier);
            }
            else
            {
                _tempObjects.Add(identifier, new TempObject(type, identifier));
            }

        }

        private class TempObject
        {
            public string Type { get; set; }
            public string Identifier { get; set; }
            public Dictionary<string, string> Entries { get; set; }
            public Dictionary<string, Dictionary<string, string>> Children { get; set; }

            public TempObject(string type, string identifier)
            {
                Type = type;
                Identifier = identifier;
                Entries = new Dictionary<string, string>();
                Children = new Dictionary<string, Dictionary<string, string>>();
            }

            public GenericEcfObject Convert()
            {
                return new GenericEcfObject(
                    Type,
                    Identifier,
                    Entries,
                    Children.ToDictionary(x => x.Key, x => x.Value as IReadOnlyDictionary<string, string>));
            }
        }
    }
}
