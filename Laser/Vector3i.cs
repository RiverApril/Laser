using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Laser {
    public struct Vector3i {
        public int x;
        public int y;
        public int z;

        public Vector3i(int x, int y, int z) {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3i(OpenTK.Vector3 position) {
            x = (int)Math.Round(position.X);
            y = (int)Math.Round(position.Y);
            z = (int)Math.Round(position.Z);
        }

        public override bool Equals(object obj) {
            if(obj.GetType()!=this.GetType()){
                return false;
            }

            if (x != ((Vector3i)obj).x) {
                return false;
            }
            if (y != ((Vector3i)obj).y) {
                return false;
            }
            if (z != ((Vector3i)obj).z) {
                return false;
            }
            return true;
        }

        public Vector3i plus(int ox, int oy, int oz) {
            return new Vector3i(x+ox, y+oy, z+oz);
        }

        public override int GetHashCode() {
            return x ^ y ^ z;
        }

        public override string ToString() {
            return x + " ," + y + ", " + z;
        }


        internal string getSafeString() {
            return x + "_" + y + "_" + z;
        }
    }
}
