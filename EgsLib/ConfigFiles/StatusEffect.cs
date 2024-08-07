﻿using EgsLib.ConfigFiles.Ecf;
using EgsLib.ConfigFiles.Ecf.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EgsLib.ConfigFiles
{
    public class StatusEffectModifier
    {
        public string Stat { get; }
        public float? Amount { get; }
        public float? Rate { get; }
        public float? ModifyMaxValue { get; }
        public string NumberOperationType { get; }
        public float? SetValue { get; }
        public string ModifierType { get; }

        internal StatusEffectModifier(IEcfChild child)
        {
            if (child.ReadProperty("Stat", out string stat))
                Stat = stat;

            if (child.ReadProperty("Amount", out float amount))
                Amount = amount;

            if (child.ReadProperty("Rate", out float rate))
                Rate = rate;

            if (child.ReadProperty("ModifyMaxValue", out float modifyMaxValue))
                ModifyMaxValue = modifyMaxValue;

            if (child.ReadProperty("SetValue", out float setValue))
                SetValue = setValue;

            if (child.ReadProperty("ModifierType", out string modifierType))
                ModifierType = modifierType;
        }
    }

    [EcfObject("StatusEffect")]
    public class StatusEffect : BaseConfig<StatusEffect>
    {
        [EcfField] public string Name { get; private set; }

        [EcfProperty] public float? Duration { get; private set; }

        [EcfProperty] public string BuffIf { get; private set; }

        [EcfProperty] public string DebuffIf { get; private set; }

        [EcfProperty] public string Mutex { get; private set; }

        [EcfProperty] public string DebuffActions { get; private set; }

        [EcfProperty] public string Actions { get; private set; }

        [EcfProperty] public string OnExpired { get; private set; }

        [EcfProperty] public bool? NextIsWorse { get; private set; }

        [EcfProperty] public string Evolves { get; private set; }

        [EcfProperty] public string CastSound { get; private set; }

        [EcfProperty("OnDebuffSound")]
        public string DebuffSound { get; private set; }

        [EcfProperty("OnExpiredSound")]
        public string ExpiredSound { get; private set; }

        [EcfProperty("Type")]
        public string EffectType { get; private set; }

        [EcfProperty] public bool? RequiresAll { get; private set; }

        [EcfProperty("Requires", typeof(StatusEffect), "ParseRequires")]
        public IReadOnlyCollection<string> Requires { get; private set; }

        [EcfProperty] public string Stack { get; private set; }

        [EcfProperty] public string Description { get; private set; }

        [EcfProperty] public string Icon { get; private set; }

        public IReadOnlyList<StatusEffectModifier> Modifiers { get; }

        public StatusEffect(IEcfObject obj) : base(obj)
        {
            Modifiers = UnparsedChildren
                .Select(child => new StatusEffectModifier(child))
                .ToArray();

            // Weirdly this object has a name field and property, they seem to be the same value.
            MarkAsParsed("Name");
        }

        public static IEnumerable<StatusEffect> ReadFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("ECF file not found", filePath);

            var ecf = new EcfFile(filePath);
            return ecf.ParseObjects().Select(obj => new StatusEffect(obj));
        }

        private static bool ParseRequires(string input, out object output, Type type)
        {
            if (!string.IsNullOrWhiteSpace(input))
                output = input.Split(',');
            else
                output = Array.Empty<string>();

            return true;
        }
    }
}
