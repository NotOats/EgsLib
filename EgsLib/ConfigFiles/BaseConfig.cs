using EgsLib.ConfigFiles.Ecf;
using EgsLib.ConfigFiles.Ecf.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EgsLib.ConfigFiles
{
    public interface IBaseConfig
    {
        IReadOnlyDictionary<string, string> UnparsedProperties { get; }
        IReadOnlyCollection<IEcfChild> UnparsedChildren { get; }
    }

    public abstract class BaseConfig<TDerived> : IBaseConfig 
        where TDerived : BaseConfig<TDerived>
    {
        private static readonly IReadOnlyDictionary<PropertyInfo, EcfFieldAttribute> EcfFieldCache;
        private static readonly IReadOnlyDictionary<PropertyInfo, EcfPropertyAttribute> EcfPropertyCache;
        private static readonly EcfObjectAttribute EcfObjAttribute;

        static BaseConfig()
        {
            EcfObjAttribute = typeof(TDerived).GetCustomAttribute<EcfObjectAttribute>()
                ?? throw new Exception("EcfObjectAttribute must be defined when inheriting BaseConfig");

            EcfFieldCache = EcfFieldAttribute.ReadFields<TDerived>();
            EcfPropertyCache = EcfPropertyAttribute.ReadProperties<TDerived>();
        }

        private readonly Dictionary<string, string> _unparsed = new Dictionary<string, string>();

        public IReadOnlyDictionary<string, string> UnparsedProperties => _unparsed;

        public IReadOnlyCollection<IEcfChild> UnparsedChildren { get; }

        protected BaseConfig(IEcfObject obj)
        {
            // Validate type
            if (!EcfObjAttribute.Types.Contains(obj.Type))
                throw new ArgumentException($"IEcfObject is not of type {string.Join(", ", EcfObjAttribute.Types)}", nameof(obj));

            SetFields(obj);
            _unparsed = SetProperties(obj);

            // TODO: Figure out child object parsing
            // For now let the inheriting class figure it out
            UnparsedChildren = obj.Children;
        }

        protected bool MarkAsParsed(string name)
        {
            return _unparsed.Remove(name);
        }

        private void SetFields(IEcfObject obj)
        {
            var unparsed = obj.Fields.Select(x => x.Key).ToList();

            foreach (var kvp in EcfFieldCache)
            {
                var prop = kvp.Key;
                var attr = kvp.Value;

                if(attr.Converter != null)
                {
                    if (!obj.Fields.TryGetValue(attr.Name, out string value))
                        continue;

                    if (!attr.Converter(value, out object output, prop.PropertyType))
                        continue;

                    prop.SetValue(this, output);
                    unparsed.Remove(attr.Name);
                }
                else if(obj.ReadField(attr.Name, out object output, prop.PropertyType))
                {
                    prop.SetValue(this, output);
                    unparsed.Remove(attr.Name);
                }
            }

            if (unparsed.Count != 0)
                throw new Exception($"IEcfObject contains unused fields: {string.Join(", ", unparsed)}");
        }

        private Dictionary<string, string> SetProperties(IEcfObject obj)
        {
            var unparsed = obj.Properties.ToDictionary(x => x.Key, x => x.Value);

            foreach (var kvp in EcfPropertyCache)
            {
                var prop = kvp.Key;
                var attr = kvp.Value;

                if (attr.Converter != null)
                {
                    // Read value
                    if (!obj.Properties.TryGetValue(attr.Name, out string value))
                        continue;

                    // Process if not blank
                    if(!string.IsNullOrWhiteSpace(value))
                    {
                        if (!attr.Converter(value, out object output, prop.PropertyType))
                            continue;

                        prop.SetValue(this, output);
                    }

                    unparsed.Remove(attr.Name);
                }
                else if (obj.ReadProperty(attr.Name, out object output, prop.PropertyType))
                {
                    prop.SetValue(this, output);
                    unparsed.Remove(attr.Name);
                }
            }

            return unparsed;
        }
    }
}
