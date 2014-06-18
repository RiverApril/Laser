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
    public class World {

        private Dictionary<ChunkPosition, Chunk> loadedChunks;

        public World(Game game) {
            loadedChunks = new Dictionary<ChunkPosition, Chunk>();

            ChunkPosition p = new ChunkPosition(0, 0, 0);
            loadedChunks.Add(p, new Chunk(p, this, game));
            loadedChunks[p].setBlock(0, 0, 0, Block.stone);
            loadedChunks[p].setBlock(1, 0, 0, Block.dirt);
            loadedChunks[p].setBlock(2, 0, 0, Block.stone);

            loadedChunks[p].setBlock(5, 5, 5, Block.dirt);

            p = new ChunkPosition(-1, 0, 0);
            loadedChunks.Add(p, new Chunk(p, this, game));
            loadedChunks[p].setBlock(15, 1, 0, Block.stone);
            //loadedChunks[p].setBlock(14, 1, 0, Block.stone);
            loadedChunks[p].setBlock(13, 1, 0, Block.dirt);
            //loadedChunks[p].setBlock(12, 1, 0, Block.stone);
            loadedChunks[p].setBlock(11, 1, 0, Block.stone);
            loadedChunks[p].setBlock(9, 1, 0, Block.stone);
            loadedChunks[p].setBlock(7, 1, 0, Block.dirt);
            loadedChunks[p].setBlock(5, 1, 0, Block.stone);

            p = new ChunkPosition(1, 0, 0);
            loadedChunks.Add(p, new Chunk(p, this, game));
            loadedChunks[p].setBlock(0, 1, 0, Block.stone);
            //loadedChunks[p].setBlock(14, 1, 0, Block.stone);
            loadedChunks[p].setBlock(2, 1, 0, Block.dirt);
            //loadedChunks[p].setBlock(12, 1, 0, Block.stone);
            loadedChunks[p].setBlock(3, 1, 0, Block.stone);


            foreach (Chunk chunk in loadedChunks.Values) {
                chunk.resetVbo(this);
                Console.WriteLine("reset vbo for chunk");
            }
        }

        public void render(Game game) {

            GL.BindTexture(TextureTarget.Texture2D, game.textureMapID);

            foreach(Chunk chunk in loadedChunks.Values){
                chunk.render(game, this);
            }

        }

        public Block getBlock(int x, int y, int z) {
            //Console.WriteLine("-1 % 16 = " + negModFix(-1, 16));
            ChunkPosition p = new ChunkPosition(chunkVertexFromBlockVertex(x, Chunk.chunkXSize), chunkVertexFromBlockVertex(y, Chunk.chunkYSize), chunkVertexFromBlockVertex(z, Chunk.chunkZSize));
            if (loadedChunks.ContainsKey(p)) {
                return loadedChunks[p].getBlock(negModFix(x, Chunk.chunkXSize), negModFix(y, Chunk.chunkYSize), negModFix(z, Chunk.chunkZSize));
            }
            return Block.air;
        }

        private int negModFix(int x, int m) {
            return (x % m + m) % m;
        }

        private int chunkVertexFromBlockVertex(int x, int xx) {
            return (x - negModFix(x, xx)) / xx;
        }
    }
}
