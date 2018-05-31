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
    public class CableRenderer : IRenderer
    {
        Canvas canvas;
        Cable cable;
        List<Line> lines = new List<Line>();

        public CableRenderer(Canvas canvas, Cable cable)
        {
            this.canvas = canvas;
            this.cable = cable;
            cable.StateChanged += OnGateOutputChanged;
        }

        public void Update()
        {
            Unrender();
            Render();
        }

        public void Render()
        {
            for (int i = 1; i < cable.Points.Count; i++)
            {
                Point pos1 = cable.Points[i - 1];
                Point pos2 = cable.Points[i];
                Line line = new Line();
                line.Stroke = Brushes.Black;
                line.StrokeThickness = MainWindow.LineWidth;
                line.X1 = pos1.X;
                line.Y1 = pos1.Y;
                line.X2 = pos2.X;
                line.Y2 = pos2.Y;
                canvas.Children.Add(line);
                lines.Add(line);
            }
        }

        public void Unrender()
        {
            foreach (Line line in lines)
            {
                canvas.Children.Remove(line);
            }
        }

        void OnGateOutputChanged(object sender, EventArgs e)
        {
            Brush brush = cable.Output.State ? Brushes.Red : Brushes.Black;
            foreach (Line line in lines)
            {
                line.Stroke = brush;
            }
        }
    }
}
