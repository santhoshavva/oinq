﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oinq.Core.Tests
{
    public class AttributedFakeData
    {
        [PigMapping("dimension")]
        public String Dim1 { get; set; }
        [PigMapping("measure")]
        public Int32 Mea1 { get; set; }
    }
}
