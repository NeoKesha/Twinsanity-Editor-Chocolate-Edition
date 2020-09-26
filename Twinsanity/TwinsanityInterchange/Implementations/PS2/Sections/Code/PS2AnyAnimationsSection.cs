﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twinsanity.TwinsanityInterchange.Enumerations;
using Twinsanity.TwinsanityInterchange.Implementations.Base;
using Twinsanity.TwinsanityInterchange.Implementations.PS2.Items.Code;

namespace Twinsanity.TwinsanityInterchange.Implementations.PS2.Sections.Code
{
    public class PS2AnyAnimationsSection : BaseTwinSection
    {
        public PS2AnyAnimationsSection() : base()
        {
            defaultType = typeof(PS2AnyAnimation);
        }
    }
}
