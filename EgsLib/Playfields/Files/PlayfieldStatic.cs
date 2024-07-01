using EgsLib.Playfields.Files.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EgsLib.Playfields.Files
{
    public class PlayfieldStatic : BasePlayfieldFile<PlayfieldYaml>, IPlayfieldFile
    {
        public PlayfieldStatic(string file)
            : base(file)
        {
            if (Path.GetFileName(file) != "playfield_static.yaml")
                throw new NotSupportedException("supplied file is not playfield_static.yaml");
        }
    }
}
