using EgsLib.ConfigFiles.Ecf;
using EgsLib.ConfigFiles.Ecf.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EgsLib.ConfigFiles
{
    public abstract class BaseConfig
    {
        public IReadOnlyDictionary<string, string> UnparsedProperties { get; }

        public IReadOnlyCollection<IEcfChild> UnparsedChildren { get; }

        protected BaseConfig(IEcfObject obj)
        {
            // Validate type
            var attribute = GetType().GetCustomAttribute<EcfObjectAttribute>()
                ?? throw new Exception("EcfObjectAttribute must be defined when inheriting BaseConfig");

            if (!attribute.Types.Contains(obj.Type))
                throw new ArgumentException($"IEcfObject is not of type {string.Join(", ", attribute.Types)}", nameof(obj));

            // Read fields & properties
            var fields = ReadProperties<EcfFieldAttribute>();
            SetFields(obj, fields);
            ThrowIfUnusedFields(obj, fields);

            var properties = ReadProperties<EcfPropertyAttribute>();
            SetProperties(obj, properties);

            UnparsedProperties = FindUnusedProperties(obj, properties);

            // TODO: Figure out child object parsing
            // For now let the inheriting class figure it out
            UnparsedChildren = obj.Children;
        }

        private Dictionary<PropertyInfo, TEcfAttribute> ReadProperties<TEcfAttribute>() where TEcfAttribute : ConverterAttribute
        {
            var result = new Dictionary<PropertyInfo, TEcfAttribute>();

            foreach(var property in GetType().GetProperties())
            {
                var attr = property.GetCustomAttribute<TEcfAttribute>();
                if (attr == null)
                    continue;

                if (!property.CanWrite)
                    throw new Exception("Ecf Field/Property does not have a setter");

                result.Add(property, attr);
            }

            return result;
        }

        private void SetFields(IEcfObject obj, 
            IReadOnlyDictionary<PropertyInfo, EcfFieldAttribute> fields)
        {
            foreach(var kvp in fields)
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
                }
                else if(obj.ReadField(attr.Name, out object output, prop.PropertyType))
                {
                    prop.SetValue(this, output);
                }
            }
        }

        private void SetProperties(IEcfObject obj, 
            IReadOnlyDictionary<PropertyInfo, EcfPropertyAttribute> properties)
        {
            foreach (var kvp in properties)
            {
                var prop = kvp.Key;
                var attr = kvp.Value;

                if (attr.Converter != null)
                {
                    if (!obj.Properties.TryGetValue(attr.Name, out string value))
                        continue;

                    if (!attr.Converter(value, out object output, prop.PropertyType))
                        continue;

                    prop.SetValue(this, output);
                }
                else if (obj.ReadProperty(attr.Name, out object output, prop.PropertyType))
                {
                    prop.SetValue(this, output);
                }
            }
        }

        private static void ThrowIfUnusedFields(IEcfObject obj, IReadOnlyDictionary<PropertyInfo, EcfFieldAttribute> fields)
        {
            bool IsInParsedFields(string name)
            {
                return fields.Any(kvp => kvp.Value.Name == name);
            }

            var unusedFields = obj.Fields
                .Where(field => !IsInParsedFields(field.Key))
                .Select(field => field.Key)
                .ToList();

            if (unusedFields.Count != 0)
                throw new Exception($"IEcfObject contains unused fields: {string.Join(", ", unusedFields)}");
        }

        private static IReadOnlyDictionary<string, string> FindUnusedProperties(
            IEcfObject obj, IReadOnlyDictionary<PropertyInfo, EcfPropertyAttribute> properties)
        {
            bool IsInParsedProperties(string name)
            {
                return properties.Any(kvp => kvp.Value.Name == name);
            }

            return obj.Properties
                .Where(prop => !IsInParsedProperties(prop.Key))
                .ToDictionary(x => x.Key, x => x.Value);
        }
    }
}
