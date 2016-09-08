using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace vsk.Rendering
{
    /// <summary>
    /// </summary>
    public partial class RenderControl : GLControl
    {
        private readonly Stopwatch _stopwatch = new Stopwatch(); // available to all event handlers
        private Color _clearColor;
        private double _accumulator;
        private int _idleCounter;

        public event EventHandler<FrameEventArgs> UpdateFrame;
        public event EventHandler<FrameEventArgs> RenderFrame;

        /// <summary>
        /// </summary>
        public RenderControl() : base(GraphicsMode.Default, 4, 3, GraphicsContextFlags.Debug)
        {
            InitializeComponent();
        }

        /// <summary>
        /// </summary>
        [Category("Misc")]
        [Description("MyToolWindowControl properties")]
        public Label FpsLabel { get; set; }

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
            
            Application.Idle += ApplicationOnIdle;
            
            Cursor.Show();

            _stopwatch.Start();
        }

        private void ApplicationOnIdle(object sender, EventArgs eventArgs)
        {
            // no guard needed -- we hooked into the event in Load handler
            while (IsIdle)
            {
                var milliseconds = ComputeTimeSlice();
                Accumulate(milliseconds);
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

//            Resize?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }

        /// <summary>Raises the System.Windows.Forms.Control.Paint event.</summary>
        /// <param name="e">A System.Windows.Forms.PaintEventArgs that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (!DesignMode && IsHandleCreated)
            {
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

        private void Accumulate(double milliseconds)
        {
            _idleCounter++;
            _accumulator += milliseconds;
            if (!(_accumulator > 1000)) return;
            if (FpsLabel != null && !FpsLabel.IsDisposed)
                FpsLabel.Text = _idleCounter.ToString();
            _accumulator -= 1000;
            _idleCounter = 0;
        }
    }
}