﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twinsanity.TwinsanityInterchange.Interfaces.Items.RM.Code;

namespace Twinsanity.TwinsanityInterchange.Implementations.PS2.Items.RM2.Code
{
    public class PS2AnySound : ITwinSound
    {
        UInt32 id;
        internal Int32 offset;

        public UInt32 Header;
        public Byte UnkFlag;
        public Byte FreqFac;
        public UInt16 Param1;
        public UInt16 Param2;
        public UInt16 Param3;
        public UInt16 Param4;
        public Byte[] Sound;

        public UInt32 GetID()
        {
            return id;
        }

        public Int32 GetLength()
        {
            return 22;
        }

        public void Read(BinaryReader reader, Int32 length)
        {
            Header = reader.ReadUInt32();
            UnkFlag = reader.ReadByte();
            FreqFac = reader.ReadByte();
            Param1 = reader.ReadUInt16();
            Param2 = reader.ReadUInt16();
            Param3 = reader.ReadUInt16();
            Param4 = reader.ReadUInt16();
            Sound = new Byte[reader.ReadUInt32()];
            reader.ReadUInt32(); // Discard offset
        }

        public void SetID(UInt32 id)
        {
            this.id = id;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Header);
            writer.Write(UnkFlag);
            writer.Write(FreqFac);
            writer.Write(Param1);
            writer.Write(Param2);
            writer.Write(Param3);
            writer.Write(Param4);
            writer.Write(Sound.Length);
            writer.Write(offset);
        }
    }
}