using EgsLib.Playfields.Files.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EgsLib.Playfields.Files
{
    public class SpaceDynamic : BasePlayfieldFile<PlayfieldSpaceYaml>, IPlayfieldFile
    {
        public SpaceDynamic(string file)
            : base(file)
        {
            if (Path.GetFileName(file) != "space_dynamic.yaml")
                throw new NotSupportedException("supplied file is not space_dynamic.yaml");
        }
    }
}
