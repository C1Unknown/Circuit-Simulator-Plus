﻿using System;
using System.Windows;

namespace CircuitSimulatorPlus
{
    public class RectHitbox : Hitbox
    {
        public Rect Bounds;

        public RectHitbox(object attachedObject, Rect bounds, double distanceFactor)
            : base(attachedObject, distanceFactor)
        {
            Bounds = bounds;
        }

        public override double DistanceTo(Point pos)
        {
            var center = new Point(
                Bounds.X + Bounds.Width / 2,
                Bounds.Y + Bounds.Height / 2);
            return distanceFactor * (center - pos).LengthSquared;
        }

        public override bool IncludesPos(Point pos)
        {
            return pos.X >= Bounds.X
                && pos.Y >= Bounds.Y
                && pos.X <= Bounds.X + Bounds.Width
                && pos.Y <= Bounds.Y + Bounds.Height;
        }

        public override bool IsIncludedIn(Rect rect)
        {
            return Bounds.X >= rect.X - Bounds.Width
                && Bounds.X <= rect.X + rect.Width
                && Bounds.Y >= rect.Y - Bounds.Height
                && Bounds.Y <= rect.Y + rect.Height;
        }
    }
}
