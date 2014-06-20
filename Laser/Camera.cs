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
    public class Camera {

        public Vector3 position = new Vector3(0, 0, 5);
        public Vector2 rotation = new Vector2(0, 0);

        public Matrix4 getViewMatrix() {
            Matrix4 modelView = Matrix4.LookAt(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY);
            modelView *= Matrix4.CreateTranslation(position);
            modelView *= Matrix4.CreateRotationY(rotation.Y);
            modelView *= Matrix4.CreateRotationX(-rotation.X);
            return modelView;
        }
    }
}
