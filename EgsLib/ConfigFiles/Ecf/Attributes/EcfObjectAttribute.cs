using System;
using System.Collections.Generic;

namespace EgsLib.ConfigFiles.Ecf.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    internal class EcfObjectAttribute : Attribute
    {
        public IReadOnlyCollection<string> Types { get; }

        public EcfObjectAttribute(params string[] types)
        {
            if (types.Length == 0)
                throw new ArgumentException("At least one type is required for an EcfObject", nameof(types));

            Types = types;
        }
    }
}
