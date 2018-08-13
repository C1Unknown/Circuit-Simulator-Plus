﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CircuitSimulatorPlus
{
    public class CableRenderer
    {
        Cable cable;
        Line line;

        public CableRenderer(Cable cable)
        {
            this.cable = cable;

            line = new Line
            {
                Stroke = Brushes.Black,
                StrokeThickness = MainWindow.LineWidth
            };

            cable.InputNode.OnPositionChanged += OnPositionChanged;
            cable.OutputNode.OnPositionChanged += OnPositionChanged;
            cable.InputNode.OnSelectionChanged += OnSelectionChanged;
            cable.OutputNode.OnSelectionChanged += OnSelectionChanged;
            cable.InputNode.OnEmptyChanged += OnEmptyChanged;
            cable.OutputNode.OnEmptyChanged += OnEmptyChanged;
            cable.OnRenderedChanged += OnRenderedChanged;
        }

        ~CableRenderer()
        {
            Console.WriteLine("CableRenderer dtor");
        }

        public void OnRenderedChanged()
        {
            if (cable.IsRendered)
            {
                MainWindow.Canvas.Children.Add(line);

                OnSelectionChanged();
                OnPositionChanged();
            }
            else
            {
                MainWindow.Canvas.Children.Remove(line);
            }
        }

        public void OnSelectionChanged()
        {
            if (cable.OutputNode.IsSelected || cable.InputNode.IsSelected)
            {
                line.Stroke = SystemColors.MenuHighlightBrush;
            }
            else
            {
                line.Stroke = Brushes.Black;
            }
        }

        public void OnEmptyChanged()
        {
            cable.IsRendered = !cable.OutputNode.IsEmpty && !cable.InputNode.IsEmpty;
        }

        public void OnPositionChanged()
        {
            line.X1 = cable.OutputNode.Position.X;
            line.Y1 = cable.OutputNode.Position.Y;
            line.X2 = cable.InputNode.Position.X;
            line.Y2 = cable.InputNode.Position.Y;
        }
    }
}
