using System;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace _3DGame.Loaders
{
    public class ObjMesh
    {
        private ObjVertex[] _vertices;
        private int _lightID;
        private int _m3X3ID;
        private int _mMatrixID;
        private int _matrixID;
        private ObjQuad[] _quads;
        private int _quadsBufferId;
        private ObjTriangle[] _triangles;
        private int _trianglesBufferId;
        private int _vMatrixID;
        private int _verticesBufferId;

        public ObjMesh(string fileName)
        {
            ObjMeshLoader.Load(this, fileName);
        }

        public ObjVertex[] Vertices
        {
            get { return _vertices; }
            set { _vertices = value; }
        }

        public ObjVertex[] TexCoords { get; set; }

        public ObjTriangle[] Triangles
        {
            get { return _triangles; }
            set { _triangles = value; }
        }

        public ObjQuad[] Quads
        {
            get { return _quads; }
            set { _quads = value; }
        }

        public ObjTexture[] Textures { get; set; }

        public void Prepare()
        {
            if (_verticesBufferId == 0)
            {
                GL.GenBuffers(1, out _verticesBufferId);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _verticesBufferId);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr) (_vertices.Length*Marshal.SizeOf(typeof (ObjVertex))),
                    _vertices, BufferUsageHint.StaticDraw);
            }

            if (_trianglesBufferId == 0)
            {
                GL.GenBuffers(1, out _trianglesBufferId);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, _trianglesBufferId);
                GL.BufferData(BufferTarget.ElementArrayBuffer,
                    (IntPtr) (_triangles.Length*Marshal.SizeOf(typeof (ObjTriangle))), _triangles,
                    BufferUsageHint.StaticDraw);
            }

            if (_quadsBufferId == 0)
            {
                GL.GenBuffers(1, out _quadsBufferId);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, _quadsBufferId);
                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr) (_quads.Length*Marshal.SizeOf(typeof (ObjQuad))),
                    _quads, BufferUsageHint.StaticDraw);
            }

            _lightID = GL.GetUniformLocation(Game.PgmID, "LightPosition_worldspace");
            _matrixID = GL.GetUniformLocation(Game.PgmID, "MVP");
            _vMatrixID = GL.GetUniformLocation(Game.PgmID, "V");
            _mMatrixID = GL.GetUniformLocation(Game.PgmID, "M");
            _m3X3ID = GL.GetUniformLocation(Game.PgmID, "MV3x3");
        }

        public void Render(Matrix4 translation)
        {
            Prepare();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.EnableClientState(ArrayCap.VertexArray);

            Matrix4 model = Matrix4.Identity;
            var m3X3 = new Matrix3(model);
            Matrix4 mvp = model*translation*Game.View*Game.Projection;

            GL.UniformMatrix4(_matrixID, 1, false, ref mvp.Row0.X);
            GL.UniformMatrix4(_mMatrixID, 1, false, ref model.Row0.X);
            GL.UniformMatrix4(_vMatrixID, 1, false, ref Controls.View.Row0.X);
            GL.UniformMatrix4(_vMatrixID, 1, false, ref Controls.View.Row0.X);
            GL.UniformMatrix3(_m3X3ID, 1, false, ref m3X3.Row0.X);

            var lightPos = new Vector3(0, 0, 4);
            GL.Uniform3(_lightID, lightPos.X, lightPos.Y, lightPos.Z);

            for (int i = 0; i < Textures.Length; i++)
            {
                GL.ActiveTexture(Textures[i].TextureNumber);
                GL.BindTexture(Textures[i].TextureTarg, Textures[i].TextureId);
                if (Textures[i].TextureUniform == "diffuse")
                    GL.Uniform1(GL.GetUniformLocation(Game.PgmID, Textures[i].TextureUniform), 0);
                else if (Textures[i].TextureUniform == "normal")
                    GL.Uniform1(GL.GetUniformLocation(Game.PgmID, Textures[i].TextureUniform), 1);
                else if (Textures[i].TextureUniform == "specular")
                    GL.Uniform1(GL.GetUniformLocation(Game.PgmID, Textures[i].TextureUniform), 2);
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, _verticesBufferId);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 14*sizeof (float),
                (IntPtr) (5*sizeof (float)));

            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 14*sizeof (float), (IntPtr) (0));

            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 14*sizeof (float),
                (IntPtr) (2*sizeof (float)));

            GL.EnableVertexAttribArray(3);
            GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, 14*sizeof (float),
                (IntPtr) (8*sizeof (float)));

            GL.EnableVertexAttribArray(4);
            GL.VertexAttribPointer(4, 3, VertexAttribPointerType.Float, false, 14*sizeof (float),
                (IntPtr) (11*sizeof (float)));

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _trianglesBufferId);
            GL.DrawElements(BeginMode.Triangles, _triangles.Length*3, DrawElementsType.UnsignedInt, IntPtr.Zero);

            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);
            GL.DisableVertexAttribArray(2);
            GL.DisableVertexAttribArray(3);
            GL.DisableVertexAttribArray(4);

            if (_quads.Length > 0)
            {
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, _quadsBufferId);
                GL.DrawElements(BeginMode.Quads, _quads.Length*4, DrawElementsType.UnsignedInt, IntPtr.Zero);
            }

            GL.DisableClientState(ArrayCap.VertexArray);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ObjQuad
        {
            public int Index0;
            public int Index1;
            public int Index2;
            public int Index3;
        }

        public struct ObjTexture
        {
            public uint TextureId;
            public TextureUnit TextureNumber;
            public TextureTarget TextureTarg;
            public string TextureUniform;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ObjTriangle
        {
            public int Index0;
            public int Index1;
            public int Index2;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ObjVertex
        {
            public Vector2 TexCoord;
            public Vector3 Normal;
            public Vector3 Vertex;
            public Vector3 Tangent;
            public Vector3 Bitangent;
        }
    }
}