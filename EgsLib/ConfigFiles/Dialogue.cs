using EgsLib.ConfigFiles.Ecf;
using EgsLib.ConfigFiles.Ecf.Attributes;
using EgsLib.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EgsLib.ConfigFiles
{
    [EcfObject("+Dialogue")]
    public class Dialogue : BaseConfig<Dialogue>
    {
        [EcfField] public string Name { get; private set; }
        
        [EcfProperty("NPCName")]
        public string NpcName { get; private set; }

        [EcfProperty] public string Output {  get; private set; }

        [EcfProperty] public string BarkingState { get; private set; }

        [EcfProperty] public string Globals { get; private set; }

        [EcfProperty] public string Input { get; private set; }

        [EcfProperty] public string Functions { get; private set; }

        public IReadOnlyList<IDialogVariable> Variables { get; private set; }

        public IReadOnlyList<IDialogOption> Options { get; private set; }

        public IReadOnlyList<IDialogNext> Next { get; private set; }

        public IReadOnlyList<string> Execute { get; private set; }

        public Dialogue(IEcfObject obj) : base(obj)
        {
            Variables = MapDialogVariables();
            Options = MapDialogOptions();
            Next = MapDialogNexts();

            Execute = ParseExecute();
        }

        public override string ToString()
        {
            return Name;
        }

        public static IEnumerable<Dialogue> ReadFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("ECF file not found", filePath);

            var ecf = new EcfFile(filePath);
            return ecf.ParseObjects().Select(obj => new Dialogue(obj));
        }

        private IReadOnlyList<string> ParseExecute()
        {
            var properties = UnparsedProperties
                .Where(kvp => kvp.Key.StartsWith("Execute"))
                .ToDictionary(x => x.Key, x => x.Value);

            return properties.Select(kvp =>
            {
                MarkAsParsed(kvp.Key);
                return kvp.Value;
            }).ToArray();
        }

        private IReadOnlyList<IDialogVariable> MapDialogVariables()
        {
            return MapParameters<DialogVariable>("Variable", (variable, name, value) =>
            {
                var index = value.IndexOf(',');
                if (index == -1)
                    return false;

                variable.Name = value.Substring(0, index).Trim('"', ' ');
                variable.Parameter = value.Substring(index + 1).Trim('"', ' ');

                return true;
            });
        }

        private IReadOnlyList<IDialogOption> MapDialogOptions()
        {
            return MapParameters<DialogOption>("Option", (option, name, value) =>
            {
                switch (name)
                {
                    case "Option":
                        option.Text = value;
                        return true;
                    case "OptionIf":
                        option.Conditional = value;
                        return true;
                    case "OptionNext":
                        option.Next = value;
                        return true;
                    case "OptionExecute":
                        option.Execute = value;
                        return true;
                    default:
                        return false;
                }
            });
        }

        private IReadOnlyList<IDialogNext> MapDialogNexts()
        {
            return MapParameters<DialogNext>("Next", (next, name, value) =>
            {
                switch (name)
                {
                    case "Next":
                        next.Dialog = value;
                        return true;
                    case "NextIf":
                        next.Conditional = value;
                        return true;
                    default:
                        return false;
                }
            });
        }

        /// <summary>
        /// Parses properties into a list of formatted objects.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="startsWith">Property name fragment to look for</param>
        /// <param name="handler">
        /// Handle passes the object (mapped to id), property name, and property value. 
        /// Returns if handling was successful.
        /// </param>
        /// <returns></returns>
        private IReadOnlyList<T> MapParameters<T>(string startsWith, 
            Func<T, string, string, bool> handler) where T : IValidatable, new()
        {
            var results = new Dictionary<int, T>();
            var parsed = new List<string>();

            foreach(var kvp in UnparsedProperties.Where(kvp => kvp.Key.StartsWith(startsWith)))
            {
                if(!SplitProperty(kvp.Key, out string name, out int index) || string.IsNullOrWhiteSpace(name))
                    throw new FormatException("Dialog parameter is malformed");

                if (!results.ContainsKey(index))
                    results.Add(index, new T());

                if(handler(results[index], name, kvp.Value))
                    parsed.Add(kvp.Key);
            }

            foreach(var name in parsed)
            {
                MarkAsParsed(name);
            }

            return results
                .Select(kvp => kvp.Value)
                .Where(obj => obj.Validate())
                .ToArray();
        }

        private static bool SplitProperty(string input, out string name, out int index)
        {
            var parts = input.Split('_');
            if (parts.Length == 2 && parts[1].ConvertType(out index))
            {
                name = parts[0];
                index -= 1; // Fix indexing to 0
                return true;
            }

            name = null;
            index = 0;
            return false;
        }


        #region Property Interfaces
        public interface IDialogVariable
        {
            string Name { get; }
            string Parameter { get; }
        }

        public interface IDialogOption
        {
            string Text { get; }
            string Next { get; }
            string Conditional { get; }
            string Execute { get; }
        }

        public interface IDialogNext
        {
            string Dialog { get; }
            string Conditional { get; }
        }
        
        private interface IValidatable
        {
            bool Validate();
        }
        #endregion

        #region Property Classes
        private class DialogVariable : IDialogVariable, IValidatable
        {
            public string Name { get; set; }
            public string Parameter { get; set; }

            public override string ToString()
            {
                return $"{Name}({Parameter})";
            }

            public bool Validate()
            {
                return !string.IsNullOrWhiteSpace(Name)
                    && !string.IsNullOrWhiteSpace(Parameter);
            }
        }

        private class DialogOption : IDialogOption, IValidatable
        {
            public string Text { get; set; }
            public string Next { get; set;  }
            public string Conditional { get; set; }
            public string Execute { get; set; }

            public bool Validate()
            {
                return !string.IsNullOrWhiteSpace(Text) 
                    && !string.IsNullOrWhiteSpace(Next);
            }
        }

        private class DialogNext : IDialogNext, IValidatable
        {
            public string Dialog { get; set; }
            public string Conditional { get; set; }

            public bool Validate()
            {
                return !string.IsNullOrWhiteSpace(Dialog);
            }
        }
        #endregion
    }
}
