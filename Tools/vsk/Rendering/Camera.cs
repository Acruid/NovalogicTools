using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Input;

namespace Render
{
    internal class Camera
    {

        public static Vector3 Up = new Vector3(0, 0, 1);

        // position
        private Vector3 position = new Vector3(-2.0f, 0, 1.0f);
        private Vector3 center = new Vector3(0,0,0.5f);
        // horizontal angle : toward -Z
        private float yaw = 3.14f;
        // vertical angle : 0, look at the horizon
        private float pitch = 0.0f;
        // Initial Field of View
        private float initialFoV = 45.0f;

        private float speed = 3.0f; // 3 units / second
        private float mouseSpeed = 0.005f;

        private Vector2 mousePos;

        public bool Think(double deltaTime)
        {
            var keyboard = Keyboard.GetState();
            var mouse = Mouse.GetState();

            Vector2 newMouse = new Vector2(mouse.X, mouse.Y);
            Vector2 mouseDelta = mousePos - newMouse;
            mousePos = newMouse;

            bool refresh = false;
            Matrix4 rotation = Matrix4.Identity;

            // Get mouse position
            int xPos = mouse.X;
            int yPos = mouse.Y;

            if (mouse[MouseButton.Left])
            {
                refresh = true;

                // Compute new orientation
                yaw += mouseSpeed* mouseDelta.X;
                pitch -= mouseSpeed*mouseDelta.Y;

                Vector3 direction = new Vector3(
                    (float) (Math.Cos(yaw)*Math.Cos(pitch)),
                    (float)(Math.Sin(yaw) * Math.Cos(pitch)),
                    (float) (Math.Sin(pitch))
                );

                Vector3 lookatPoint = new Vector3((float)Math.Cos(yaw), (float)Math.Sin(pitch), (float)Math.Sin(yaw));
/*
                // Camera matrix
                ModelView = Matrix4.LookAt(
                    position,           // Camera is here
                    position - direction, // and looks here : at the same position, plus "direction"
                    up                  // Head is up (set to 0,-1,0 to look upside-down)
                );
*/
                ModelView = Matrix4.LookAt(Vector3.Zero + Vector3.Multiply(direction, 2), center, Up);
            }
/*
            if (keyboard[Key.Left])
            {
                refresh = true;
                rotation = Matrix4.CreateRotationZ((float) e.Time);
                Matrix4.Mult(ref rotation, ref _modelviewMatrix, out _modelviewMatrix);
            }
            else if (keyboard[Key.Right])
            {
                refresh = true;
                rotation = Matrix4.CreateRotationZ(-1*(float) e.Time);
                Matrix4.Mult(ref rotation, ref _modelviewMatrix, out _modelviewMatrix);
            }

            if (keyboard[Key.Up])
            {
                refresh = true;
                rotation = Matrix4.CreateRotationX((float) e.Time);
                Matrix4.Mult(ref rotation, ref _modelviewMatrix, out _modelviewMatrix);
            }
            else if (keyboard[Key.Down])
            {
                refresh = true;
                rotation = Matrix4.CreateRotationX(-1*(float) e.Time);
                Matrix4.Mult(ref rotation, ref _modelviewMatrix, out _modelviewMatrix);
            }

            if (keyboard[Key.Space])
            {
                refresh = true;
                _modelviewMatrix = Matrix4.LookAt(_spawn, _center, _up);
            }
*/
            return refresh;
        }

        public Matrix4 ModelView { get; private set; }
    }
}