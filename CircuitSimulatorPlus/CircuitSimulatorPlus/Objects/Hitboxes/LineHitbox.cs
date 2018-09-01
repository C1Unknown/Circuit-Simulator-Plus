﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CircuitSimulatorPlus
{
    public class LineHitbox : Hitbox
    {
        Cable parent;
        int index;

        public LineHitbox(Cable parent, int index) : base(DistanceFactor)
        {
            this.parent = parent;
            this.index = index;
        }

        public const double DistanceFactor = 0.2;
        public const double Width = 1.0;

        public override Rect RectBounds
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override double DistanceTo(Point pos)
        {
            throw new NotImplementedException();
        }

        public override bool IncludesPos(Point pos)
        {
            throw new NotImplementedException();
        }

        public override bool IsIncludedIn(Rect rect)
        {
            throw new NotImplementedException();
        }
    }
}
