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

        private string vertexShaderSource = @"

precision highp float;

attribute vec3 in_Position;
attribute vec4 in_Color;
attribute vec2 in_TextureCoord;
attribute vec3 in_Normal;

uniform mat4 modelMatrix;
uniform mat4 projMatrix;

varying vec4 pass_Color;
varying vec2 pass_TextureCoord;
varying vec2 pass_Normal;

void main(void) {
    in_Position.y=-in_Position.y;
    gl_Position = projMatrix * modelMatrix * vec4(in_Position, 1.0);
	
	pass_Color = in_Color;
	pass_TextureCoord = in_TextureCoord;
	pass_Normal = in_Normal;
}
";

        private string fragmentShaderSource = @"

precision highp float;

uniform sampler2D texture_diffuse;

varying vec4 pass_Color;
varying vec2 pass_TextureCoord;
varying vec2 pass_Normal;

void main(void) {
    gl_FragColor = (texture2D(texture_diffuse, pass_TextureCoord) * pass_Color);
}
";

        public Random rand = new Random();

        public int textureMapID;

        public World world;
        public Player player;

        public int vertexShader;
        public int fragmentShader;
        public int glProgram;

        private Matrix4 projectionView;
        private Matrix4 modelView;

        public int renderDistance = 3;

        public Game() : base(640, 480, GraphicsMode.Default, ""){
            Run(120, 60);
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
            GL.UseProgram(glProgram);

            Console.WriteLine(GL.GetError());

            Console.WriteLine(GL.GetError());


            GL.ClearColor(Color4.DarkGray);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Front);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);

            world = new World(this);
            player = new Player(this);

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

            projectionView = Matrix4.CreatePerspectiveFieldOfView((float)MathHelper.DegreesToRadians(90), Width / (float)Height, 0.1f, 1000.0f);
            //GL.MatrixMode(MatrixMode.Projection);
            //GL.LoadMatrix(ref projectionView);

        }

        protected override void OnUpdateFrame(FrameEventArgs e) {
            Profiler.updateFps.update(e.Time);
            Title = "Laser  UPS: " + Profiler.updateFps.fps + "  FPS: " + Profiler.renderFps.fps + "  Mem: " + (System.GC.GetTotalMemory(false) / 1048576)+" MB";
            base.OnUpdateFrame(e);

            if(Keyboard[Key.Escape]){
                world.save();
                Exit();
            }

            world.update(this);

            player.update(this);
        }

        protected override void OnRenderFrame(FrameEventArgs e) {
            Profiler.renderFps.update(e.Time);
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.LoadIdentity();

            modelView = player.camera.getViewMatrix();
            //GL.MatrixMode(MatrixMode.Modelview);
            //GL.LoadMatrix(ref modelView);

            float[] mf = Matrix4ToArray(modelView);
            float[] pf = Matrix4ToArray(projectionView);


            int mm = GL.GetUniformLocation(glProgram, "modelMatrix");
            int pm = GL.GetUniformLocation(glProgram, "projMatrix");

            GL.ProgramUniformMatrix4(glProgram, mm, 1, false, mf);
            GL.ProgramUniformMatrix4(glProgram, pm, 1, false, pf);

            world.render(this);

            SwapBuffers();                                                                                                                                                                
        }

        private float[] Matrix4ToArray(Matrix4 m) {
            return new float[]{
                m.Row0.X,
                m.Row0.Y,
                m.Row0.Z,
                m.Row0.W,
                m.Row1.X,
                m.Row1.Y,
                m.Row1.Z,
                m.Row1.W,
                m.Row2.X,
                m.Row2.Y,
                m.Row2.Z,
                m.Row2.W,
                m.Row3.X,
                m.Row3.Y,
                m.Row3.Z,
                m.Row3.W
            };
        }
        
        [STAThread]
        static void Main(string[] args) {
            Game game = new Game();
        }
    }



}
