using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Laser {
    public class Block {

        [Flags]
        public enum BlockFlags {
            none = 0,
            nonsolid = 1,
            abnormalRender = 1 << 1,
            invisible = 1 << 2 | abnormalRender
        }

        private static Block[] blockArray = new Block[3];

        public static Block air = new Block(0, "air", BlockFlags.nonsolid | BlockFlags.invisible);
        public static Block stone = new Block(1, "stone", BlockFlags.none);
        public static Block dirt = new Block(2, "dirt", BlockFlags.none);

        private Vector2[] textureCoords;

        public int id { get; private set; }
        public string name { get; private set; }
        public BlockFlags flags { get; private set; }

        public Block(int id, string name, BlockFlags flags) {
            this.id = id;
            this.name = name;
            this.flags = flags;

            blockArray[id] = this;

            textureCoords = makeTextureCoords(id, 0);
        }

        private Vector2[] makeTextureCoords(int x, int y) {
            return new Vector2[] {
                new Vector2((x+0) * RenderGroup.textureSplit, (y+0) * RenderGroup.textureSplit), 
                new Vector2((x+1) * RenderGroup.textureSplit, (y+0) * RenderGroup.textureSplit), 
                new Vector2((x+1) * RenderGroup.textureSplit, (y+1) * RenderGroup.textureSplit), 
                new Vector2((x+0) * RenderGroup.textureSplit, (y+1) * RenderGroup.textureSplit) };
        }

        public static Block getBlock(int b) {
            return blockArray[b];
        }


        public void addToVbo(World world, Chunk chunk, RenderGroup vbo, int x, int y, int z) {
            if (!flags.HasFlag(BlockFlags.invisible)) {
                vbo.addCube(new Vector3(x - .5f, y - .5f, z - .5f), new Vector3(x + .5f, y + .5f, z + .5f), sidesToRender(chunk, world, x, y, z), new Vector4[]{new Vector4(1, 0, 0, 1)}, textureCoords);
            }
        }

        private Sides sidesToRender(Chunk chunk, World world, int x, int y, int z) {
            Sides sides = Sides.none;
            sides |= (world.getBlock(x - 1, y, z).flags.HasFlag(BlockFlags.abnormalRender) ? Sides.xLow : Sides.none);
            sides |= (world.getBlock(x + 1, y, z).flags.HasFlag(BlockFlags.abnormalRender) ? Sides.xHigh : Sides.none);
            sides |= (world.getBlock(x, y - 1, z).flags.HasFlag(BlockFlags.abnormalRender) ? Sides.yLow : Sides.none);
            sides |= (world.getBlock(x, y + 1, z).flags.HasFlag(BlockFlags.abnormalRender) ? Sides.yHigh : Sides.none);
            sides |= (world.getBlock(x, y, z - 1).flags.HasFlag(BlockFlags.abnormalRender) ? Sides.zLow : Sides.none);
            sides |= (world.getBlock(x, y, z + 1).flags.HasFlag(BlockFlags.abnormalRender) ? Sides.zHigh : Sides.none);
            Console.WriteLine("Sides to render: "+sides.ToString());
            return sides;
        }
    }
}
