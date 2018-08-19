﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CircuitSimulatorPlus
{
    public class SegmentDisplay : Gate
    {
        public SegmentDisplay() : base(7, 0)
        {
            new SegmentDisplayRenderer(this);

            Size = new Size(5, 7);
        }

        public override bool Eval()
        {
            throw new InvalidOperationException();
        }
    }
}
