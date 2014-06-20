using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.IO;

namespace Laser {
    public class World {

        public Game game;

        private Dictionary<Vector3i, Chunk> loadedChunks;
        private List<Vector3i> chunksToLoad;
        private List<Vector3i> chunksToUnload;

        public string directory = "testWorld";

        public World(Game game) {

            Directory.CreateDirectory(directory);

            this.game = game;

            chunksToLoad = new List<Vector3i>();
            chunksToUnload = new List<Vector3i>();
            loadedChunks = new Dictionary<Vector3i, Chunk>();



        }

        public void render(Game game) {

            GL.BindTexture(TextureTarget.Texture2D, game.textureMapID);

            foreach(Chunk chunk in loadedChunks.Values){
                chunk.render(game, this);
            }

        }

        public void update(Game game) {

            int x = (int)game.player.position.X/Chunk.chunkSize.x;
            int y = (int)game.player.position.Y/Chunk.chunkSize.y;
            int z = (int)game.player.position.Z/Chunk.chunkSize.z;

            addToLoadedIfNeeded(new Vector3i(x, y, z));

            for (int i = -game.renderDistance; i <= game.renderDistance; i++) {
                for (int j = -game.renderDistance; j <= game.renderDistance; j++) {
                    for (int k = -game.renderDistance; k <= game.renderDistance; k++) {
                        addToLoadedIfNeeded(new Vector3i(i + x, j + y, k + z));
                    }
                }
            }

            if (chunksToLoad.Count > 0) {
                loadChunk(chunksToLoad[0]);
                chunksToLoad.RemoveAt(0);
            }

            if (chunksToUnload.Count > 0) {
                unloadChunk(chunksToUnload[0]);
                chunksToUnload.RemoveAt(0);
            }

            bool a = true;

            foreach (Chunk chunk in loadedChunks.Values) {
                chunk.update(game, this);
                if(a){
                    if(outOfView(chunk.chunkPosition, x, y, z, game)){
                        chunksToUnload.Add(chunk.chunkPosition);
                        a = false;
                    }
                }
            }

        }

        private bool outOfView(Vector3i p, int x, int y, int z, Game game) {
            return
                (p.x < x - game.renderDistance) ||
                (p.y < y - game.renderDistance) ||
                (p.z < z - game.renderDistance) ||
                (p.x > x + game.renderDistance) ||
                (p.y > y + game.renderDistance) ||
                (p.z > z + game.renderDistance);
        }

        private void addToLoadedIfNeeded(Vector3i p) {
            if(!loadedChunks.ContainsKey(p)){
                if(!chunksToLoad.Contains(p)){
                    chunksToLoad.Add(p);
                }
            }
        }

        public bool setBlock(int x, int y, int z, Block block, bool createChunkIfNeeded = false) {
            return setBlock(new Vector3i(x, y, z), block, createChunkIfNeeded);
        }

        public bool setBlock(Vector3i position, Block block, bool createChunkIfNeeded = false) {
            Vector3i p = MathCustom.chunkFromBlock(position, Chunk.chunkSize);
            if (createChunkIfNeeded) {
                loadChunk(p, false);
            } else {
                return false;
            }
            loadedChunks[p].setBlock(MathCustom.negModFix(position.x, Chunk.chunkSize.x), MathCustom.negModFix(position.y, Chunk.chunkSize.y), MathCustom.negModFix(position.z, Chunk.chunkSize.z), block);
            return true;
        }

        public Block getBlock(int x, int y, int z, bool createChunkIfNeeded = false) {
            return getBlock(new Vector3i(x, y, z), createChunkIfNeeded);
        }

        public Block getBlock(Vector3i position, bool createChunkIfNeeded = false) {
            Vector3i p = MathCustom.chunkFromBlock(position, Chunk.chunkSize);
            if (createChunkIfNeeded) {
                loadChunk(p, false);
            } else if(!loadedChunks.ContainsKey(p)) {
                return Block.air;
            }
            return loadedChunks[p].getBlock(MathCustom.negModFix(position.x, Chunk.chunkSize.x), MathCustom.negModFix(position.y, Chunk.chunkSize.y), MathCustom.negModFix(position.z, Chunk.chunkSize.z));
        }

        private void loadChunk(Vector3i p, bool forceLoad = false) {
            if(!loadedChunks.ContainsKey(p) || forceLoad){
                loadedChunks.Add(p, new Chunk(p, this, game));
            }
        }

        private void unloadChunk(Vector3i p) {
            if (loadedChunks.ContainsKey(p)) {
                loadedChunks[p].save();
                loadedChunks.Remove(p);
            }
        }

        internal void save() {
            foreach (Chunk chunk in loadedChunks.Values) {
                chunk.save();
            }
        }

        internal void resetVboForChunk(int x, int y, int z) {
            Vector3i p = MathCustom.chunkFromBlock(x, y, z, Chunk.chunkSize);
            if (!loadedChunks.ContainsKey(p)) {
                loadedChunks[p].needsVboReset = true;
            }
        }
    }
}
