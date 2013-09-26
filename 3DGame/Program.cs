using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using _3DGame.Loaders;

namespace _3DGame
{
    internal class Game : GameWindow
    {
        #region initdata

        public static int PgmID; // Pointer to GLSL Program

        public static Matrix4 Projection;
        public static Matrix4 View;

        public static Point Center;
        public static TimeSpan DeltaTime = new TimeSpan();
        private readonly Controls _controls = new Controls();
        private readonly List<ObjMesh> _meshes = new List<ObjMesh>();

        private DateTime _currentTime = DateTime.Now;
        private int _fsID; // Pointer to Fragment Shader
        private DateTime _previousTime = DateTime.Now;
        private int _vao; // Pointer to VAO
        private int _vsID; // Pointer to Vertex Shader

        #endregion

        public Game()
            : base(800, 600, new GraphicsMode(32, 24, 0, 4), "3D")
        {
            VSync = VSyncMode.On;
            Cursor.Position = new Point(Center.X, Center.Y);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            InitProgram();

            Center = new Point((Bounds.Left + Bounds.Right)/2, (Bounds.Top + Bounds.Bottom)/2);

            Keyboard.KeyDown += _controls.Keyboard_KeyDown;
            Keyboard.KeyUp += _controls.Keyboard_KeyUp;
            Mouse.Move += _controls.Mouse_Move;

            GL.ClearColor(0.1f, 0.1f, 0.1f, 0.0f);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);
        }

        private void InitProgram()
        {
            GL.GenVertexArrays(1, out _vao);
            GL.BindVertexArray(_vao);

            PgmID = GL.CreateProgram();
            LoadShader("Shaders/vs.glsl", ShaderType.VertexShader, PgmID, out _vsID);
            LoadShader("Shaders/fs.glsl", ShaderType.FragmentShader, PgmID, out _fsID);
            GL.LinkProgram(PgmID);
            Console.WriteLine(GL.GetProgramInfoLog(PgmID));

            GL.DeleteShader(_vsID);
            GL.DeleteShader(_fsID);

            string[] dirs = Directory.GetFiles("Meshes/");
            foreach (string file in dirs)
            {
                _meshes.Add(new ObjMesh(file));
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
            var projection = Matrix4.CreatePerspectiveFieldOfView((float) Math.PI/4, Width/(float) Height, 1.0f,
                64.0f);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);
        }

        /// Called when it is time to setup the next frame. Add you game logic here.
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            _previousTime = _currentTime;
            _currentTime = DateTime.Now;
            DeltaTime = _currentTime.Subtract(_previousTime);

            _controls.compute_matrices_from_input();
            Projection = _controls.GetProjection();
            View = _controls.GetViewMatrix();

            base.OnUpdateFrame(e);
        }

        /// Called when it is time to render the next frame. Add your rendering code here.
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(PgmID);

            int i = 0;
            foreach (ObjMesh mesh in _meshes)
            {
                var translation = Matrix4.CreateTranslation(i, i, i);
                mesh.Render(translation);
                i++;
            }

            GL.Flush();

            SwapBuffers();
        }

        /// The main entry point for the application.
        [STAThread]
        private static void Main()
        {
            // The 'using' idiom guarantees proper resource cleanup.
            // We request 30 UpdateFrame events per second, and unlimited
            // RenderFrame events (as fast as the computer can handle).
            using (var game = new Game())
            {
                game.Run(60.0);
            }
        }

        #region load

        private static void LoadShader(String filename, ShaderType type, int program, out int address)
        {
            address = GL.CreateShader(type);

            using (var sr = new StreamReader(filename))
            {
                GL.ShaderSource(address, sr.ReadToEnd());
            }

            GL.CompileShader(address);
            GL.AttachShader(program, address);
            Console.WriteLine(GL.GetShaderInfoLog(address));
        }

        #endregion
    }
}