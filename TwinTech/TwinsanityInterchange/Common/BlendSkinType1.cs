﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twinsanity.TwinsanityInterchange.Enumerations;
using Twinsanity.TwinsanityInterchange.Interfaces;

namespace Twinsanity.TwinsanityInterchange.Common
{
    public class BlendSkinType1 : ITwinSerializable
    {
        public Int32 ListLength { set; get; }
        public Int32 UnkInt;
        public Byte[] VifCode;
        public Byte[] UnkData;
        public List<Byte[]> UnkBlobs;
        public List<Int32> UnkInts;

        public BlendSkinType1()
        {
            UnkBlobs = new List<Byte[]>();
            UnkInts = new List<Int32>();
        }

        public int GetLength()
        {
            return 20 + VifCode.Length + UnkInts.Count * Constants.SIZE_UINT32 + UnkBlobs.Sum((blob) => blob.Length) + UnkBlobs.Count * Constants.SIZE_UINT32;
        }

        public void Read(BinaryReader reader, int length)
        {
            var blobLen = reader.ReadInt32();
            UnkInt = reader.ReadInt32();
            VifCode = reader.ReadBytes(blobLen);
            UnkData = reader.ReadBytes(0xC);
            for (int i = 0; i < ListLength; ++i)
            {
                var blobSize = reader.ReadInt32();
                UnkInts.Add(reader.ReadInt32());
                UnkBlobs.Add(reader.ReadBytes(blobSize << 4));
            }
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(VifCode.Length);
            writer.Write(UnkInt);
            writer.Write(VifCode);
            writer.Write(UnkData);
            for (int i = 0; i < ListLength; ++i)
            {
                writer.Write(UnkBlobs[i].Length >> 4);
                writer.Write(UnkInts[i]);
                writer.Write(UnkBlobs[i]);
            }
        }
    }
}
