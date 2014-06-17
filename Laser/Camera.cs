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

        private float rotationSpeed = MathHelper.DegreesToRadians(1);
        private float moveSpeed = .1f;

        private float radians90 = MathHelper.DegreesToRadians(90);
        private float radians180 = MathHelper.DegreesToRadians(180);
        private float radians270 = MathHelper.DegreesToRadians(270);

        public void traslateAndRotateMatrix() {
            GL.Rotate(MathHelper.RadiansToDegrees(rotation.X), 1, 0, 0);
            GL.Rotate(MathHelper.RadiansToDegrees(rotation.Y), 0, 1, 0);
            GL.Translate(position);
        }

        public void update(Game game) {

            KeyboardState k = Keyboard.GetState();
            MouseState m = Mouse.GetCursorState();

            Vector2 moveDif = new Vector2();

            float sin = (float)Math.Sin(radians180 - rotation.Y);
            float cos = (float)Math.Cos(radians180 - rotation.Y);

            if (k.IsKeyDown(Key.Space)) {
                position.Y -= moveSpeed;
            }
            if (k.IsKeyDown(Key.LShift)) {
                position.Y += moveSpeed;
            }


            if(k.IsKeyDown(Key.W)){
                moveDif.X += sin * moveSpeed;
                moveDif.Y += cos * moveSpeed;
            }
            if (k.IsKeyDown(Key.S)) {
                moveDif.X += sin * -moveSpeed;
                moveDif.Y += cos * -moveSpeed;
            }

            sin = (float)Math.Sin(radians270 - rotation.Y);
            cos = (float)Math.Cos(radians270 - rotation.Y);

            if (k.IsKeyDown(Key.A)) {
                moveDif.X += sin * moveSpeed;
                moveDif.Y += cos * moveSpeed;
            }
            if (k.IsKeyDown(Key.D)) {
                moveDif.X += sin * -moveSpeed;
                moveDif.Y += cos * -moveSpeed;
            }

            position.X += moveDif.X;
            position.Z += moveDif.Y;

            if(game.Focused){

                int x = ((Mouse.GetCursorState().X - game.Bounds.X - game.Bounds.Width / 2));
                int y = ((Mouse.GetCursorState().Y - game.Bounds.Y - game.Bounds.Height / 2));

                rotation.Y += x * rotationSpeed;
                rotation.X -= y * rotationSpeed;

                if (rotation.X < -radians90) {
                    rotation.X = -radians90;
                }
                if (rotation.X > radians90) {
                    rotation.X = radians90;
                }

                //Console.WriteLine("Mouse: " + x + ", " + y);

                Mouse.SetPosition(game.Bounds.X + game.Bounds.Width / 2, game.Bounds.Y + game.Bounds.Height / 2);
            }
        }
    }
}
