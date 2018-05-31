﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CircuitSimulatorPlus
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
            canvas.SnapsToDevicePixels = true;

            UpdateTitle();
            ResetView();

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
                context = StorageConverter.ToGate(Storage.Load(args[1]));
            else
                context = new Gate();
            foreach (Gate gate in context.Context)
                gate.Renderer.Render();

            timer.Interval = TimeSpan.FromMilliseconds(0);
            timer.Tick += TimerTick;
        }

        #region Constants
        public const string WindowTitle = "Circuit Simulator Plus";
        public const string FileFilter = "Circuit Simulator Plus Circuit|*" + FileFormat;
        public const string FileFormat = "tici";
        public const string DefaultTitle = "untitled";
        public const string Unsaved = "\u2022";
        public const double ScaleFactor = 0.9;
        public const double LineWidth = 0.1;
        public const int UndoBufferSize = 32;
        #endregion

        #region Properties
        Point lastMousePos;
        Point lastCanvasPos;
        Point lastMouseClick;
        Point lastCanvasClick;
        bool showContextMenu;
        bool drawingcable;
        bool dragging;
        bool saved = true;
        string title = DefaultTitle;

        Queue<ConnectionNode> tickedNodes = new Queue<ConnectionNode>();
        DispatcherTimer timer = new DispatcherTimer();
        List<Cable> cables = new List<Cable>();
        List<Gate> selected = new List<Gate>();
        Gate context = new Gate();
        #endregion

        #region Gates
        public Gate CreateGate(Gate gate, int amtInputs, int amtOutputs)
        {
            context.Context.Add(gate);
            var renderer = new SimpleGateRenderer(canvas, gate);
            renderer.InputClicked += OnGateInputClicked;
            renderer.OutputClicked += OnGateOutputClicked;
            gate.Renderer = renderer;
            gate.Position = new Point(5, context.Context.Count * 5);

            for (int i = 0; i < amtInputs; i++)
                gate.Input.Add(new InputNode(gate));
            for (int i = 0; i < amtOutputs; i++)
                gate.Output.Add(new OutputNode(gate));

            gate.Renderer.Render();
            return gate;
        }
        public void Tick(ConnectionNode node)
        {
            tickedNodes.Enqueue(node);
            timer.Start();
        }
        public void TickQueue()
        {
            List<ConnectionNode> copy = tickedNodes.ToList();
            tickedNodes.Clear();
            foreach (ConnectionNode ticked in copy)
                ticked.Tick(tickedNodes);
            if (tickedNodes.Count == 0)
                timer.Stop();
        }
        public void TimerTick(object sender, EventArgs e)
        {
            TickQueue();
        }
        public void Select(Gate gate)
        {
            if (!gate.IsSelected)
            {
                selected.Add(gate);
                gate.IsSelected = true;
            }
        }
        #endregion

        #region Visuals
        public void ResetView()
        {
            Matrix matrix = Matrix.Identity;
            matrix.Scale(20, 20);
            canvas.RenderTransform = new MatrixTransform(matrix);
        }
        public void UpdateTitle()
        {
            Title = $"{title}{(saved ? "" : " " + Unsaved)} - {WindowTitle}";
        }
        #endregion

        #region Misc
        public void PerformAction(Action action)
        {
            // TODO: Create undo / redo stack
            action.Redo();
        }
        #endregion

        #region Events
        void DEBUG_AddAndGate(object sender, EventArgs e)
        {
            CreateGate(new Gate(Gate.GateType.And), 2, 1).Position = lastCanvasClick;
            foreach (Gate gate in context.Context)
                gate.SnapToGrid();
        }
        void DEBUG_AddOrGate(object sender, EventArgs e)
        {
            CreateGate(new Gate(Gate.GateType.Or), 2, 1).Position = lastCanvasClick;
            foreach (Gate gate in context.Context)
                gate.SnapToGrid();
        }
        void DEBUG_AddNotGate(object sender, EventArgs e)
        {
            CreateGate(new Gate(Gate.GateType.Not), 1, 1).Position = lastCanvasClick;
            foreach (Gate gate in context.Context)
                gate.SnapToGrid();
        }

        void Window_KeyDown(object sender, KeyEventArgs e)
        {
        }
        void Window_KeyUp(object sender, KeyEventArgs e)
        {
        }

        void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is Gate)
                Select(e.OriginalSource as Gate);

            lastCanvasClick = e.GetPosition(canvas);
            lastMousePos = lastMouseClick = e.GetPosition(this);
            showContextMenu = true;
            if (e.RightButton == MouseButtonState.Pressed)
            {
                dragging = true;
                CaptureMouse();
            }
            if (drawingcable)
            {
                Cable lastcable = cables.Last();
                Point pos = e.GetPosition(canvas);
                pos.X = Math.Round(pos.X);
                pos.Y = Math.Round(pos.Y);
                lastcable.AddPoint(pos);
            }
        }
        void Window_MouseMove(object sender, MouseEventArgs e)
        {
            Point currentPos = e.GetPosition(this);
            Vector moved = currentPos - lastMousePos;

            if (dragging)
            {
                Matrix matrix = canvas.RenderTransform.Value;
                matrix.Translate(moved.X, moved.Y);
                canvas.RenderTransform = new MatrixTransform(matrix);
            }

            if (e.LeftButton == MouseButtonState.Pressed)
                foreach (Gate gate in selected)
                    gate.Move(moved);

            lastMousePos = currentPos;
            lastCanvasPos = e.GetPosition(canvas);

            if (moved.Length > 0)
                showContextMenu = false;
        }
        void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            dragging = false;
            foreach (Gate gate in selected)
                gate.SnapToGrid();
            ReleaseMouseCapture();
        }

        void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point currentPos = e.GetPosition(canvas);
            double scale = e.Delta > 0 ? 1 / ScaleFactor : ScaleFactor;

            Matrix matrix = canvas.RenderTransform.Value;
            matrix.ScaleAtPrepend(scale, scale, currentPos.X, currentPos.Y);
            canvas.RenderTransform = new MatrixTransform(matrix);
        }

        void Window_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = !showContextMenu;
        }

        void NewFile_Click(object sender, RoutedEventArgs e)
        {
            foreach (Gate gate in context.Context)
                gate.Renderer.Unrender();
        }
        void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            NewFile_Click(sender, e);
            var dialog = new OpenFileDialog();
            dialog.Filter = "Circuit File (.json)|*.json";
            if (dialog.ShowDialog() == true)
            {
                context = StorageConverter.ToGate(Storage.Load(dialog.FileName));
            }
        }
        void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.FileName = "Circuit";
            dialog.DefaultExt = ".json";
            if (dialog.ShowDialog() == true)
            {
                string path = dialog.FileName;
                Storage.Save(path, StorageConverter.ToStorageObject(context));
            }
        }
        void SaveFileAs_Click(object sender, RoutedEventArgs e)
        {

        }

        void Undo_Click(object sender, RoutedEventArgs e)
        {

        }
        void Redo_Click(object sender, RoutedEventArgs e)
        {

        }
        void Copy_Click(object sender, RoutedEventArgs e)
        {

        }
        void Cut_Click(object sender, RoutedEventArgs e)
        {

        }
        void Paste_Click(object sender, RoutedEventArgs e)
        {

        }
        void Delete_Click(object sender, RoutedEventArgs e)
        {

        }

        void DefaultView_Click(object sender, RoutedEventArgs e)
        {
            ResetView();
        }
        void ZoomIn_Click(object sender, RoutedEventArgs e)
        {

        }
        void ZoomOut_Click(object sender, RoutedEventArgs e)
        {

        }

        void NewGate_Click(object sender, RoutedEventArgs e)
        {

        }
        void RenameGate_Click(object sender, RoutedEventArgs e)
        {

        }
        void ResizeGate_Click(object sender, RoutedEventArgs e)
        {

        }
        void EmptyInput_Click(object sender, RoutedEventArgs e)
        {

        }
        void TrimInputs_Click(object sender, RoutedEventArgs e)
        {

        }
        void InvertConnection_Click(object sender, RoutedEventArgs e)
        {

        }
        void SelectAll_Click(object sender, RoutedEventArgs e)
        {

        }

        void Reset_Click(object sender, RoutedEventArgs e)
        {

        }
        void Reload_Click(object sender, RoutedEventArgs e)
        {
        }
        void TickAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (Gate gate in context.Context)
            {
                foreach (InputNode inputNode in gate.Input)
                    Tick(inputNode);
                foreach (OutputNode outputNode in gate.Output)
                    Tick(outputNode);
            }
        }

        void OnGateOutputClicked(object sender, EventArgs e)
        {
            Gate gate = (Gate)sender;
            int index = ((IndexEventArgs)e).Index;

            if (!drawingcable)
            {
                Point point = new Point();
                point.X = gate.Position.X + 3 + 1;
                point.Y = gate.Position.Y + (double)4 * (1 + 2 * index) / (2 * gate.Output.Count);
                Cable cable = new Cable();
                cables.Add(cable);
                cable.Renderer = new CableRenderer(canvas, cable);
                cable.Output = gate.Output[index];
                drawingcable = true; 
            }
        }
        void OnGateInputClicked(object sender, EventArgs e)
        {
            Gate gate = (Gate)sender;
            int index = ((IndexEventArgs)e).Index;

            if (drawingcable)
            {
                Point point = new Point();
                point.X = gate.Position.X - 1;
                point.Y = gate.Position.Y + (double)4 * (1 + 2 * index) / (2 * gate.Input.Count);
                Cable lastcable = cables.Last();
                lastcable.AddPoint(point, true);
                lastcable.Input = gate.Input[index];
                lastcable.Output.ConnectTo(lastcable.Input);
                Tick(lastcable.Input);
                drawingcable = false; 
            }
        }
        #endregion
    }
}
