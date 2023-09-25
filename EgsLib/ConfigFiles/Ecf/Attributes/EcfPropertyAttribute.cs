using System;

namespace EgsLib.ConfigFiles.Ecf.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    internal class EcfPropertyAttribute : ConverterAttribute
    {
        public string Name { get; }

        public EcfPropertyAttribute(string name, Type converterType = null, string converterFunction = null)
            : base(converterType, converterFunction)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }
    }
}
