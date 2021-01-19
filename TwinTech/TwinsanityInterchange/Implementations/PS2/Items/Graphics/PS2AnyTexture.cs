﻿using System.Text.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Twinsanity.Libraries;
using Twinsanity.PS2Hardware;
using Twinsanity.TwinsanityInterchange.Common;
using Twinsanity.TwinsanityInterchange.Enumerations;
using Twinsanity.TwinsanityInterchange.Implementations.Base;
using Twinsanity.TwinsanityInterchange.Interfaces.Items;

namespace Twinsanity.TwinsanityInterchange.Implementations.PS2.Items.Graphics
{
    public class PS2AnyTexture : BaseTwinItem, ITwinTexture
    {
        static Dictionary<string, KeyValuePair<Int32, Int32>> ResolutionMapper;
        public UInt32 HeaderSignature { get; set; }
        public UInt16 ImageWidthPower { get; set; }
        public UInt16 ImageHeightPower { get; set; }
        public Byte MipLevels { get; set; }
        public TexturePixelFormat TextureFormat { get; set; }
        public TexturePixelFormat DestinationTextureFormat { get; set; }
        public TextureColorComponent ColorComponent;
        public Byte UnkByte;
        public TextureFunction TexFun;
        public Byte[] UnkBytes1;
        public Int32 TextureBasePointer { get; set; }
        public Int32[] MipLevelsTBP { get; set; }
        public Int32 TextureBufferWidth { get; set; }
        public Int32[] MipLevelsTBW { get; set; }
        public Int32 ClutBufferBasePointer { get; set; }
        public Byte[] UnkBytes2;
        public Byte[] UnkBytes3;
        public Byte[] UnusedMetadata {get; set; }
        public Byte[] TextureData { get; set; }

        public PS2AnyTexture()
        {
            if (ResolutionMapper == null)
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                using (FileStream stream = new FileStream(Path.Combine(Path.GetDirectoryName(path), @"TextureResolutionMapper.json"), FileMode.Open, FileAccess.Read))
                using (StreamReader reader = new StreamReader(stream))
                {
                    ResolutionMapper = JsonSerializer.Deserialize<Dictionary<string, KeyValuePair<Int32, Int32>>>(reader.ReadToEnd());
                }
            }
            UnusedMetadata = new byte[32];
            HeaderSignature = 0xbbcccdcd;
            DestinationTextureFormat = TexturePixelFormat.PSMCT32;
            ColorComponent = TextureColorComponent.RGBA;
            UnkByte = 0;
            TextureBasePointer = 0;
            MipLevelsTBP = new int[6];
            TextureBufferWidth = 4;
            MipLevelsTBW = new int[6];
            ClutBufferBasePointer = 0;
            UnkBytes1 = new byte[2];
            UnkBytes2 = new byte[8] { 0, 0, 0, 0, 224, 0, 2, 0 };
            UnkBytes3 = new byte[2] { 0, 2}; 
            UnusedMetadata = new byte[32];
            UnusedMetadata[0] = 31;
            UnusedMetadata[16] = 64;
            UnusedMetadata[17] = 246;
            UnusedMetadata[18] = 89;
            UnusedMetadata[19] = 32;
        }

        public override Int32 GetLength()
        {
            return 4 + 96 + UnusedMetadata.Length + (TextureData != null ? TextureData.Length : 0);
        }

