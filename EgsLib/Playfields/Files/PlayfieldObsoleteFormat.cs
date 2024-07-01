using EgsLib.Playfields.Files.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EgsLib.Playfields.Files
{
    public class PlayfieldObsoleteFormat : BasePlayfieldFile<PlayfieldYaml>, IPlayfieldFile
    {
        public PlayfieldObsoleteFormat(string file)
            : base(file)
        {
            if (Path.GetFileName(file) != "playfield.yaml")
                throw new NotSupportedException("supplied file is not playfield_static.yaml");
        }
    }
}
