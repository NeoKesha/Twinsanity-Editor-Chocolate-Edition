﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twinsanity.TwinsanityInterchange.Enumerations;
using Twinsanity.TwinsanityInterchange.Interfaces;

namespace Twinsanity.TwinsanityInterchange.Common.CameraSubtypes
{
    public class CameraSub1C0D : CameraSubBase
    {
        public Vector4[] BoundingBox { get; private set; }
        public Single UnkFloat3 { get; set; }
        public Single UnkFloat4 { get; set; }
        public CameraSub1C0D()
        {
            BoundingBox = new Vector4[2];
            for (int i = 0; i < BoundingBox.Length; ++i)
            {
                BoundingBox[i] = new Vector4();
            }
        }
        public new int GetLength()
        {
            return base.GetLength() + 8 + BoundingBox.Length * Constants.SIZE_VECTOR4;
        }

        public new void Read(BinaryReader reader, int length)
        {
            base.Read(reader, base.GetLength());
            for (int i = 0; i < BoundingBox.Length; ++i)
            {
                BoundingBox[i].Read(reader, Constants.SIZE_VECTOR4);
            }
            UnkFloat3 = reader.ReadSingle();
            UnkFloat4 = reader.ReadSingle();
        }

        public new void Write(BinaryWriter writer)
        {
            base.Write(writer);
            for (int i = 0; i < BoundingBox.Length; ++i)
            {
                BoundingBox[i].Write(writer);
            }
            writer.Write(UnkFloat3);
            writer.Write(UnkFloat4);
        }
    }
}
