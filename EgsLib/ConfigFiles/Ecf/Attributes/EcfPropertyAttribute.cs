using System;
using System.Runtime.CompilerServices;

namespace EgsLib.ConfigFiles.Ecf.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    internal class EcfPropertyAttribute : ConverterAttribute
    {
        public string Name { get; }

        public EcfPropertyAttribute([CallerMemberName] string name = null, Type converterType = null, string converterFunction = null)
            : base(converterType, converterFunction)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }
    }
}