        public override void Read(BinaryReader reader, Int32 length)
        {
            int dataLen = reader.ReadInt32();
            HeaderSignature = reader.ReadUInt32();
            ImageWidthPower = reader.ReadUInt16();
            ImageHeightPower = reader.ReadUInt16();
            MipLevels = reader.ReadByte();
            TextureFormat = (TexturePixelFormat)reader.ReadByte();
            DestinationTextureFormat = (TexturePixelFormat)reader.ReadByte();
            ColorComponent = (TextureColorComponent)reader.ReadByte();
            UnkByte = reader.ReadByte();
            TexFun = (TextureFunction)reader.ReadByte();
            UnkBytes1 = reader.ReadBytes(2);
            TextureBasePointer = reader.ReadInt32();
            MipLevelsTBP = new int[6];
            for (var i = 0; i < 6; ++i)
            {
                MipLevelsTBP[i] = reader.ReadInt32();
            }
            TextureBufferWidth = reader.ReadInt32();
            MipLevelsTBW = new int[6];
            for (var i = 0; i < 6; ++i)
            {
                MipLevelsTBW[i] = reader.ReadInt32();
            }
            ClutBufferBasePointer = reader.ReadInt32();
            UnkBytes2 = reader.ReadBytes(8);
            reader.ReadInt32(); // Reserved
            reader.ReadInt32(); // Reserved
            UnkBytes3 = reader.ReadBytes(2);
            reader.ReadBytes(2); // Reserved
            reader.Read(UnusedMetadata, 0, UnusedMetadata.Length);
            TextureData = reader.ReadBytes(dataLen - 96 - UnusedMetadata.Length);
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(GetLength() - 4);
            writer.Write(HeaderSignature);
            writer.Write(ImageWidthPower);
            writer.Write(ImageHeightPower);
            writer.Write(MipLevels);
            writer.Write((Byte)TextureFormat);
            writer.Write((Byte)DestinationTextureFormat);
            writer.Write((Byte)ColorComponent);
            writer.Write(UnkByte);
            writer.Write((Byte)TexFun);
            writer.Write(UnkBytes1);
            writer.Write(TextureBasePointer);
            for (var i = 0; i < 6; ++i)
            {
                writer.Write(MipLevelsTBP[i]);
            }
            writer.Write(TextureBufferWidth);
            for (var i = 0; i < 6; ++i)
            {
                writer.Write(MipLevelsTBW[i]);
            }
            writer.Write(ClutBufferBasePointer);
            writer.Write(UnkBytes2);
            writer.Write(0); // Reserved
            writer.Write(0); // Reserved
            writer.Write(UnkBytes3);
            writer.Write((Int16)0); // Reserved
            writer.Write(UnusedMetadata);
            writer.Write(TextureData);
        }

