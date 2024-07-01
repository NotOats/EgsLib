using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using EgsLib.Playfields.Files.Types;

namespace EgsLib.Playfields.Files
{
    public class PlayfieldDynamic : BasePlayfieldFile<PlayfieldDynamicYaml>, IPlayfieldFile
    {
        public PlayfieldDynamic(string file)
            : base(file)
        {
            if (Path.GetFileName(file) != "playfield_dynamic.yaml")
                throw new NotSupportedException("supplied file is not playfield_dynamic.yaml");
        }
    }
}
