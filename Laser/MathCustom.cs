using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser {
    public class MathCustom {

        public static int negModFix(int x, int m) {
            return (x % m + m) % m;
        }

        public static Vector3i chunkFromBlock(Vector3i blockPosition, Vector3i chunkSize) {
            return new Vector3i(chunkFromBlock(blockPosition.x, chunkSize.x), chunkFromBlock(blockPosition.y, chunkSize.y), chunkFromBlock(blockPosition.z, chunkSize.z));
        }

        public static int chunkFromBlock(int blockPosition, int chunkSize) {
            int c = (blockPosition / chunkSize);
            if (negModFix(blockPosition, chunkSize) != 0 && blockPosition<0) {
                c--;
            }
            return c;
        }

        internal static Vector3i chunkFromBlock(int x, int y, int z, Vector3i chunkSize) {
            return new Vector3i(chunkFromBlock(x, chunkSize.x), chunkFromBlock(y, chunkSize.y), chunkFromBlock(z, chunkSize.z));
        }
    }
}
