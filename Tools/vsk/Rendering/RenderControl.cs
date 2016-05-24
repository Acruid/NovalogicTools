using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using KeyPressEventArgs = OpenTK.KeyPressEventArgs;

namespace vsk.Rendering
{
    /// <summary>
    /// </summary>
    public partial class RenderControl : GLControl, OpenTK.Platform.IGameWindow
    {
        private readonly Stopwatch _stopwatch = new Stopwatch(); // available to all event handlers
        private Color _clearColor;
        private double _accumulator;
        private int _idleCounter;
        private float _rotation;
        private int _x;
/*
        /// <summary>
        /// </summary>
        public RenderControl() : base(GraphicsMode.Default, 3, 2, GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug)
        {
            InitializeComponent();
        }
*/

        /// <summary>
        /// </summary>
        public RenderControl() : base(GraphicsMode.Default, 1, 0, GraphicsContextFlags.Debug)
        {
            InitializeComponent();
        }

        /// <summary>
        /// </summary>
        [Category("Misc")]
        [Description("MyToolWindowControl properties")]
        public Label FpsLabel { private get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Color ClearColor
        {
            get { return _clearColor; }
            set
            {
                _clearColor = value;

                if (!DesignMode)
                {
                    MakeCurrent();
                    GL.ClearColor(_clearColor);
                }
            }
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.UserControl.Load" /> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data. </param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (DesignMode)
                return;

            Load?.Invoke(this, EventArgs.Empty);

//            SetupViewport();

            Application.Idle += ApplicationOnIdle;

            _stopwatch.Start(); // start at application boot
        }

        private void ApplicationOnIdle(object sender, EventArgs eventArgs)
        {
            // no guard needed -- we hooked into the event in Load handler
            while (IsIdle)
            {
                var milliseconds = ComputeTimeSlice();
                Accumulate(milliseconds);
//                Animate(milliseconds);
                UpdateFrame?.Invoke(this, new FrameEventArgs(milliseconds));
                Invalidate();
            }
        }

        /// <summary>
        /// Raises the Resize event.
        /// Note: this method may be called before the OpenGL context is ready.
        /// Check that IsHandleCreated is true before using any OpenGL methods.
        /// </summary>
        /// <param name="e">A System.EventArgs that contains the event data.</param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (DesignMode || !IsHandleCreated)
                return;

            Resize?.Invoke(this, EventArgs.Empty);
//            SetupViewport();
            Invalidate();
        }

        /// <summary>Raises the System.Windows.Forms.Control.Paint event.</summary>
        /// <param name="e">A System.Windows.Forms.PaintEventArgs that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (!DesignMode && IsHandleCreated)
            {
//                Render();
                RenderFrame?.Invoke(this, new FrameEventArgs());
            }
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.Control.KeyDown" /> event.</summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.KeyEventArgs" /> that contains the event data. </param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode != Keys.Space)
                return;

            _x++;
            Invalidate();
        }

        /// <summary>
        ///     Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Application.Idle -= ApplicationOnIdle;
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        private double ComputeTimeSlice()
        {
            _stopwatch.Stop();
            var timeslice = _stopwatch.Elapsed.TotalMilliseconds;
            _stopwatch.Reset();
            _stopwatch.Start();
            return timeslice;
        }

        private void Animate(double milliseconds)
        {
            var deltaRotation = (float) milliseconds/20.0f;
            _rotation += deltaRotation;
        }

        private void Accumulate(double milliseconds)
        {
            _idleCounter++;
            _accumulator += milliseconds;
            if (!(_accumulator > 1000)) return;
            if (FpsLabel != null && !FpsLabel.IsDisposed)
                FpsLabel.Text = _idleCounter.ToString();
            _accumulator -= 1000;
            _idleCounter = 0; // don't forget to reset the counter!
        }

        private void Render()
        {
            MakeCurrent();

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            GL.Translate(_x, 0, 0); // position triangle according to our x variable

            GL.Color3(Focused ? Color.Yellow : Color.Blue);

            GL.Rotate(_rotation, Vector3.UnitZ); // OpenTK has this nice Vector3 class!
            GL.Begin(PrimitiveType.Triangles);
            GL.Vertex2(10, 20);
            GL.Vertex2(100, 20);
            GL.Vertex2(100, 50);
            GL.End();

            SwapBuffers();
        }

        private void SetupViewport()
        {
            var w = Width;
            var h = Height;
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, w, 0, h, -1, 1); // Bottom-left corner pixel has coordinate (0, 0)
            GL.Viewport(0, 0, w, h); // Use all of the glControl painting area
        }

#region GameWindow
        public void Close()
        {
            throw new NotImplementedException();
        }

        public void ProcessEvents()
        {
            throw new NotImplementedException();
        }

        public Icon Icon { get; set; }
        public string Title { get; set; }
        public bool Exists { get; }
        public WindowState WindowState { get; set; }
        public WindowBorder WindowBorder { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public Rectangle ClientRectangle { get; set; }
        public IInputDriver InputDriver { get; }
        public MouseCursor Cursor { get; set; }
        public bool CursorVisible { get; set; }
        public event EventHandler<EventArgs> Move;
        public event EventHandler<EventArgs> Resize;
        public event EventHandler<CancelEventArgs> Closing;
        public event EventHandler<EventArgs> Closed;
        public event EventHandler<EventArgs> Disposed;
        public event EventHandler<EventArgs> IconChanged;
        public event EventHandler<EventArgs> TitleChanged;
        public event EventHandler<EventArgs> VisibleChanged;
        public event EventHandler<EventArgs> FocusedChanged;
        public event EventHandler<EventArgs> WindowBorderChanged;
        public event EventHandler<EventArgs> WindowStateChanged;
        public event EventHandler<KeyboardKeyEventArgs> KeyDown;
        public event EventHandler<KeyPressEventArgs> KeyPress;
        public event EventHandler<KeyboardKeyEventArgs> KeyUp;
        public event EventHandler<EventArgs> MouseLeave;
        public event EventHandler<EventArgs> MouseEnter;
        public event EventHandler<MouseButtonEventArgs> MouseDown;
        public event EventHandler<MouseButtonEventArgs> MouseUp;
        public event EventHandler<MouseMoveEventArgs> MouseMove;
        public event EventHandler<MouseWheelEventArgs> MouseWheel;
        public void Run()
        {
            throw new NotImplementedException();
        }

        public void Run(double updateRate)
        {
            throw new NotImplementedException();
        }

        public new event EventHandler<EventArgs> Load;
        public event EventHandler<EventArgs> Unload;
        public event EventHandler<FrameEventArgs> UpdateFrame;
        public event EventHandler<FrameEventArgs> RenderFrame;
#endregion
    }
}