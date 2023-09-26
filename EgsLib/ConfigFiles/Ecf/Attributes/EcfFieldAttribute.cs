﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace EgsLib.ConfigFiles.Ecf.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    internal class EcfFieldAttribute : ConverterAttribute
    {
        public string Name { get; }

        public EcfFieldAttribute([CallerMemberName] string name = null, Type converterType = null, string converterFunction = null)
            : base(converterType, converterFunction)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public static IReadOnlyDictionary<PropertyInfo, EcfFieldAttribute> ReadFields<TObject>()
        {
            return ReadProperties<TObject, EcfFieldAttribute>();
        }
    }
}