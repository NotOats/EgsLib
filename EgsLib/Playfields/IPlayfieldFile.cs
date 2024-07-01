using System;
using System.Collections.Generic;
using System.Text;

namespace EgsLib.Playfields
{
    public interface IPlayfieldFile
    {
        string Name { get; }
        string File { get; }

        //Dictionary<string, object> Properties { get; }

        //bool TryReadProperty<T>(string name, out T value);
    }
}
