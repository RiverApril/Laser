using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Laser {

    [Flags]
    enum Sides {
        none = 0,
        front = 1,
        back = 1 << 1,
        left = 1 << 2,
        right = 1 << 3,
        top = 1 << 4,
        bottom = 1 << 5,
        xLow = left,
        xHigh = right,
        zLow = front,
        zHigh = back,
        yLow = bottom,
        yHigh = top,
        xBoth = xLow | xHigh,
        yBoth = yLow | yHigh,
        zBoth = zLow | zHigh,
        high = xHigh | yHigh | zHigh,
        low = xLow | yLow | zLow,
        all = front | back | left | right | top | bottom


    }

    public class RenderGroup {
        // Was called Vbo, but then I realized that was an inacuate name.

        private BeginMode beginMode = BeginMode.Quads;

        private int bufferIdVertices;
        private int bufferIdColors;
        private int bufferIdIndices;
        private int bufferIdTextureCoords;

        private List<Vector3> vertexList;
        private List<Vector4> colorList;
        private List<uint> indexList;
        private List<Vector2> textureCoordList;
        private List<Vector3> normalList;

        private Vector3[] vertices;
        private Vector4[] colors;
        private uint[] indices;
        private Vector2[] textureCoords;
        private Vector3[] normals;

        uint i = 0;

        private int indicesCount;
        private int verticesCount;

        private bool useColor;
        private bool useTexture;

        public static float textureSplit = 1f / 32f;

        private static Vector4[] justWhite = new Vector4[] { new Vector4(1, 1, 1, 1) };
        private static Vector2[] defaultTextureCoords = new Vector2[4] { new Vector2(0, 0), new Vector2(textureSplit, 0), new Vector2(textureSplit, textureSplit), new Vector2(0, textureSplit) };


        private bool renderEnabled = false;

        private uint ibo;
        private uint vao;

        private uint vboVertices;
        private uint vboColors;
        private uint vboTextureCoords;
        private uint vboNormals;

        private uint pId;

        public RenderGroup(Game game) {
            pId = (uint)game.glProgram;
        }

        internal void begin(BeginMode beginMode, bool useColor, bool useTexture) {

            vertexList = new List<Vector3>();
            if (useColor) {
                colorList = new List<Vector4>();
            }
            if (useTexture) {
                textureCoordList = new List<Vector2>();
            }
            normalList = new List<Vector3>();
            indexList = new List<uint>();

            this.beginMode = beginMode;
            this.useColor = useColor;
            this.useTexture = useTexture;

            renderEnabled = false;

            i = 0;

            GL.DeleteBuffers(1, ref vboVertices);
            GL.DeleteBuffers(1, ref vboColors);
            GL.DeleteBuffers(1, ref vboTextureCoords);
            GL.DeleteBuffers(1, ref ibo);
            GL.DeleteVertexArrays(1, ref vao);
        }

        internal void addCubeColorsAtCorners(Vector3 a, Vector3 b, Vector4[] colors) {
            vertexList.Add(new Vector3(b.X, b.Y, b.Z));// 0
            vertexList.Add(new Vector3(a.X, b.Y, b.Z));// 1
            vertexList.Add(new Vector3(a.X, a.Y, b.Z));// 2
            vertexList.Add(new Vector3(b.X, a.Y, b.Z));// 3
            vertexList.Add(new Vector3(b.X, b.Y, a.Z));// 4
            vertexList.Add(new Vector3(a.X, b.Y, a.Z));// 5
            vertexList.Add(new Vector3(a.X, a.Y, a.Z));// 6
            vertexList.Add(new Vector3(b.X, a.Y, a.Z));// 7

            for (int i = 0; i < colors.Length; i++) {
                colorList.Add(colors[i]);
            }

            /*
             * 
             *     4 ----- 5
             *   / |     / |
             * 0 ----- 1   |
             * |   |   |   |
             * |   7 --|-- 6
             * | /     | /
             * 3 ----- 2
             * 
             * 
             */

            //Front
            indexList.Add(0);
            indexList.Add(1);
            indexList.Add(2);
            indexList.Add(3);

            //Back
            indexList.Add(5);
            indexList.Add(4);
            indexList.Add(7);
            indexList.Add(6);

            //Right
            indexList.Add(1);
            indexList.Add(5);
            indexList.Add(6);
            indexList.Add(2);

            //Left
            indexList.Add(4);
            indexList.Add(0);
            indexList.Add(3);
            indexList.Add(7);

            //Top
            indexList.Add(4);
            indexList.Add(5);
            indexList.Add(1);
            indexList.Add(0);

            //Bottom
            indexList.Add(3);
            indexList.Add(2);
            indexList.Add(6);
            indexList.Add(7);
        }

        internal void addCube(Vector3 a, Vector3 b, Vector4[] colors = null, Vector2[] textureCoords = null) {
            addCube(a, b, Sides.all, colors, textureCoords);
        }

        internal void addCube(Vector3 a, Vector3 b, Sides sides, Vector4[] colors = null, Vector2[] textureCoords = null) {
            addCube(a, b, sides.HasFlag(Sides.front), sides.HasFlag(Sides.back), sides.HasFlag(Sides.left), sides.HasFlag(Sides.right), sides.HasFlag(Sides.top), sides.HasFlag(Sides.bottom), colors, textureCoords);
        }

        internal void addCube(Vector3 a, Vector3 b, bool front, bool back, bool left, bool right, bool top, bool bottom, Vector4[] colors = null, Vector2[] textureCoords = null) {

            if(colors == null && useColor){
                colors = justWhite;
            }

            if (textureCoords == null && useTexture) {
                textureCoords = defaultTextureCoords;
            }

            uint c = 0;
            uint t = 0;

            bool cc = colors.Length > 1;
            bool tt = textureCoords.Length > 4;

            /* Front & Back
             * 
             *     4 ----- 5
             *   / |     / |
             * 0 ----- 1   |
             * |   |   |   |
             * |   7 --|-- 6
             * | /     | /
             * 3 ----- 2
             * 
             * 
             */

            //Front
            if (front) {

                normalList.Add(new Vector3(0, 0, 1));

                vertexList.Add(new Vector3(b.X, b.Y, b.Z));// 0
                vertexList.Add(new Vector3(a.X, b.Y, b.Z));// 1
                vertexList.Add(new Vector3(a.X, a.Y, b.Z));// 2
                vertexList.Add(new Vector3(b.X, a.Y, b.Z));// 3

                indexList.Add(0 + (i * 4));
                indexList.Add(1 + (i * 4));
                indexList.Add(2 + (i * 4));
                indexList.Add(3 + (i * 4));

                if(useColor){
                    colorList.Add(colors[c]);
                    colorList.Add(colors[c]);
                    colorList.Add(colors[c]);
                    colorList.Add(colors[c]);
                }

                if(useTexture){
                    textureCoordList.Add(textureCoords[t + 0]);
                    textureCoordList.Add(textureCoords[t + 1]);
                    textureCoordList.Add(textureCoords[t + 2]);
                    textureCoordList.Add(textureCoords[t + 3]);
                }

                i++;
                if (cc) c++;
                if (tt) t += 4;
            }


            //Back
            if (back) {

                normalList.Add(new Vector3(0, 0, -1));

                vertexList.Add(new Vector3(b.X, b.Y, a.Z));// 4
                vertexList.Add(new Vector3(a.X, b.Y, a.Z));// 5
                vertexList.Add(new Vector3(a.X, a.Y, a.Z));// 6
                vertexList.Add(new Vector3(b.X, a.Y, a.Z));// 7

                indexList.Add(1 + (i * 4));
                indexList.Add(0 + (i * 4));
                indexList.Add(3 + (i * 4));
                indexList.Add(2 + (i * 4));

                if (useColor) {
                    colorList.Add(colors[c]);
                    colorList.Add(colors[c]);
                    colorList.Add(colors[c]);
                    colorList.Add(colors[c]);
                }

                if (useTexture) {
                    textureCoordList.Add(textureCoords[t + 0]);
                    textureCoordList.Add(textureCoords[t + 1]);
                    textureCoordList.Add(textureCoords[t + 2]);
                    textureCoordList.Add(textureCoords[t + 3]);
                }

                i++;
                if (cc) c++;
                if (tt) t += 4;
            }


            /* Left & Right (+8)
             * 
             *     0 ----- 4
             *   / |     / |
             * 1 ----- 5   |
             * |   |   |   |
             * |   3 --|-- 7
             * | /     | /
             * 2 ----- 6
             * 
             * 
             */


            //Left
            if (left) {

                normalList.Add(new Vector3(-1, 0, 0));

                vertexList.Add(new Vector3(a.X, b.Y, b.Z));// 1 + 8
                vertexList.Add(new Vector3(a.X, b.Y, a.Z));// 5 + 8
                vertexList.Add(new Vector3(a.X, a.Y, a.Z));// 6 + 8
                vertexList.Add(new Vector3(a.X, a.Y, b.Z));// 2 + 8

                indexList.Add(0 + (i * 4));
                indexList.Add(1 + (i * 4));
                indexList.Add(2 + (i * 4));
                indexList.Add(3 + (i * 4));

                if (useColor) {
                    colorList.Add(colors[c]);
                    colorList.Add(colors[c]);
                    colorList.Add(colors[c]);
                    colorList.Add(colors[c]);
                }

                if (useTexture) {
                    textureCoordList.Add(textureCoords[t + 0]);
                    textureCoordList.Add(textureCoords[t + 1]);
                    textureCoordList.Add(textureCoords[t + 2]);
                    textureCoordList.Add(textureCoords[t + 3]);
                }

                i++;
                if (cc) c++;
                if (tt) t += 4;
            }


            //Right
            if (right) {

                normalList.Add(new Vector3(1, 0, 0));

                vertexList.Add(new Vector3(b.X, b.Y, b.Z));// 0 + 8
                vertexList.Add(new Vector3(b.X, b.Y, a.Z));// 4 + 8
                vertexList.Add(new Vector3(b.X, a.Y, a.Z));// 7 + 8
                vertexList.Add(new Vector3(b.X, a.Y, b.Z));// 3 + 8

                indexList.Add(1 + (i * 4));
                indexList.Add(0 + (i * 4));
                indexList.Add(3 + (i * 4));
                indexList.Add(2 + (i * 4));

                if (useColor) {
                    colorList.Add(colors[c]);
                    colorList.Add(colors[c]);
                    colorList.Add(colors[c]);
                    colorList.Add(colors[c]);
                }

                if (useTexture) {
                    textureCoordList.Add(textureCoords[t + 0]);
                    textureCoordList.Add(textureCoords[t + 1]);
                    textureCoordList.Add(textureCoords[t + 2]);
                    textureCoordList.Add(textureCoords[t + 3]);
                }

                i++;
                if (cc) c++;
                if (tt) t += 4;
            }

            /* Top & Bottom (+16)
             * 
             *     0 ----- 1
             *   / |     / |
             * 3 ----- 2   |
             * |   |   |   |
             * |   4 --|-- 5
             * | /     | /
             * 7 ----- 6
             * 
             * 
             */


            //Top
            if (top) {

                normalList.Add(new Vector3(0, -1, 0));

                vertexList.Add(new Vector3(b.X, a.Y, b.Z));// 3 + 16
                vertexList.Add(new Vector3(a.X, a.Y, b.Z));// 2 + 16
                vertexList.Add(new Vector3(a.X, a.Y, a.Z));// 6 + 16
                vertexList.Add(new Vector3(b.X, a.Y, a.Z));// 7 + 16

                indexList.Add(0 + (i * 4));
                indexList.Add(1 + (i * 4));
                indexList.Add(2 + (i * 4));
                indexList.Add(3 + (i * 4));

                if (useColor) {
                    colorList.Add(colors[c]);
                    colorList.Add(colors[c]);
                    colorList.Add(colors[c]);
                    colorList.Add(colors[c]);
                }

                if (useTexture) {
                    textureCoordList.Add(textureCoords[t + 0]);
                    textureCoordList.Add(textureCoords[t + 1]);
                    textureCoordList.Add(textureCoords[t + 2]);
                    textureCoordList.Add(textureCoords[t + 3]);
                }

                i++;
                if (cc) c++;
                if (tt) t += 4;
            }


            //Bottom
            if (bottom) {

                normalList.Add(new Vector3(0, 1, 0));

                vertexList.Add(new Vector3(b.X, b.Y, b.Z));// 0 + 16
                vertexList.Add(new Vector3(a.X, b.Y, b.Z));// 1 + 16
                vertexList.Add(new Vector3(a.X, b.Y, a.Z));// 5 + 16
                vertexList.Add(new Vector3(b.X, b.Y, a.Z));// 4 + 16

                indexList.Add(1 + (i * 4));
                indexList.Add(0 + (i * 4));
                indexList.Add(3 + (i * 4));
                indexList.Add(2 + (i * 4));

                if (useColor) {
                    colorList.Add(colors[c]);
                    colorList.Add(colors[c]);
                    colorList.Add(colors[c]);
                    colorList.Add(colors[c]);
                }

                if (useTexture) {
                    textureCoordList.Add(textureCoords[t + 0]);
                    textureCoordList.Add(textureCoords[t + 1]);
                    textureCoordList.Add(textureCoords[t + 2]);
                    textureCoordList.Add(textureCoords[t + 3]);
                }

                i++;
                if (cc) c++;
                if (tt) t += 4;
            }
        }

        internal void end() {

            if (indexList.Count==0) {
                return;
            }

            /*bufferIdVertices = GL.GenBuffer();
            if (useColor) bufferIdColors = GL.GenBuffer();
            bufferIdIndices = GL.GenBuffer();
            if (useTexture) bufferIdTextureCoords = GL.GenBuffer();*/

            vertices = vertexList.ToArray<Vector3>();
            Console.WriteLine(vertices.Length + " vertices");

            normals = normalList.ToArray<Vector3>();
            Console.WriteLine(normals.Length + " normals");

            /*GL.BindBuffer(BufferTarget.ArrayBuffer, bufferIdVertices);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * 3 * sizeof(float)), vertices, BufferUsageHint.StaticDraw);
            // Theory: Uses *3 because it is using 3d Vertices.
            */
            if (useColor) {
                colors = colorList.ToArray<Vector4>();
                Console.WriteLine(colors.Length + " colors");
            }
            /*
                GL.BindBuffer(BufferTarget.ArrayBuffer, bufferIdColors);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(colors.Length * 4 * sizeof(float)), colors, BufferUsageHint.StaticDraw);
                // Theory: Uses *4 because it is using 4d Vertices.
            }
            */
            if(useTexture){
                textureCoords = textureCoordList.ToArray<Vector2>();
                Console.WriteLine(textureCoords.Length+" texture coords");
            }
            /*
                GL.BindBuffer(BufferTarget.ArrayBuffer, bufferIdTextureCoords);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(textureCoords.Length * 2 * sizeof(float)), textureCoords, BufferUsageHint.StaticDraw);
                // Theory: Uses *2 because it is using 2d Vertices.
            }*/

            indices = indexList.ToArray<uint>();
            Console.WriteLine(indices.Length+" indices");

            /*GL.BindBuffer(BufferTarget.ElementArrayBuffer, bufferIdIndices);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * 1 * sizeof(uint)), indices, BufferUsageHint.StaticDraw);*/

            indicesCount = indices.Length;
            verticesCount = vertices.Length;
            
            /*vertices = null;
            colors = null;
            indices = null;
            textureCoords = null;

            vertexList.Clear();
            if (useColor) colorList.Clear();
            indexList.Clear();
            if (useTexture) textureCoordList.Clear();*/


            uint pIndex = 0;
            uint cIndex = 1;
            uint tIndex = 2;
            uint nIndex = 3;

            GL.BindAttribLocation(pId, pIndex, "in_Position");
            GL.BindAttribLocation(pId, cIndex, "in_Color");
            GL.BindAttribLocation(pId, tIndex, "in_TextureCoord");
            GL.BindAttribLocation(pId, nIndex, "in_Normal");

            GL.UseProgram(pId);

            vboVertices = CreateVBO<Vector3>(vertices, Vector3.SizeInBytes);
            if (useColor) { vboColors = CreateVBO<Vector4>(colors, Vector4.SizeInBytes); }
            if (useTexture) { vboTextureCoords = CreateVBO<Vector2>(textureCoords, Vector2.SizeInBytes); }
            vboNormals = CreateVBO<Vector3>(normals, Vector3.SizeInBytes);
            ibo = CreateIBO(indices);

            GL.GenVertexArrays(1, out vao);
            GL.BindVertexArray(vao);

            GL.EnableVertexAttribArray(pIndex);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboVertices);
            GL.VertexAttribPointer(pIndex, 3, VertexAttribPointerType.Float, false, Vector3.SizeInBytes, IntPtr.Zero);

            GL.EnableVertexAttribArray(nIndex);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboNormals);
            GL.VertexAttribPointer(nIndex, 3, VertexAttribPointerType.Float, false, Vector3.SizeInBytes, IntPtr.Zero);

            if (useColor) {
                GL.EnableVertexAttribArray(cIndex);
                GL.BindBuffer(BufferTarget.ArrayBuffer, vboColors);
                GL.VertexAttribPointer(cIndex, 4, VertexAttribPointerType.Float, false, Vector4.SizeInBytes, IntPtr.Zero);
            }

            if (useTexture) {
                GL.EnableVertexAttribArray(tIndex);
                GL.BindBuffer(BufferTarget.ArrayBuffer, vboTextureCoords);
                GL.VertexAttribPointer(tIndex, 2, VertexAttribPointerType.Float, false, Vector2.SizeInBytes, IntPtr.Zero);
            }

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            renderEnabled = true;

        }

        private uint CreateVBO<T>(T[] data, int size) where T : struct{
            uint vbo;
            GL.GenBuffers(1, out vbo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData<T>(BufferTarget.ArrayBuffer, (IntPtr)(data.Length * size), data, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            return vbo;
        }

        private uint CreateIBO(uint[] data) {
            uint ibo;
            GL.GenBuffers(1, out ibo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(data.Length * sizeof(uint)), data, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            return ibo;
        }

        internal void render(Game game) {

            if (renderEnabled) {

                //Console.WriteLine("");
                //Console.WriteLine(GL.GetError());

                GL.UseProgram(pId);
                //Console.WriteLine(GL.GetError());


                GL.BindVertexArray(vao);

                GL.EnableVertexAttribArray(0);
                GL.EnableVertexAttribArray(1);
                GL.EnableVertexAttribArray(2);
                GL.EnableVertexAttribArray(3);

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);

                GL.DrawElements(beginMode, indicesCount, DrawElementsType.UnsignedInt, IntPtr.Zero);

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

                GL.DisableVertexAttribArray(0);
                GL.DisableVertexAttribArray(1);
                GL.DisableVertexAttribArray(2);
                GL.DisableVertexAttribArray(3);

                GL.BindVertexArray(0);


                //Console.WriteLine(GL.GetError());

                //GL.BindBuffer(BufferTarget.ArrayBuffer, bufferIdVertices);
                //GL.EnableClientState(ArrayCap.VertexArray);
                //GL.VertexPointer(3, VertexPointerType.Float, 0, IntPtr.Zero);
                

                /*if (useColor) {
                    GL.BindBuffer(BufferTarget.ArrayBuffer, bufferIdColors);
                    GL.EnableClientState(ArrayCap.ColorArray);
                    GL.ColorPointer(4, ColorPointerType.Float, 0, IntPtr.Zero);
                }

                if (useTexture) {
                    GL.BindBuffer(BufferTarget.ArrayBuffer, bufferIdTextureCoords);
                    GL.EnableClientState(ArrayCap.TextureCoordArray);
                    GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, IntPtr.Zero);
                }*/

                //GL.BindBuffer(BufferTarget.ArrayBuffer, bufferIdIndices);
                //GL.DrawElements(beginMode, indicesCount, DrawElementsType.UnsignedInt, IntPtr.Zero);

                /*GL.DisableClientState(ArrayCap.VertexArray);
                GL.DisableClientState(ArrayCap.ColorArray);
                GL.DisableClientState(ArrayCap.TextureCoordArray);*/



            }


        }
    }
}
