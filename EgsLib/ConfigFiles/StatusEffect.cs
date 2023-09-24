using EgsLib.ConfigFiles.Ecf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EgsLib.ConfigFiles
{
    public class StatusEffectModifier
    {
        public string Stat { get; }
        public int? Amount { get; }
        public int? Rate { get; }
        public int? ModifyMaxValue { get; }
        public string NumberOperationType { get; }
        public float? SetValue { get; }
        public string ModifierType { get; }

        internal StatusEffectModifier(IEcfChild child)
        {
            if(child.ReadProperty("Stat", out string stat))
                Stat = stat;

            if (child.ReadProperty("Amount", out int amount))
                Amount = amount;

            if (child.ReadProperty("Rate", out int rate))
                Rate = rate;

            if (child.ReadProperty("ModifyMaxValue", out int modifyMaxValue))
                ModifyMaxValue = modifyMaxValue;

            if (child.ReadProperty("SetValue", out float setValue))
                SetValue = setValue;

            if (child.ReadProperty("ModifierType", out string modifierType))
                ModifierType = modifierType;
        }
    }

    public class StatusEffect
    {
        public string Name { get; }

        public int? Duration { get; }
        public string Actions { get; }
        public string OnExpired { get; }
        public bool? NextIsWorse { get; }
        public string Evolves { get; }
        public string CastSound { get; }
        public string DebuffSound { get; }
        public string ExpiredSound { get; }
        public string EffectType { get; }
        public bool? RequiresAll { get; }
        public IReadOnlyCollection<string> Requires { get; }
        public string Stack { get; }
        public string Description { get; }
        public string Icon { get; }
        public IReadOnlyList<StatusEffectModifier> Modifiers { get; }

        public StatusEffect(IEcfObject obj)
        {
            // Required
            if (obj.Type != "StatusEffect")
                throw new FormatException("IEcfObject is not a StatusEffect");

            if (!obj.ReadField("Name", out string name))
                throw new FormatException("StatusEffect has no name");

            Name = name;

            // Optional
            if (obj.ReadProperty("Duration", out int duration))
                Duration = duration;

            if (obj.ReadProperty("Actions", out string actions))
                Actions = actions;

            if (obj.ReadProperty("OnExpired", out string onExpired))
                OnExpired = onExpired;

            if (obj.ReadProperty("NextIsWorse", out string nextIsWorse))
                NextIsWorse = nextIsWorse == "true";

            if (obj.ReadProperty("Evolves", out string evolves))
                Evolves = evolves;

            if (obj.ReadProperty("CastSound", out string castSound))
                CastSound = castSound;

            if (obj.ReadProperty("DebuffSound", out string debuffSound))
                DebuffSound = debuffSound;

            if (obj.ReadProperty("ExpiredSound", out string expiredSound))
                ExpiredSound = expiredSound;

            if (obj.ReadProperty("Type", out string type))
                EffectType = type;

            if (obj.ReadProperty("RequiresAll", out string requiresAll))
                RequiresAll = requiresAll == "false";

            if (obj.ReadProperty("Stack", out string stack))
                Stack = stack;

            if (obj.ReadProperty("Description", out string description))
                Description = description;

            if (obj.ReadProperty("Icon", out string icon))
                Icon = icon;


            if (obj.ReadProperty("Requires", out string requires))
                Requires = requires.Split(',');
            else
                Requires = Array.Empty<string>();


            Modifiers = obj.Children
                .Select(child => new StatusEffectModifier(child))
                .ToArray();
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
    }
}
