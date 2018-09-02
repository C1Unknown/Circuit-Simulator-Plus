﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CircuitSimulatorPlus
{
    public class InputNodeRenderer
    {
        InputNode inputNode;

        Line upperRisingEdgeLine;
        Line lowerRisingEdgeLine;

        Label nameLabel;

        public InputNodeRenderer(InputNode inputNode)
        {
            this.inputNode = inputNode;

            upperRisingEdgeLine = new Line
            {
                Stroke = Brushes.Black,
                StrokeThickness = Constants.LineWidth
            };
            lowerRisingEdgeLine = new Line
            {
                Stroke = Brushes.Black,
                StrokeThickness = Constants.LineWidth
            };

            nameLabel = new Label
            {
                Padding = new Thickness(0.2, 0, 0, 0),
                HorizontalContentAlignment = HorizontalAlignment.Left,
                VerticalContentAlignment = VerticalAlignment.Center,
                Height = 1,
                FontSize = 0.5
            };

            inputNode.OnRenderedChanged += OnRenderedChanged;

            if (inputNode.IsRendered)
            {
                OnRenderedChanged();
            }
        }

        public void OnRenderedChanged()
        {
            if (inputNode.IsRendered)
            {
                MainWindow.Self.canvas.Children.Add(upperRisingEdgeLine);
                MainWindow.Self.canvas.Children.Add(lowerRisingEdgeLine);

                MainWindow.Self.canvas.Children.Add(nameLabel);

                inputNode.OnRisingEdgeChanged += OnRisingEdgeChanged;
                inputNode.OnRisingEdgeChanged += OnPositionChanged;
                inputNode.OnNameChanged += OnNameChanged;
                inputNode.OnPositionChanged += OnPositionChanged;

                OnRisingEdgeChanged();
                OnNameChanged();
                OnPositionChanged();
            }
            else
            {
                MainWindow.Self.canvas.Children.Remove(upperRisingEdgeLine);
                MainWindow.Self.canvas.Children.Remove(lowerRisingEdgeLine);

                MainWindow.Self.canvas.Children.Remove(nameLabel);

                inputNode.OnRisingEdgeChanged -= OnRisingEdgeChanged;
                inputNode.OnRisingEdgeChanged -= OnPositionChanged;
                inputNode.OnNameChanged -= OnNameChanged;
                inputNode.OnPositionChanged -= OnPositionChanged;
            }
        }

        public void OnRisingEdgeChanged()
        {
            Visibility lineVisibility = inputNode.IsRisingEdge ? Visibility.Visible : Visibility.Collapsed;
            upperRisingEdgeLine.Visibility = lowerRisingEdgeLine.Visibility = lineVisibility;
        }

        public void OnNameChanged()
        {
            nameLabel.Content = inputNode.Name;
        }

        public void OnPositionChanged()
        {
            upperRisingEdgeLine.X1 = inputNode.Position.X;
            upperRisingEdgeLine.X2 = inputNode.Position.X + Constants.InversionDotDiameter * 1.2;
            upperRisingEdgeLine.Y1 = inputNode.Position.Y - Constants.InversionDotRadius * 1.2;
            upperRisingEdgeLine.Y2 = inputNode.Position.Y;

            lowerRisingEdgeLine.X1 = inputNode.Position.X;
            lowerRisingEdgeLine.X2 = inputNode.Position.X + Constants.InversionDotDiameter * 1.2;
            lowerRisingEdgeLine.Y1 = inputNode.Position.Y + Constants.InversionDotRadius * 1.2;
            lowerRisingEdgeLine.Y2 = inputNode.Position.Y;

            if (inputNode.IsRisingEdge)
            {
                Canvas.SetLeft(nameLabel, inputNode.Position.X + Constants.InversionDotDiameter * 1.2);
            }
            else
            {
                Canvas.SetLeft(nameLabel, inputNode.Position.X);
            }
            Canvas.SetTop(nameLabel, inputNode.Position.Y - nameLabel.Height / 2);
        }
    }
}