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
        Cable cable;
        int index;
        bool vert;

        public LineHitbox(Cable cable, int index) : base(Cable.DistanceFactor)
        {
            this.cable = cable;
            this.index = index;
            vert = (index & 1) != 0;
        }

        public override Rect RectBounds
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        double Dist(Point pos)
        {
            Point point = cable.GetPoint(index);
            Point lastPoint = cable.GetPoint(index - 1);
            Point nextPoint = cable.GetPoint(index + 1);

            double startX = vert ? point.X : lastPoint.X;
            double startY = vert ? lastPoint.Y : point.Y;
            double endX = vert ? point.X : nextPoint.X;
            double endY = vert ? nextPoint.Y : point.Y;

            double dist = vert ? Math.Abs(pos.X - startX) : Math.Abs(pos.Y - startY);

            double lastDist = vert ? Math.Abs(pos.Y - startY) : Math.Abs(pos.X - startX);
            double nextDist = vert ? Math.Abs(pos.Y - endY) : Math.Abs(pos.X - endX);

            double len = vert ? Math.Abs(endY - startY) : Math.Abs(endX - startX);

            if (lastDist < len && nextDist < len)
                return dist - 1e-7;

            double minDist = Math.Min(lastDist, nextDist);
            double sideDist = Math.Max(dist, minDist);

            if (dist > minDist)
                return sideDist - 1e-7;

            return sideDist;
        }

        public override double DistanceTo(Point pos)
        {
            return Dist(pos) * Cable.DistanceFactor;
        }

        public override bool IncludesPos(Point pos)
        {
            return Dist(pos) <= Cable.SegmentWidth && cable.IsCompleted;
        }

        public override bool IsIncludedIn(Rect rect)
        {
            return RectBounds.IntersectsWith(rect);
        }
    }
}