        public void CalculateData()
        {
            var interpreter = VIFInterpreter.InterpretCode(TextureData);
            var data = interpreter.GetGifMem();
            Colors.Clear();
            switch (TextureFormat)
            {
                case TexturePixelFormat.PSMCT32:
                    EzSwizzle.TagToColors(data[1], Colors);
                    break;
                case TexturePixelFormat.PSMT8:
                    byte[] gifData = EzSwizzle.TagToBytes(data[1]);
                    int RRW = (int)((data[0].Data[1].Output >> 0) & 0xFFFFFFFF);
                    int RRH = (int)((data[0].Data[1].Output >> 32) & 0xFFFFFFFF);
                    int Width = (int)(Math.Pow(2, ImageWidthPower));
                    int Height = (int)(Math.Pow(2, ImageHeightPower));
                    byte[] rawTextureData = EzSwizzle.writeTexPSMCT32(0, 1, 0, 0, RRW, RRH, gifData);
                    byte[] texData = EzSwizzle.readTexPSMT8(0, TextureBufferWidth, 0, 0, Width, Height, rawTextureData, false);
                    byte[] paletteData = EzSwizzle.readTexPSMCT32(ClutBufferBasePointer, 1, 0, 0, 16, 16, rawTextureData, false);
                    List<Color> palette = EzSwizzle.BytesToColors(paletteData);
                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 8; j < 16; j++)
                        {
                            Color tmp = palette[j + i * 32];
                            palette[j + i * 32] = palette[j + i * 32 + 8];
                            palette[j + i * 32 + 8] = tmp;
                        }
                    }
                    int Pixels = Width * Height;
                    for (var i = 0; i < Pixels; ++i)
                    {
                        Colors.Add(palette[texData[i]]);
                    }
                    break;
            }
        }

        public void FromBitmap(List<Color> image, Int32 width, byte mips, TextureFunction fun, TexturePixelFormat format)
        {
            mips = 1;
            int height = image.Count / width;
            TexFun = fun;
            TextureFormat = format;
            MipLevels = mips;
            TextureBufferWidth = (int)Math.Ceiling(width / 64.0f);
            ImageWidthPower = (ushort)Math.Log2(width);
            ImageHeightPower = (ushort)Math.Log2(height);
            ClutBufferBasePointer = 0;
            //this is probably not bytes but whatever
            UnkBytes2[5] = UnkBytes3[0] = (width == 256) ? 0 : (byte)Math.Min(width, height);
            UnkBytes2[6] = UnkBytes3[1] = (width == 256) ? 2 : 0;
            {
                var mipWidth = width;
                var mipHeight = height;
                var basePointer = 0;
                for (var i = 0; i < MipLevels; ++i)
                {
                    MipLevelsTBP[i] = basePointer;
                    MipLevelsTBW[i] = (Byte)Math.Ceiling(mipWidth / 64.0f);
                    mipWidth /= 2;
                    mipHeight /= 2;
                    ClutBufferBasePointer = basePointer + 4;
                    basePointer += (int)Math.Ceiling(mipWidth * mipHeight / 64.0f);
                }
            }
            GIFTag headerTag = new GIFTag();
            headerTag.REGS = new REGSEnum[16];
            headerTag.REGS[0] = REGSEnum.ApD;
            headerTag.NLOOP = 3;
            headerTag.NREG = 1;
            headerTag.FLG = GIFModeEnum.PACKED;
            headerTag.Data = new List<RegOutput>();
            RegOutput head1 = new RegOutput();
            head1.REG = REGSEnum.ApD;
            head1.Address = 81;
            RegOutput head2 = new RegOutput();
            head2.REG = REGSEnum.ApD;
            head2.Address = 82;
            RegOutput head3 = new RegOutput();
            head3.REG = REGSEnum.ApD;
            head3.Address = 83;
            headerTag.Data.Add(head1);
            headerTag.Data.Add(head2);
            headerTag.Data.Add(head3);
            GIFTag tag;
            if (format == TexturePixelFormat.PSMCT32)
            {
                tag = EzSwizzle.ColorsToTag(image);
            } 
            else
            {
                byte[] textureData = new byte[width * height];
                byte[] paletteData = new byte[256 * 4];
                Dictionary<Color, Byte> palette = new Dictionary<Color, Byte>();
                var index = 0;
                foreach (var c in image)
                {
                    if (!palette.ContainsKey(c))
                    {
                        EzSwizzle.ColorsToByte(c, paletteData, palette.Count);
                        palette.Add(c, (Byte)palette.Count);
                    }
                    textureData[index] = palette[c];
                    ++index;
                }
                var rr = ResolutionMapper[$"[{width}, {height}]"];
                ulong high = (ulong)rr.Value;
                ulong low = (ulong)rr.Key;
                head2.Output = (high << 32) | (low);
                byte[] rawTextureData = new byte[rr.Value * 256];
                EzSwizzle.writeTexPSMT8To(MipLevelsTBP[0], TextureBufferWidth, 0, 0, width, height, textureData, rawTextureData);
                if (MipLevels > 1)
                {
                    var mipWidth = width;
                    var mipHeight = height;
                    var prevLevel = textureData;
                    for (var i = 1; i < MipLevels; ++i)
                    {
                        mipWidth /= 2;
                        mipHeight /= 2;
                        byte[] mipData = new byte[mipWidth * mipHeight];
                        for (var x = 0; x < mipWidth; ++x)
                        {
                            for (var y = 0; y < mipHeight; ++y)
                            {
                                mipData[x + y * mipWidth] = prevLevel[x * 2 + y * 2 * (mipWidth * 2)];
                            }
                        }
                        EzSwizzle.writeTexPSMT8To(MipLevelsTBP[i], MipLevelsTBW[i], 0, 0, mipWidth, mipHeight, mipData, rawTextureData);
                        prevLevel = mipData;
                    }
                }
                EzSwizzle.writeTexPSMT8To(ClutBufferBasePointer, 1, 0, 0, 16, 16, paletteData, rawTextureData);
                byte[] gifData = EzSwizzle.readTexPSMCT32(0, 1, 0, 0, rr.Key, rr.Value, rawTextureData); // what's difference between RRW and width
                tag = EzSwizzle.ColorsToTag(EzSwizzle.BytesToColors(gifData));
            }
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(stream);
                var QWC = (UInt64)headerTag.GetLength() + (UInt64)tag.GetLength() + 2;
                UInt64 low = QWC;
                low |= (UInt64)6 << 28;
                writer.Write(low);
                VIFCode code1 = new VIFCode();
                code1.OP = VIFCodeEnum.NOP;
                code1.Write(writer);
                VIFCode code2 = new VIFCode();
                code2.OP = VIFCodeEnum.DIRECT;
                code2.Immediate = (ushort)QWC;
                code2.Write(writer);
                headerTag.Write(writer);
                tag.Write(writer);
                TextureData = stream.ToArray();
            }
        }
        public List<Color> Colors { get; set; } = new List<Color>();
        public override String GetName()
        {
            return $"Texture {id:X}";
        }

        #region Enums
        public enum TexturePixelFormat
        {
            PSMCT32 = 0b000000,
            PSMCT24 = 0b000001,
            PSMCT16 = 0b000010,
            PSMCT16S = 0b001010,
            PSMT8 = 0b010011,
            PSMT4 = 0b010100,
            PSMT8H = 0b011011,
            PSMT4HL = 0b100100,
            PSMT4HH = 0b101100,
            PSMZ32 = 0b110000,
            PSMZ24 = 0b110001,
            PSMZ16 = 0b110010,
            PSMZ16S = 0b111010
        }
        public enum TextureColorComponent
        {
            RGB = 0,
            RGBA = 1
        }
        public enum TextureFunction
        {
            MODULATE = 0b00,
            DECAL = 0b01,
            HIGHLIGHT = 0b10,
            HIGHLIGHT2 = 0b11
        }
        #endregion
    }
}
