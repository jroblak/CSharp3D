using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Input;

namespace _3DGame
{
    class Controls
    {
        private Vector3 _position = new Vector3(0, 0, 5);
        private Vector3 _right = new Vector3(0, 0, 0);
        private Vector3 _direction = new Vector3(0, 0, 0);
        private Vector3 _up = new Vector3(0f, 1f, 0f);
        private Vector2 _mouseDelta = new Vector2(0f, 0f);

        private float _horzAngle = 3.14f;
        private float _vertAngle;
        private const float InitFov = 45.0f;
        private const float Speed = 3.0f;
        private const float MouseSpeed = 0.1f;
        private float _fov;

        public static Matrix4 Projection = new Matrix4();
        public static Matrix4 View = new Matrix4();

        public List<string> Movements = new List<string>();

        public Controls()
        {
            Cursor.Hide();
            Cursor.Position = new Point(Game.Center.X, Game.Center.Y);
        }

        public void compute_matrices_from_input()
        {
            Cursor.Position = new Point(Game.Center.X, Game.Center.Y);

            _horzAngle -= MouseSpeed * (float)Game.DeltaTime.TotalSeconds * _mouseDelta.X;
            _vertAngle -= MouseSpeed * (float)Game.DeltaTime.TotalSeconds * _mouseDelta.Y;

            _right.X = (float)Math.Sin(_horzAngle - 3.14f / 2.0f);
            _right.Y = 0;
            _right.Z = (float)Math.Cos(_horzAngle - 3.14f / 2.0f);

            _direction.X = (float)(Math.Cos(_vertAngle) * Math.Sin(_horzAngle));
            _direction.Y = (float)Math.Sin(_vertAngle);
            _direction.Z = (float)(Math.Cos(_vertAngle) * Math.Cos(_horzAngle));

            _up = Vector3.Cross(_right, _direction);

            _fov = InitFov - (5 * Mouse.GetState().ScrollWheelValue);

            if (Movements.Count > 0) Move();

            Projection = Matrix4.CreatePerspectiveFieldOfView(((float)(Math.PI * _fov) / 180), 4.0f / 3.0f, 0.1f, 100.0f);

            View = Matrix4.LookAt(_position, _position + _direction, _up);
        }

        public void Mouse_Move(object sender, MouseMoveEventArgs e)
        {
            _mouseDelta = new Vector2(e.XDelta, e.YDelta);
        }

        public Matrix4 GetProjection()
        {
            return Projection;
        }

        public Matrix4 GetViewMatrix()
        {
            return View;
        }

        #region controls

        public void Keyboard_KeyUp(object sender, KeyboardKeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.A:
                    if (Movements.IndexOf("Left") >= 0) Movements.Remove("Left");
                    break;
                case Key.D:
                    if (Movements.IndexOf("Right") >= 0) Movements.Remove("Right");
                    break;
                case Key.W:
                    if (Movements.IndexOf("Forward") >= 0) Movements.Remove("Forward");
                    break;
                case Key.S:
                    if (Movements.IndexOf("Backward") >= 0) Movements.Remove("Backward");
                    break;
            }
        }

        public void Keyboard_KeyDown(object sender, KeyboardKeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.A:
                    if (Movements.IndexOf("Left") < 0) Movements.Add("Left");
                    break;
                case Key.D:
                    if (Movements.IndexOf("Right") < 0) Movements.Add("Right");
                    break;
                case Key.W:
                    if (Movements.IndexOf("Forward") < 0) Movements.Add("Forward");
                    break;
                case Key.S:
                    if (Movements.IndexOf("Backward") < 0) Movements.Add("Backward");
                    break;
            }
        }

        #endregion

        public void Move()
        {
            if (Movements.IndexOf("Left") > -1) _position -= _right * (float)Game.DeltaTime.TotalSeconds * Speed;
            if (Movements.IndexOf("Right") > -1) _position += _right * (float)Game.DeltaTime.TotalSeconds * Speed;
            if (Movements.IndexOf("Forward") > -1) _position += _direction * (float)Game.DeltaTime.TotalSeconds * Speed;
            if (Movements.IndexOf("Backward") > -1) _position -= _direction * (float)Game.DeltaTime.TotalSeconds * Speed;
        }

    }
}
