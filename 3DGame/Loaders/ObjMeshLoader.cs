using System.Collections.Generic;
using System.IO;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace _3DGame.Loaders
{
    public class ObjMeshLoader
    {
        public static bool Load(ObjMesh mesh, string fileName)
        {
            try
            {
                using (var streamReader = new StreamReader(fileName))
                {
                    Load(mesh, streamReader);
                    LoadTextures(mesh, fileName);
                    streamReader.Close();
                    return true;
                }
            }
            catch { return false; }
        }

        static readonly char[] SplitCharacters = { ' ' };

        static List<Vector3> _vertices;
        static List<Vector3> _normals;
        static List<Vector2> _texCoords;
        static Dictionary<ObjMesh.ObjVertex, int> _objVerticesIndexDictionary;
        static List<ObjMesh.ObjVertex> _objVertices;
        static List<ObjMesh.ObjTriangle> _objTriangles;
        static List<ObjMesh.ObjQuad> _objQuads;

        static void Load(ObjMesh mesh, TextReader textReader)
        {
            _vertices = new List<Vector3>();
            _normals = new List<Vector3>();
            _texCoords = new List<Vector2>();
            _objVertices = new List<ObjMesh.ObjVertex>();
            _objTriangles = new List<ObjMesh.ObjTriangle>();
            _objVerticesIndexDictionary = new Dictionary<ObjMesh.ObjVertex, int>();
            _objQuads = new List<ObjMesh.ObjQuad>();

            string line;
            while ((line = textReader.ReadLine()) != null)
            {
                line = line.Trim(SplitCharacters);
                line = line.Replace("  ", " ");

                string[] parameters = line.Split(SplitCharacters);

                switch (parameters[0])
                {
                    case "p": // Point
                        break;

                    case "v": // Vertex
                        float x = float.Parse(parameters[1]);
                        float y = float.Parse(parameters[2]);
                        float z = float.Parse(parameters[3]);

                        _vertices.Add(new Vector3(x, y, z));
                        break;

                    case "vt": // TexCoord
                        float u = float.Parse(parameters[1]);
                        float v = float.Parse(parameters[2]);
                        _texCoords.Add(new Vector2(u, v));
                        break;

                    case "vn": // Normal
                        float nx = float.Parse(parameters[1]);
                        float ny = float.Parse(parameters[2]);
                        float nz = float.Parse(parameters[3]);
                        _normals.Add(new Vector3(nx, ny, nz));
                        break;

                    case "f":
                        switch (parameters.Length)
                        {
                            case 4:
                                var objTriangle = new ObjMesh.ObjTriangle
                                {
                                    Index0 = ParseFaceParameter(parameters[1]),
                                    Index1 = ParseFaceParameter(parameters[2]),
                                    Index2 = ParseFaceParameter(parameters[3])
                                };
                                _objTriangles.Add(objTriangle);
                                break;

                            case 5:
                                var objQuad = new ObjMesh.ObjQuad
                                {
                                    Index0 = ParseFaceParameter(parameters[1]),
                                    Index1 = ParseFaceParameter(parameters[2]),
                                    Index2 = ParseFaceParameter(parameters[3]),
                                    Index3 = ParseFaceParameter(parameters[4])
                                };
                                _objQuads.Add(objQuad);
                                break;
                        }
                        break;
                }
            }

            // FillTangentData(); // Fill the triangles with tangents / bitangents
            // IndexTangentsFix(); // Attempt to average the tangents / bitangents
            
            mesh.Vertices = _objVertices.ToArray();
            mesh.Triangles = _objTriangles.ToArray();
            mesh.Quads = _objQuads.ToArray();

            _objVerticesIndexDictionary = null;
            _vertices = null;
            _normals = null;
            _texCoords = null;
            _objVertices = null;
            _objTriangles = null;
            _objQuads = null;
        }

        static void IndexTangentsFix()
        {
            // Loop through every Triangle
            foreach (ObjMesh.ObjTriangle triangle in _objTriangles)
            {
                int index0, index1, index2;

                // Get the three vertices for the Triangle
                var vert0 = _objVertices[triangle.Index0];
                var vert1 = _objVertices[triangle.Index1];
                var vert2 = _objVertices[triangle.Index2];

                /* The logic here:
                 * If the vertex exists in the _objVerticesIndexDictionary, then the vertex is shared.
                 * Therefore, we get that shared vertex and average the tangents / bitangents 
                */

                if (_objVerticesIndexDictionary.TryGetValue(vert0, out index0))
                {
                    var othervert = _objVertices[index0];

                    var combTangent = vert0.Tangent + othervert.Tangent;
                    var combBitangent = vert0.Bitangent + othervert.Bitangent;

                    vert0.Tangent = combTangent;
                    vert0.Bitangent = combBitangent;

                    othervert.Tangent = combTangent;
                    othervert.Bitangent = combBitangent;
                }
                
                if (_objVerticesIndexDictionary.TryGetValue(vert1, out index1))
                {
                    var othervert = _objVertices[index1];

                    var combTangent = vert1.Tangent + othervert.Tangent;
                    var combBitangent = vert1.Bitangent + othervert.Bitangent;

                    vert1.Tangent = combTangent;
                    vert1.Bitangent = combBitangent;

                    othervert.Tangent = combTangent;
                    othervert.Bitangent = combBitangent;
                }
                
                if (_objVerticesIndexDictionary.TryGetValue(vert2, out index2))
                {
                    var othervert = _objVertices[index2];

                    var combTangent = vert2.Tangent + othervert.Tangent;
                    var combBitangent = vert2.Bitangent + othervert.Bitangent;

                    vert2.Tangent = combTangent;
                    vert2.Bitangent = combBitangent;

                    othervert.Tangent = combTangent;
                    othervert.Bitangent = combBitangent;
                }
            }
        }

        static void FillTangentData()
        {
            // Loop through the triangles and calculate the tangents and bitangents
            foreach (ObjMesh.ObjTriangle triangle in _objTriangles) {

                ObjMesh.ObjVertex vert0 = _objVertices[triangle.Index0];
                ObjMesh.ObjVertex vert1 = _objVertices[triangle.Index1];
                ObjMesh.ObjVertex vert2 = _objVertices[triangle.Index2];

                Vector3 v0 = vert0.Vertex;
                Vector3 v1 = vert1.Vertex;
                Vector3 v2 = vert2.Vertex;

                Vector2 uv0 = vert0.TexCoord;
                Vector2 uv1 = vert1.TexCoord;
                Vector2 uv2 = vert2.TexCoord;

                Vector3 deltaPos1 = v1 - v0;
                Vector3 deltaPos2 = v2 - v0;

                Vector2 deltaUv1 = uv1 - uv0;
                Vector2 deltaUv2 = uv2 - uv0;

                float r = 1.0f / (deltaUv1.X * deltaUv2.Y - deltaUv1.Y * deltaUv2.X);
                Vector3 tangent = (deltaPos1 * deltaUv2.Y - deltaPos2 * deltaUv1.Y) * r;
                Vector3 bitangent = (deltaPos2 * deltaUv1.X - deltaPos1 * deltaUv2.X) * r;

                vert0.Tangent = tangent;
                vert1.Tangent = tangent;
                vert2.Tangent = tangent;
                
                vert0.Bitangent = bitangent;
                vert1.Bitangent = bitangent;
                vert2.Bitangent = bitangent;
            }
        }

        static readonly char[] FaceParamaterSplitter = { '/' };
        static int ParseFaceParameter(string faceParameter)
        {
            var texCoord = new Vector2();
            var normal = new Vector3();

            string[] parameters = faceParameter.Split(FaceParamaterSplitter);

            int vertexIndex = int.Parse(parameters[0]);
            if (vertexIndex < 0) vertexIndex = _vertices.Count + vertexIndex;
            else vertexIndex = vertexIndex - 1;
            Vector3 vertex = _vertices[vertexIndex];

            if (parameters.Length > 1)
            {
                int texCoordIndex = int.Parse(parameters[1]);
                if (texCoordIndex < 0) texCoordIndex = _texCoords.Count + texCoordIndex;
                else texCoordIndex = texCoordIndex - 1;
                texCoord = _texCoords[texCoordIndex];
            }

            if (parameters.Length > 2)
            {
                int normalIndex = int.Parse(parameters[2]);
                if (normalIndex < 0) normalIndex = _normals.Count + normalIndex;
                else normalIndex = normalIndex - 1;
                normal = _normals[normalIndex];
            }

            return AddObjVertex(ref vertex, ref texCoord, ref normal);
        }

        static void LoadTextures(ObjMesh mesh, string filename)
        {
            string texdir = Path.GetFileNameWithoutExtension(filename);
            string fulldir = @"Textures/"+texdir+@"/";
            string[] files = Directory.GetFiles(fulldir);
            var textureList = new List<ObjMesh.ObjTexture>();

            foreach (string file in files)
            {
                string textureUniform = Path.GetFileNameWithoutExtension(file);
                uint textureId;
                TextureTarget textureTarget;
              
                ImageDDS.LoadFromDisk(file, out textureId, out textureTarget);

                GL.BindTexture(textureTarget, textureId);
                GL.TexParameter(textureTarget, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

                var mipMapCount = new int[1];
                GL.GetTexParameter(textureTarget, GetTextureParameter.TextureMaxLevel, out mipMapCount[0]);

                if (mipMapCount[0] == 0) GL.TexParameter(textureTarget, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                else GL.TexParameter(textureTarget, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);

                var newTexture = new ObjMesh.ObjTexture
                {
                    TextureId = textureId,
                    TextureTarg = textureTarget,
                    TextureUniform = textureUniform
                };

                switch (textureUniform)
                {
                    case "diffuse":
                        newTexture.TextureNumber = TextureUnit.Texture0;
                        break;
                    case "normal":
                        newTexture.TextureNumber = TextureUnit.Texture1;
                        break;
                    case "specular":
                        newTexture.TextureNumber = TextureUnit.Texture2;
                        break;
                }

                textureList.Add(newTexture);
            }

            mesh.Textures = textureList.ToArray();
        }

        static int AddObjVertex(ref Vector3 vertex, ref Vector2 texCoord, ref Vector3 normal)
        {
            var newObjVertex = new ObjMesh.ObjVertex
            {
                Vertex = vertex,
                TexCoord = texCoord,
                Normal = normal,
                Bitangent = new Vector3(0),
                Tangent = new Vector3(0)
            };

            int index;
            if (_objVerticesIndexDictionary.TryGetValue(newObjVertex, out index))
            {
                return index;
            }
            
            _objVertices.Add(newObjVertex);
            _objVerticesIndexDictionary[newObjVertex] = _objVertices.Count - 1;
            return _objVertices.Count - 1;
        }
    }
}