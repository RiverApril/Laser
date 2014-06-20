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
    public class Player {
        private Game game;

        public Camera camera;

        public Vector3 position = new Vector3(0, 0, 0);
        public Vector2 rotation = new Vector2(0, 0);
        public Vector3 radii = new Vector3(.4f, 0.8f, .4f);

        public Vector3i scanRadii = new Vector3i(1, 2, 1);
        public Vector3 eyeOffset = new Vector3(0, -.4f, 0);

        private float rotationSpeed = MathHelper.DegreesToRadians(1);
        private float moveSpeed = .1f;

        private float radians90 = MathHelper.DegreesToRadians(90);
        private float radians180 = MathHelper.DegreesToRadians(180);
        private float radians270 = MathHelper.DegreesToRadians(270);

        public Player(Game game) {
            this.game = game;
            camera = new Camera();
        }

        public void update(Game game) {

            KeyboardState k = Keyboard.GetState();
            MouseState m = Mouse.GetCursorState();

            if (m.IsButtonDown(MouseButton.Left)) {
                game.world.setBlock(new Vector3i(position), Block.stone, true);
            }

            if (m.IsButtonDown(MouseButton.Right)) {
                game.world.setBlock(new Vector3i(position), Block.air, true);
            }

            Vector3 moveDif = new Vector3();

            float sin = (float)Math.Sin(radians180 - rotation.Y);
            float cos = (float)Math.Cos(radians180 - rotation.Y);

            if (k.IsKeyDown(Key.Space)) {
                moveDif.Y -= moveSpeed;
            }
            if (k.IsKeyDown(Key.LShift)) {
                moveDif.Y += moveSpeed;
            }


            if (k.IsKeyDown(Key.W)) {
                moveDif.X += sin * -moveSpeed;
                moveDif.Z += cos * -moveSpeed;
            }
            if (k.IsKeyDown(Key.S)) {
                moveDif.X += sin * moveSpeed;
                moveDif.Z += cos * moveSpeed;
            }

            sin = (float)Math.Sin(radians270 - rotation.Y);
            cos = (float)Math.Cos(radians270 - rotation.Y);

            if (k.IsKeyDown(Key.A)) {
                moveDif.X += sin * -moveSpeed;
                moveDif.Z += cos * -moveSpeed;
            }
            if (k.IsKeyDown(Key.D)) {
                moveDif.X += sin * moveSpeed;
                moveDif.Z += cos * moveSpeed;
            }

            if(!k.IsKeyDown(Key.Enter)){
                position += moveDif;
            }else{
                position += attemtToMove(moveDif);
            }

            if (game.Focused) {

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

            camera.position = position+eyeOffset;
            camera.rotation = rotation;

        }

        private Vector3 attemtToMove(Vector3 amount) {

            bool xc = false;
            bool yc = false;
            bool zc = false;

            Vector3 pax = new Vector3(position.X + amount.X, position.Y, position.Z);
            Vector3 pay = new Vector3(position.X, position.Y + amount.Y, position.Z);
            Vector3 paz = new Vector3(position.X, position.Y, position.Z + amount.Z);

            for (int i = -scanRadii.x; i <= scanRadii.x; i++) {
                for (int j = -scanRadii.y; j <= scanRadii.y; j++) {
                    for (int k = -scanRadii.z; k <= scanRadii.z; k++) {
                        if (!xc) { xc = collisionWithBlock((int)(position.X + amount.X + i), (int)(position.Y + j), (int)(position.Z + k), pax); }
                        if (!yc) { yc = collisionWithBlock((int)(position.X + i), (int)(position.Y + amount.Y + j), (int)(position.Z + k), pay); }
                        if (!zc) { zc = collisionWithBlock((int)(position.X + i), (int)(position.Y + j), (int)(position.Z + amount.Z + k), paz); }
                    }
                }
            }
            amount.X = xc ? 0 : amount.X;
            amount.Y = yc ? 0 : amount.Y;
            amount.Z = zc ? 0 : amount.Z;
            return amount;
        }

        private bool collisionWithBlock(int x, int y, int z, Vector3 pos) {
            Block block = game.world.getBlock(x, y, z, false);
            if(block.flags.HasFlag(Block.BlockFlags.nonsolid)){
                return false;
            }
            if (block.collides(game.world, pos, radii, x, y, z)) {
                return true;
            }
            return false;
        }
    }
}
