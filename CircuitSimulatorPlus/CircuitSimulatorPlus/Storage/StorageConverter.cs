﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CircuitSimulatorPlus
{
    static class StorageConverter
    {
        public static StorageObject ToStorageObject(Gate gate)
        {
            var store = new StorageObject();
            store.Name = gate.Name;
            store.Position = gate.Position;
            store.Type = gate.GetType().Name;

            for (int i = 0; i < gate.Input.Count; i++)
            {
                if (gate.Input[i].IsInverted)
                {
                    if (store.InvertedInputs == null)
                        store.InvertedInputs = new List<int>();
                    store.InvertedInputs.Add(i);
                }
            }

            for (int i = 0; i < gate.Output.Count; i++)
            {
                if (gate.Output[i].IsInverted)
                {
                    if (store.InvertedOutputs == null)
                        store.InvertedOutputs = new List<int>();
                    store.InvertedOutputs.Add(i);
                }
                if (gate.Output[i].State)
                {
                    if (store.InitialActiveOutputs == null)
                        store.InitialActiveOutputs = new List<int>();
                    store.InitialActiveOutputs.Add(i);
                }
            }

            if (gate is ContextGate)
            {
                var contextGate = gate as ContextGate;
                var contextCopy = new List<Gate>(contextGate.Context);
                int nextId = 1;
                var nodeToId = new Dictionary<ConnectionNode, int>();
                store.Context = new List<StorageObject>();

                for (int i = 0; i < contextGate.Input.Count; i++)
                {
                    var inputSwitch = new InputSwitch();
                    ConnectionNode contextNode = contextGate.Input[i];
                    ConnectionNode switchNode = inputSwitch.Output.First();
                    switchNode.NextConnectedTo = contextNode.NextConnectedTo;
                    inputSwitch.Position = new Point(0, i);
                    contextCopy.Add(inputSwitch);
                }
                foreach (Gate innerGate in contextCopy)
                {
                    foreach (OutputNode output in innerGate.Output)
                    {
                        if (output.NextConnectedTo.Count > 0)
                        {
                            nodeToId[output] = nextId;
                            foreach (ConnectionNode next in output.NextConnectedTo)
                                nodeToId[next] = nextId;
                            nextId++;
                        }
                        else
                        {
                            nodeToId[output] = 0;
                        }
                    }
                }

                for (int i = 0; i < contextGate.Output.Count; i++)
                {
                    int id;
                    var outputLight = new OutputLight();
                    ConnectionNode contextNode = contextGate.Output[i];
                    if (nodeToId.ContainsKey(contextNode))
                    {
                        id = nodeToId[contextNode];
                        nodeToId.Remove(contextNode);
                    }
                    else
                        id = 0;
                    ConnectionNode switchNode = outputLight.Input.First();
                    nodeToId[switchNode] = id;
                    outputLight.Position = new Point(1, i);
                    contextCopy.Add(outputLight);
                }

                foreach (Gate innerGate in contextCopy)
                {
                    StorageObject innerStore = ToStorageObject(innerGate);
                    innerStore.InputConnections = new int[innerGate.Input.Count];
                    for (int i = 0; i < innerGate.Input.Count; i++)
                    {
                        if (nodeToId.ContainsKey(innerGate.Input[i]))
                            innerStore.InputConnections[i] = nodeToId[innerGate.Input[i]];
                        else
                            innerStore.InputConnections[i] = 0;
                    }
                    innerStore.OutputConnections = new int[innerGate.Output.Count];
                    for (int i = 0; i < innerGate.Output.Count; i++)
                    {
                        if (nodeToId.ContainsKey(innerGate.Output[i]))
                            innerStore.OutputConnections[i] = nodeToId[innerGate.Output[i]];
                        else
                            throw new InvalidOperationException("Invalid connection");
                    }
                    store.Context.Add(innerStore);
                }
            }

            return store;
        }

        public static ContextGate ToGateTopLayer(StorageObject storageObject)
        {
            if (storageObject.Type != "ContextGate")
                throw new Exception("Object does not store an ContextGate");
            ContextGate contextGate = new ContextGate();
            contextGate.Name = storageObject.Name;

            var idToNode = new Dictionary<int, ConnectionNode>();
            foreach (StorageObject innerStore in storageObject.Context)
            {
                Gate innerGate = ToGate(innerStore);
                contextGate.Context.Add(innerGate);
                for (int i = 0; i < innerStore.OutputConnections.Count(); i++)
                {
                    int id = innerStore.OutputConnections[i];
                    if (id != 0)
                        idToNode[id] = innerGate.Output[i];
                }
            }

            for (int i = 0; i < storageObject.Context.Count; i++)
            {
                StorageObject innerStore = storageObject.Context[i];
                Gate innerGate = contextGate.Context[i];
                for (int j = 0; j < innerStore.InputConnections.Count(); j++)
                {
                    int id = innerStore.InputConnections[j];
                    if (id != 0)
                    {
                        if (!idToNode.ContainsKey(id))
                            throw new InvalidOperationException("Invalid connection");
                        ConnectionNode thisNode = innerGate.Input[j];
                        ConnectionNode otherNode = idToNode[id];
                        otherNode.NextConnectedTo.Add(thisNode);
                        thisNode.BackConnectedTo = otherNode;
                        thisNode.State = thisNode.IsInverted ? !otherNode.State : otherNode.State;
                    }
                }
            }

            return contextGate;
        }

        public static Gate ToGate(StorageObject storageObject)
        {
            Gate gate;
            switch (storageObject.Type)
            {
            case "ContextGate":
                gate = new ContextGate();
                break;
            case "AndGate":
                gate = new AndGate();
                break;
            case "OrGate":
                gate = new OrGate();
                break;
            case "NopGate":
                gate = new NopGate();
                break;
            case "InputSwitch":
                gate = new InputSwitch();
                break;
            case "OutputLight":
                gate = new OutputLight();
                break;
            case "SegmentDisplay":
                gate = new SegmentDisplay();
                break;
            default:
                throw new InvalidOperationException("Unknown type");
            }

            gate.Name = storageObject.Name;
            gate.Position = storageObject.Position;

            // AAAAAAAAAAAHHHHHH
            foreach (InputNode node in new List<InputNode>(gate.Input))
                gate.RemoveInputNode(node);
            foreach (OutputNode node in new List<OutputNode>(gate.Output))
                gate.RemoveOutputNode(node);

            if (storageObject.InputConnections == null)
                storageObject.InputConnections = new int[0];
            if (storageObject.OutputConnections == null)
                storageObject.OutputConnections = new int[0];

            if (storageObject.Type == "ContextGate")
            {
                ContextGate contextGate = gate as ContextGate;
                var idToNode = new Dictionary<int, ConnectionNode>();
                var inputStores = new List<StorageObject>();
                var outputStores = new List<StorageObject>();
                var gateStores = new List<StorageObject>();

                foreach (StorageObject innerStore in storageObject.Context)
                {
                    switch (innerStore.Type)
                    {
                    case "InputSwitch":
                        inputStores.Add(innerStore);
                        break;
                    case "OutputLight":
                        outputStores.Add(innerStore);
                        break;
                    default:
                        gateStores.Add(innerStore);
                        break;
                    }
                }

                inputStores.Sort(ComparePosition);
                outputStores.Sort(ComparePosition);

                foreach (StorageObject gateStore in gateStores)
                {
                    Gate innerGate = ToGate(gateStore);
                    contextGate.Context.Add(innerGate);
                    for (int i = 0; i < gateStore.OutputConnections.Count(); i++)
                    {
                        int id = gateStore.OutputConnections[i];
                        if (id != 0)
                            idToNode[id] = innerGate.Output[i];
                    }
                }
                foreach (StorageObject inputStore in inputStores)
                {
                    int id = inputStore.OutputConnections.First();
                    var inputNode = new InputNode(contextGate);
                    contextGate.Input.Add(inputNode);
                    if (id != 0)
                        idToNode[id] = inputNode;
                }

                for (int i = 0; i < gateStores.Count; i++)
                {
                    StorageObject innerStore = gateStores[i];
                    Gate innerGate = contextGate.Context[i];
                    for (int j = 0; j < innerStore.InputConnections.Count(); j++)
                    {
                        int id = innerStore.InputConnections[j];
                        if (id != 0)
                        {
                            if (!idToNode.ContainsKey(id))
                                throw new InvalidOperationException("Invalid connection");
                            ConnectionNode thisNode = innerGate.Input[j];
                            ConnectionNode otherNode = idToNode[id];
                            otherNode.NextConnectedTo.Add(thisNode);
                            thisNode.BackConnectedTo = otherNode;
                            thisNode.State = thisNode.IsInverted ? !otherNode.State : otherNode.State;
                        }
                    }
                }
                foreach (StorageObject outputStore in outputStores)
                {
                    int id = outputStore.InputConnections.First();
                    OutputNode contextNode = new OutputNode(contextGate);
                    contextGate.Output.Add(contextNode);
                    if (id != 0)
                    {
                        if (!idToNode.ContainsKey(id))
                            throw new InvalidOperationException("Invalid connection");
                        ConnectionNode otherNode = idToNode[id];
                        otherNode.NextConnectedTo.Add(contextNode);
                        contextNode.BackConnectedTo = otherNode;
                        contextNode.State = otherNode.State;
                    }
                }
            }
            else
            {
                foreach (int id in storageObject.InputConnections)
                    gate.Input.Add(new InputNode(gate));
                foreach (int id in storageObject.OutputConnections)
                    gate.Output.Add(new OutputNode(gate));
            }

            if (storageObject.InvertedInputs != null)
                foreach (int index in storageObject.InvertedInputs)
                {
                    gate.Input[index].Invert();
                    gate.Input[index].State = !gate.Input[index].State;
                }
            if (storageObject.InvertedOutputs != null)
                foreach (int index in storageObject.InvertedOutputs)
                    gate.Output[index].Invert();

            if (storageObject.InitialActiveOutputs != null)
                foreach (int index in storageObject.InitialActiveOutputs)
                    gate.Output[index].State = true;

            if (storageObject.Type == "InputSwitch")
                ((InputSwitch)gate).State = gate.Output.First().State;

            return gate;
        }

        private static int ComparePosition(StorageObject a, StorageObject b)
        {
            int res = (int)a.Position.Y - (int)b.Position.Y;
            if (res == 0)
                res = (int)a.Position.X - (int)b.Position.X;
            return res;
        }
    }
}
