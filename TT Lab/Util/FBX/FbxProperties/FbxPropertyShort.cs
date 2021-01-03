﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TT_Lab.Util.FBX.FbxProperties
{
    public class FbxPropertyShort : FbxProperty
    {
        public override UInt32 GetLength()
        {
            return 3;
        }
        public override void ReadBinary(BinaryReader reader)
        {
            Value = reader.ReadInt16();
        }
        public override void SaveBinary(BinaryWriter writer)
        {
            writer.Write('Y');
            writer.Write((Int16)Value);
        }
        public override String ToString() { return "Property: Short"; }
    }
}
