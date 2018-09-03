﻿using System;
using System.Collections.Generic;
using System.Windows;

namespace CircuitSimulatorPlus
{
    public class Cable
    {
        public Cable(ConnectionNode startNode)
        {
            StartNode = startNode;

            if (startNode is InputNode)
            {
                (startNode as InputNode).ConnectedCable = this;
            }
            if (startNode is OutputNode)
            {
                (startNode as OutputNode).ConnectedCables.Add(this);
            }

            Segments.Add(new CableSegment(this, Segments.Count));
            Segments.Add(new CableSegment(this, Segments.Count));
        }

        public const double DistanceFactor = 0.9;
        public const double SegmentWidth = 1.0;

        public List<CableSegment> Segments = new List<CableSegment>();

        public void SplitSegment(int index)
        {
            if (index > 0 && index < points.Count + 1)
            {
                if ((index & 1) != 0)
                {
                    double centerY = GetPoint(index - 1).Y / 2 + GetPoint(index + 1).Y / 2;
                    double centerX = GetPoint(index).X;
                    points[index - 1] = new Point(centerX, centerY);

                    AddSegment(index + 1, new Point(centerX + 0.5, 0));
                    AddSegment(index + 0, new Point(centerX - 0.5, 0));
                }
                else
                {
                    double centerX = GetPoint(index - 1).X / 2 + GetPoint(index + 1).X / 2;
                    double centerY = GetPoint(index).Y;
                    points[index - 1] = new Point(centerX, centerY);

                    AddSegment(index + 1, new Point(0, centerY + 0.5));
                    AddSegment(index + 0, new Point(0, centerY - 0.5));
                }
            }
        }

        public bool IsCompleted;

        bool vertical;

        public event Action OnPointsChanged;

        List<Point> points = new List<Point>();

        public void MovePoint(int index, Vector vector)
        {
            if (index > 0 && index < points.Count + 1)
                points[index - 1] += vector;

            OnPointsChanged?.Invoke();
        }

        public Point GetPoint(int index)
        {
            if (index <= 0)
                return StartPos;
            if (index >= points.Count + 1)
                return EndPos;
            return points[index - 1];
        }

        public Point StartPos
        {
            get
            {
                return StartNode.Position;
            }
        }
        public Point EndPos
        {
            get
            {
                if (IsCompleted)
                    return EndNode.Position;
                return MainWindow.Self.LastCanvasPos;
            }
        }

        ConnectionNode startNode;
        public ConnectionNode StartNode
        {
            get
            {
                return startNode;
            }
            set
            {
                startNode = value;
                if (value is OutputNode)
                    outputNode = value as OutputNode;
            }
        }

        ConnectionNode endNode;
        public ConnectionNode EndNode
        {
            get
            {
                return endNode;
            }
            set
            {
                endNode = value;
                if (value is OutputNode)
                    outputNode = value as OutputNode;
                OnPointsChanged?.Invoke();
            }
        }

        OutputNode outputNode;
        public OutputNode OutputNode
        {
            get
            {
                return outputNode;
            }
            set
            {
                outputNode = value;
                outputNode.OnStateChanged += OnStateChanged;
                OnStateChanged?.Invoke();
            }
        }

        public event Action OnStateChanged;
        public bool State
        {
            get
            {
                return outputNode?.State ?? false;
            }
        }

        public void AutoComplete()
        {
            if (points.Count == 0)
            {
                AddSegment(StartPos + (EndPos - StartPos) / 2);
            }
            else if ((points.Count & 1) == 0)
            {
                Point lastPoint = points[points.Count - 1];
                AddSegment(lastPoint + (EndPos - lastPoint) / 2);
            }
        }

        public void ConnectTo(ConnectionNode endNode)
        {
            EndNode = endNode;

            if (endNode is InputNode)
            {
                (endNode as InputNode).ConnectedCable = this;
            }
            if (endNode is OutputNode)
            {
                (endNode as OutputNode).ConnectedCables.Add(this);
            }

            AutoComplete();
            IsCompleted = true;
            
            OnPointsChanged?.Invoke();
        }

        public void RemoveSegment(int index)
        {
            vertical = !vertical;
            points.RemoveAt(index - 1);

            Segments.RemoveAt(index);

            for (int i = index + 1; i < Segments.Count; i++)
                Segments[i].Index--;

            OnPointsChanged?.Invoke();
        }

        public void AddSegment(int index, Point point)
        {
            vertical = !vertical;
            points.Insert(index - 1, point);

            Segments.Insert(index, new CableSegment(this, index));

            for (int i = index + 1; i < Segments.Count; i++)
                Segments[i].Index++;

            OnPointsChanged?.Invoke();
        }

        public void AddSegment(Point point)
        {
            vertical = !vertical;
            points.Add(point);

            Segments.Add(new CableSegment(this, Segments.Count));

            OnPointsChanged?.Invoke();
        }

        public void Remove()
        {
            foreach (CableSegment segment in Segments)
            {
                segment.Remove();
            }
        }
    }
}
