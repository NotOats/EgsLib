using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace EgsLib.ConfigFiles
{
    public abstract class BaseEcfFile
    {
        public string FilePath { get; }
        public string FileName => Path.GetFileName(FilePath);

        protected BaseEcfFile(string filePath)
        {
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));

            if (!File.Exists(FilePath))
                throw new FileNotFoundException("ECF file not found", FilePath);

            //var lines = File.ReadAllLines(FilePath);
            var lines = ReadFile(FilePath);
            Parse(lines);
        }

        protected abstract void HandleNewObject(string type, string identifier);
        protected abstract void HandleEntry(string identifier, string key, string value);

        protected abstract void HandleNewChild(string objectIdentifier, string childIdentifier);
        protected abstract void HandleChildEntry(string objectIdentifier, string childIdentifier, string key, string value);

        private static string[] ReadFile(string filePath)
        {
            var contents = File.ReadAllText(filePath);

            // Remove comments: /* */, #, @
            // Why is @ a valid comment for these files?!
            contents = Regex.Replace(
                contents, 
                @"(?:/\*[^*]*\*+(?:[^/*][^*]*\*+)*)/|(?:^[\s]*[#|@|=].+$)|(?:^//.+$)", 
                "", 
                RegexOptions.Multiline | RegexOptions.Compiled);

            return contents.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        }

        private void Parse(string[] lines)
        {
            string entryName = null;
            string childName = null;

            foreach(var line in lines.Select(CleanLine).Where(l => l != null))
            {
                // start of a new object or child
                if(ReadObjectStart(line, out string type, out string identifier))
                {
                    identifier = identifier.Trim();

                    if(entryName == null)
                    {
                        entryName = identifier;
                        HandleNewObject(type, identifier);
                    }
                    else if(type == "Child")
                    {
                        childName = identifier;
                        HandleNewChild(entryName, identifier);
                    } else
                    {
                        throw new FormatException("Start is not a new object or child");
                    }

                    continue;
                }

                // read end of object or child
                if (line[0] == '}')
                {
                    if (entryName == null)
                        throw new FormatException("Found end of object before start");

                    if (childName != null)
                        childName = null;
                    else
                        entryName = null;

                    continue;
                }

                // read entry
                if(ReadObjectEntry(line, out string key, out string value))
                {
                    if (entryName == null)
                        throw new FormatException("Found entry before start of object");

                    value = value.Trim();

                    if(childName != null)
                        HandleChildEntry(entryName, childName, key, value);
                    else
                        HandleEntry(entryName, key, value);

                    continue;
                }

                throw new FormatException("Failed to parse line");
            }
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
            var parts = output.Split(comments, StringSplitOptions.None);
            
            return !string.IsNullOrWhiteSpace(parts[0]) ? parts[0] : null;
        }

        /// <summary>
        /// Reads an object start or returns false
        /// </summary>
        /// <exception cref="FormatException">Thrown when line is invalid</exception>
        private static bool ReadObjectStart(string line, out string type, out string identifier)
        {
            if (line[0] != '{')
            {
                type = string.Empty;
                identifier = string.Empty;
                return false;
            }

            var parts = line.Split(' ', '\t');

            if (parts.Length < 3)
                throw new FormatException("Start of object is invalid");

            type = parts[1];
            identifier = string.Join(" ", parts.Skip(2));

            return true;
        }

        /// <summary>
        /// Reads an object entry or returns false
        /// </summary>
        /// <exception cref="FormatException">Thrown when the line is invalid</exception>
        private static bool ReadObjectEntry(string line, out string key, out string value)
        {
            var parts = line.Split(':');

            if (parts.Length < 2)
                throw new FormatException("Object entry is invalid");

            key = parts[0];
            value = string.Join(":", parts.Skip(1));
            
            return true;
        }
    }
}