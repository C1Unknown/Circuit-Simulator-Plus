﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CircuitSimulatorPlus
{
    public class Cable
    {
        public const double CableWidth = 1;

        InputNode inputNode;
        OutputNode outputNode;
        List<Point> points;
        RectHitbox hitbox;

        public OutputNode OutputNode
        {
            get; set;
        }

        public InputNode InputNode
        {
            get; set;
        }

        public bool State
        {
            get; set;
        }
    }
}