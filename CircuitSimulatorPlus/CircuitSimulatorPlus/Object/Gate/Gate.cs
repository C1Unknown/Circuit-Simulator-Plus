﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace CircuitSimulatorPlus
{
    public abstract class Gate : IClickable, IMovable
    {
        public Gate()
        {
            hitbox = new RectHitbox(this, new Rect(), DistanceFactor);
            Size = new Size(3, 4);
        }

        public const double DistanceFactor = 0.2;

        string name;
        string tag;
        bool isSelected;
        Point position;
        Size size;
        RectHitbox hitbox;

        public Dictionary<ConnectionNode.Align, List<ConnectionNode>> ConnectedNodes;

        public abstract string Type { get; }

        public List<InputNode> Input = new List<InputNode>();
        public List<OutputNode> Output = new List<OutputNode>();
        public GateRenderer Renderer;

        public Point Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
                hitbox.Bounds.Location = value;
                Renderer?.OnPositionChanged();
            }
        }
        public Size Size
        {
            get
            {
                return size;
            }
            set
            {
                size = value;
                hitbox.Bounds.Size = value;
                Renderer?.OnSizeChanged();
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                Renderer?.OnNameChanged();
            }
        }
        public string Tag
        {
            get
            {
                return tag;
            }
            set
            {
                tag = value;
                Renderer?.OnTagChanged();
            }
        }

        public void Move(Vector vector)
        {
            Position += vector;
            UpdateConnectionNodePos();
        }

        public void UpdateConnectionNodePos()
        {
            // TODO
        }

        public bool IsSelected
        {
            get
            {
                return isSelected;
            }
            set
            {
                isSelected = value;
                Renderer?.OnSelectionChanged();
            }
        }
        public Hitbox Hitbox
        {
            get
            {
                return hitbox;
            }
            set
            {
                hitbox = value as RectHitbox;
            }
        }

        public virtual bool HasContext
        {
            get
            {
                return false;
            }
        }

        public abstract bool Eval();
    }
}
