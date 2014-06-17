using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;


namespace Laser {

    public class Game : GameWindow {

        Random rand = new Random();

        //Vbo testVbo;

        public int textureMapID;

        public World world;
        public Camera camera = new Camera();

        public int vertexShader;
        public int fragmentShader;
        public int glProgram;

        private string vertexShaderSource = @"

precision highp float;

attribute vec3 in_Position;
attribute vec4 in_Color;
attribute vec2 in_TextureCoord;
uniform mat4 modelAndProjMatrix;

varying vec4 pass_Color;
varying vec2 pass_TextureCoord;

void main(void) {
    gl_Position = vec4(in_Position, 1.0) * modelAndProjMatrix;
	
	pass_Color = in_Color;
	pass_TextureCoord = in_TextureCoord;
}
";

        private string fragmentShaderSource = @"

precision highp float;

uniform sampler2D texture_diffuse;

varying vec4 pass_Color;
varying vec2 pass_TextureCoord;

void main(void) {
  gl_FragColor = pass_Color;
  gl_FragColor = texture2D(texture_diffuse, pass_TextureCoord);
}
";
        private Matrix4 projection;
        private Matrix4 modelView;
        private Matrix4 modelAndProjectionView;

        public Game() : base(640, 480, GraphicsMode.Default, "L.A.S.E.R."){
            Run(60);
        }

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);

            OpenTK.Input.Mouse.SetPosition(Bounds.X + Bounds.Width / 2, Bounds.Y + Bounds.Height / 2);

            System.IO.Directory.SetCurrentDirectory(System.IO.Directory.GetCurrentDirectory() + "\\..\\..\\");
            Console.WriteLine(System.IO.Directory.GetCurrentDirectory() + "");

            textureMapID = loadTexture("textureMap.png");


            vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexShaderSource);
            GL.CompileShader(vertexShader);

            fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);
            GL.CompileShader(fragmentShader);

            glProgram = GL.CreateProgram();
            GL.AttachShader(glProgram, vertexShader);
            GL.AttachShader(glProgram, fragmentShader);

            GL.LinkProgram(glProgram);
            GL.ValidateProgram(glProgram);

            Console.WriteLine(GL.GetError());

            Console.WriteLine(GL.GetError());


            GL.ClearColor(Color4.DarkGray);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            world = new World(this);

            /*testVbo = new Vbo();
            testVbo.begin(BeginMode.Quads, false, true);

            float size = .1f;

            int t = 100;

            for (float i = -size * t; i < size * t; i += size * 3) {
                for (float j = -size * t; j < size * t; j += size * 3) {
                    for (float k = -size * t; k < size * t; k += size * 3) {
                        testVbo.addCube(new Vector3(-size + i, -size + j, -size + k), new Vector3(size + i, size + j, size + k), Sides.all);
                    }
                }
            }

            testVbo.end();*/

        }

        private int loadTexture(string file) {

            if(String.IsNullOrEmpty(file)){
                throw new ArgumentException(file);
            }
            if (!System.IO.File.Exists(file)) {
                throw new ArgumentException(file);
            }
            int id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);

            Bitmap bmp = new Bitmap(file);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmpData.Width, bmpData.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0);

            return id;
        }

        protected override void OnResize(EventArgs e) {
            base.OnResize(e);

            GL.Viewport(ClientRectangle);

            projection = Matrix4.CreatePerspectiveFieldOfView((float)MathHelper.DegreesToRadians(120), Width / (float)Height, 0.001f, 1000.0f);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);

        }

        protected override void OnUpdateFrame(FrameEventArgs e) {
            base.OnUpdateFrame(e);

            if(Keyboard[Key.Escape]){
                Exit();
            }

            camera.update(this);
        }

        protected override void OnRenderFrame(FrameEventArgs e) {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            modelView = Matrix4.LookAt(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelView);

            camera.traslateAndRotateMatrix();

            modelAndProjectionView = projection * modelView;

            float f = modelAndProjectionView[0, 0];

            int l = GL.GetUniformLocation(glProgram, "modelAndProjMatrix");

            GL.ProgramUniformMatrix4(glProgram, l, 1, true, ref f);

            world.render(this);

            SwapBuffers();

        }
        
        [STAThread]
        static void Main(string[] args) {
            Game game = new Game();
        }
    }



}
