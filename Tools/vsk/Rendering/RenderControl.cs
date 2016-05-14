using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace vsk.Rendering
{
    /// <summary>
    /// 
    /// </summary>
    public partial class RenderControl : GLControl
    {
        Color _clearColor;
        readonly Stopwatch _stopwatch = new Stopwatch(); // available to all event handlers

        /// <summary>
        /// 
        /// </summary>
        public RenderControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        [Category("Misc")]
        [Description("MyToolWindowControl properties")]
        public Label FpsLabel { private get; set; }

        public Color ClearColor
        {
            get { return _clearColor; }
            set
            {
                _clearColor = value;

                if (!this.DesignMode)
                {
                    MakeCurrent();
                    GL.ClearColor(_clearColor);
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(DesignMode)
                return;

            SetupViewport();

            Application.Idle += ApplicationOnIdle;

            _stopwatch.Start(); // start at application boot
        }
        
        private void ApplicationOnIdle(object sender, EventArgs eventArgs)
        {
            // no guard needed -- we hooked into the event in Load handler
            while (this.IsIdle)
            {

                double milliseconds = ComputeTimeSlice();
                Accumulate(milliseconds);
                Animate(milliseconds);

                this.Invalidate();
            }
        }

        protected override void OnResize(System.EventArgs e)
        {
            base.OnResize(e);

            if (!DesignMode && IsHandleCreated)
            {
                SetupViewport();
                this.Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (!DesignMode && IsHandleCreated)
            {
                Render();
            }
        }

        int x = 0;
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Space)
            {
                x++;
                this.Invalidate();
            }
        }

        private double ComputeTimeSlice()
        {
            _stopwatch.Stop();
            double timeslice = _stopwatch.Elapsed.TotalMilliseconds;
            _stopwatch.Reset();
            _stopwatch.Start();
            return timeslice;
        }

        float rotation = 0;
        private void Animate(double milliseconds)
        {
            float deltaRotation = (float)milliseconds / 20.0f;
            rotation += deltaRotation;
        }

        double accumulator = 0;
        int idleCounter = 0;
        private void Accumulate(double milliseconds)
        {
            idleCounter++;
            accumulator += milliseconds;
            if (accumulator > 1000)
            {
                if(FpsLabel != null && !FpsLabel.IsDisposed)
                    FpsLabel.Text = idleCounter.ToString();
                accumulator -= 1000;
                idleCounter = 0; // don't forget to reset the counter!
            }
        }

        private void Render()
        {
            MakeCurrent();

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            GL.Translate(x, 0, 0); // position triangle according to our x variable

            if (this.Focused) // Simple enough :)
                GL.Color3(Color.Yellow);
            else
                GL.Color3(Color.Blue);

            GL.Rotate(rotation, Vector3.UnitZ); // OpenTK has this nice Vector3 class!
            GL.Begin(BeginMode.Triangles);
            GL.Vertex2(10, 20);
            GL.Vertex2(100, 20);
            GL.Vertex2(100, 50);
            GL.End();

            SwapBuffers();
        }

        private void SetupViewport()
        {
            int w = this.Width;
            int h = this.Height;
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, w, 0, h, -1, 1); // Bottom-left corner pixel has coordinate (0, 0)
            GL.Viewport(0, 0, w, h); // Use all of the glControl painting area
        }
    }
}