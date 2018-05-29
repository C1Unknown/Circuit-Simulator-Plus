﻿using System;
using System.Collections.Generic;

namespace CircuitSimulatorPlus
{
    public abstract class ConnectionNode
    {
        protected ConnectionNode(Gate owner)
        {
            Owner = owner;
            IsEmpty = true;
            NextConnectedTo = new List<ConnectionNode>();
        }

        public List<ConnectionNode> NextConnectedTo { get; set; }
        public ConnectionNode BackConnectedTo { get; set; }

        bool state;
        bool stateChanged;
        /// <summary>
        /// True, if this ConnectionNode is displayed as 'high' (1).
        /// False, if this ConnectionNode is displayed as 'low' (0).
        /// </summary>
        public bool State
        {
            get { return state; }
            set {
                if (state != value)
                {
                    stateChanged = !stateChanged;
                    state = value;
                }
            }
        }
        /// <summary>
        /// A reference to the Gate which the ConnectionNode is connected to.
        /// </summary>
        public Gate Owner { get; private set; }
        /// <summary>
        /// True, if this ConnectionNode is NOT connected to another ConnectionNode.
        /// </summary>
        public bool IsEmpty { get; set; }
        /// <summary>
        /// True, if this ConnectionNode is inverted.
        /// </summary>
        public bool IsInverted { get; set; }
        /// <summary>
        /// Name displayed next to the ConnectionNode.
        /// (inside the Gate it is connected to)
        /// </summary>
        public string Name { get; set; }

        public abstract void Clear();

        /// <summary>
        /// Inverts the ConnectionNode.
        /// </summary>
        public void Invert()
        {
            IsInverted = !IsInverted;
        }

        public abstract void Tick(Queue<ConnectionNode> tickedNodes);

        protected void Tick(Queue<ConnectionNode> tickedNodes, bool isOutput)
        {
            bool nextIsElementary = !Owner.HasContext && !isOutput;
            bool lastWasElementary = !Owner.HasContext && isOutput;

            if (IsEmpty)
            {
                if (lastWasElementary)
                    State = Owner.Eval();
            }
            else
            {
                State = BackConnectedTo.State != IsInverted;
            }

            if (stateChanged)
            {
                stateChanged = false;

                if (nextIsElementary)
                {
                    foreach (OutputNode node in Owner.Output)
                        tickedNodes.Enqueue(node);
                }
                else
                {
                    foreach (ConnectionNode node in NextConnectedTo)
                        tickedNodes.Enqueue(node);
                }
            }
        }
    }
}