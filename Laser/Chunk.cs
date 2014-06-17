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
    public class Chunk {

        private ChunkPosition chunkPosition;

        public const int chunkXSize = 16;
        public const int chunkYSize = 16;
        public const int chunkZSize = 16;

        private RenderGroup vbo;

        private int[, ,] blockArray = new int[chunkXSize, chunkYSize, chunkZSize];

        public Chunk(ChunkPosition pos, World world, Game game) {
            this.chunkPosition = new ChunkPosition(pos.x, pos.y, pos.z);
            vbo = new RenderGroup(game);

        }

        public void resetVbo(World world) {
            vbo.begin(BeginMode.Quads, true, true);
            int x, y, z;
            for (int i = 0; i < chunkXSize; i++) {
                for (int j = 0; j < chunkYSize; j++) {
                    for (int k = 0; k < chunkZSize; k++) {
                        x = i + (chunkPosition.x * chunkXSize);
                        y = j + (chunkPosition.y * chunkYSize);
                        z = k + (chunkPosition.z * chunkZSize);
                        int b = blockArray[i, j, k];
                        addCubeToVbo(Block.getBlock(b), x, y, z, world);
                    }
                }
            }
            vbo.end();
        }

        private void addCubeToVbo(Block block, int x, int y, int z, World world) {
            block.addToVbo(world, this, vbo, x, y, z);
        }

        public void render(Game game, World world) {
            vbo.render(game);
        }

        public void setBlock(int x, int y, int z, Block block) {
            blockArray[x, y, z] = block.id;
        }

        internal Block getBlock(int x, int y, int z) {
            //Console.WriteLine(x + ", " + y + ", " + z);
            //return Block.air;
            return Block.getBlock(blockArray[x, y, z]);
        }
    }
}
