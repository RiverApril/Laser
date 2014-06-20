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
    public class Chunk {

        public Vector3i chunkPosition{get; private set;}

        public static Vector3i chunkSize = new Vector3i(16, 16, 16);

        private VboGroup vbo;

        private int[, ,] blockArray = new int[chunkSize.x, chunkSize.y, chunkSize.z];

        public bool needsVboReset = true;

        private string fileName;

        private World world;

        public Chunk(Vector3i pos, World world, Game game) {
            this.chunkPosition = new Vector3i(pos.x, pos.y, pos.z);
            vbo = new VboGroup(game);

            fileName = world.directory + "\\chunk_" + chunkPosition.getSafeString()+".chunk";

            bool loaded = false;

            if (File.Exists(fileName)) {
                loaded = load();
            }

            if (!loaded) {
                generate(game, world);
                save();
                Console.WriteLine("Generate Chunk");
            } else {
                Console.WriteLine("Loaded Chunk");
            }

            this.world = world;
        }

        private void generate(Game game, World world) {
            setBlock(0, 0, 0, Block.stone);
        }

        public void save() {
            List<byte> bytes = new List<byte>(); ;
            saveToBytes(bytes);
            File.WriteAllBytes(fileName, bytes.ToArray());
        }

        private void saveToBytes(List<byte> bytes) {
            TagCompound c = new TagCompound();
            c.name = "chunk "+chunkPosition.ToString();
            c.add(new Tag3dIntArray("blocks", blockArray));
            c.saveTag(bytes);
        }

        public bool load() {
            byte[] bytes = File.ReadAllBytes(fileName);
            return loadFromBytes(bytes);
        }

        private bool loadFromBytes(byte[] bytes) {
            TagCompound c = (TagCompound)TagBase.load(bytes);
            blockArray = ((Tag3dIntArray)c.data["blocks"]).data;
            return true;
        }

        public void resetVbo(World world) {
            vbo.begin(BeginMode.Quads, true, true);
            int x, y, z;
            for (int i = 0; i < chunkSize.x; i++) {
                for (int j = 0; j < chunkSize.y; j++) {
                    for (int k = 0; k < chunkSize.z; k++) {
                        x = i + (chunkPosition.x * chunkSize.x);
                        y = j + (chunkPosition.y * chunkSize.y);
                        z = k + (chunkPosition.z * chunkSize.z);
                        int b = blockArray[i, j, k];
                        addCubeToVbo(Block.getBlock(b), x, y, z, world);
                    }
                }
            }
            vbo.end();
            Console.WriteLine("Chunk Vbo Reset "+chunkPosition.ToString());
        }

        private void addCubeToVbo(Block block, int x, int y, int z, World world) {
            block.addToVbo(world, this, vbo, x, y, z);
        }

        public void render(Game game, World world) {
            vbo.render(game);
        }

        internal void update(Game game, World world) {
            if (needsVboReset) {
                needsVboReset = false;
                resetVbo(world);
            }
        }

        public void setBlock(int x, int y, int z, Block block) {
            blockArray[x, y, z] = block.id;
            needsVboReset = true;
        }

        internal Block getBlock(int x, int y, int z) {
            return Block.getBlock(blockArray[x, y, z]);
        }
    }
}
