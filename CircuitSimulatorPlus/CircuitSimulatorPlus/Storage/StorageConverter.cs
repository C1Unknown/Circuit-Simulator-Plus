﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                int nextId = 1;
                var nodeToId = new Dictionary<ConnectionNode, int>();
                store.Context = new List<StorageObject>();
                
                foreach (Gate innerGate in contextGate.Context)
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
                
                foreach (Gate innerGate in contextGate.Context)
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
                        ConnectionNode outputNode = idToNode[id];
                        outputNode.ConnectTo(innerGate.Input[j]);
                        innerGate.Input[j].State = outputNode.State;
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

            foreach (int id in storageObject.InputConnections)
                gate.Input.Add(new InputNode(gate));
            if (storageObject.InvertedInputs != null)
                foreach (int index in storageObject.InvertedInputs)
                    gate.Input[index].Invert();

            // AAAAAAAAAAAHHHHHH
            foreach (OutputNode node in new List<OutputNode>(gate.Output))
                gate.RemoveOutputNode(node);

            foreach (int id in storageObject.OutputConnections)
                gate.Output.Add(new OutputNode(gate));
            if (storageObject.InvertedOutputs != null)
                foreach (int index in storageObject.InvertedOutputs)
                    gate.Output[index].Invert();
            if (storageObject.InitialActiveOutputs != null)
                foreach (int index in storageObject.InitialActiveOutputs)
                    gate.Output[index].State = true;

            if (storageObject.Type == "ContextGate")
            {
                ContextGate contextGate = gate as ContextGate;
                var idToNode = new Dictionary<int, ConnectionNode>();
                List<StorageObject> inputStores = new List<StorageObject>();
                foreach (StorageObject innerStore in storageObject.Context)
                {
                    if (innerStore.Type == "InputSwitch")
                    {
                        inputStores.Add(innerStore);
                        continue;
                    }
                    else if (innerStore.Type == "OutputLight")
                        continue;
                    Gate innerGate = ToGate(innerStore);
                    contextGate.Context.Add(innerGate);
                    for (int i = 0; i < innerStore.OutputConnections.Count(); i++)
                    {
                        int id = innerStore.OutputConnections[i];
                        if (id != 0)
                            idToNode[id] = innerGate.Output[i];
                    }
                }
                inputStores.Sort(ComparePosition);
                foreach (StorageObject inputStore in inputStores)
                {
                    int id = inputStore.OutputConnections.First();
                    var inputNode = new InputNode(contextGate);
                    contextGate.Input.Add(inputNode);
                    if (id != 0)
                        idToNode[id] = inputNode;
                }

                List<StorageObject> outputStores = new List<StorageObject>();
                for (int i = 0; i < storageObject.Context.Count; i++)
                {
                    StorageObject innerStore = storageObject.Context[i];
                    if (innerStore.Type == "OutputLight")
                    {
                        outputStores.Add(innerStore);
                        continue;
                    }
                    else if (innerStore.Type == "InputSwitch")
                        continue;
                    Gate innerGate = contextGate.Context[i];
                    for (int j = 0; j < innerStore.InputConnections.Count(); j++)
                    {
                        int id = innerStore.InputConnections[j];
                        if (id != 0)
                        {
                            if (!idToNode.ContainsKey(id))
                                throw new InvalidOperationException("Invalid connection");
                            ConnectionNode outputNode = idToNode[id];
                            outputNode.ConnectTo(innerGate.Input[j]);
                            innerGate.Input[j].State = outputNode.State;
                        }
                    }
                }
                outputStores.Sort(ComparePosition);
                foreach (StorageObject outputStore in outputStores)
                {
                    int id = outputStore.InputConnections.First();
                    if (id != 0)
                    {
                        if (!idToNode.ContainsKey(id))
                            throw new InvalidOperationException("Invalid connection");
                        ConnectionNode outputNode = idToNode[id];
                        OutputNode contextNode = new OutputNode(contextGate);
                        contextGate.Output.Add(contextNode);
                        outputNode.ConnectTo(contextNode);
                        contextNode.State = outputNode.State;
                    }
                }
            }

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
