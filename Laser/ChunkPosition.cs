using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Laser {
    public struct ChunkPosition {
        public int x;
        public int y;
        public int z;

        public ChunkPosition(int x, int y, int z) {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public override bool Equals(object obj) {
            if(obj.GetType()!=this.GetType()){
                return false;
            }

            if (x != ((ChunkPosition)obj).x) {
                return false;
            }
            if (y != ((ChunkPosition)obj).y) {
                return false;
            }
            if (z != ((ChunkPosition)obj).z) {
                return false;
            }
            return true;
        }

        public override int GetHashCode() {
            return x ^ y ^ z;
        }

    }
}
