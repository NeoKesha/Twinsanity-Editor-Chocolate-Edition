﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twinsanity.TwinsanityInterchange.Implementations.PS2.Items.RM2.Code;

namespace TT_Lab.Assets.Code
{
    public class SoundEffectGR : SoundEffect
    {
        public SoundEffectGR() : base() { }
        public SoundEffectGR(UInt32 id, String name, PS2AnySound sound) : base(id, name, sound)
        {
        }
    }
}
