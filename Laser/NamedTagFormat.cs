using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Laser {

    public class TagBase {

        public static Type[] tagIds = new Type[] { typeof(TagBase), typeof(TagCompound), typeof(Tag3dIntArray) };

        public String name = "NO_NAME";

        public static TagBase load(byte[] bytes) {
            int i = 0;
            return load(bytes, ref i);
        }

        public static TagBase load(byte[] bytes, ref int startIndex) {
            Console.WriteLine();
            TagBase tag = newNTFFromID(bytes[startIndex]);
            startIndex++;
            tag.name = readString(bytes, ref startIndex);
            tag.loadTag(bytes, ref startIndex);
            return tag;
        }

        public virtual void loadTag(byte[] bytes, ref int startIndex) {
            
        }

        public void saveTag(List<byte> bytes) {
            bytes.Add(getId(GetType()));
            writeString(bytes, name);
            save(bytes);
        }

        private byte getId(Type type) {
            for (byte i = 0; i < tagIds.Length; i++) {
                if (tagIds[i] == type){
                    return i;
                }
            }
            return 0;
        }

        protected virtual void save(List<byte> bytes) {

        }

        public static TagBase newNTFFromID(byte p) {
            return (TagBase)Activator.CreateInstance(tagIds[p]);
        }

        private static String readString(byte[] bytes, ref int startIndex) {
            byte size = bytes[startIndex];
            startIndex++;
            String s = "";
            for (byte i = 0; i < size; i++) {
                s += Convert.ToChar(bytes[startIndex]);
                startIndex++;
            }
            return s;
        }

        private void writeString(List<byte> bytes, string s) {
            if(s.Length >= 256){
                s = s.Substring(0, 255);
            }
            bytes.Add((byte)s.Length);
            for (byte i = 0; i < (byte)s.Length;i++ ) {
                bytes.Add(Convert.ToByte(s[i]));
            }
        }
    }

    public class TagCompound : TagBase{

        public Dictionary<String, TagBase> data = new Dictionary<String, TagBase>();

        public TagCompound() {

        }

        public TagCompound(String name) {
            this.name = name;
        }

        public override void loadTag(byte[] bytes, ref int startIndex) {
            byte size = bytes[startIndex];
            startIndex++;
            for (int i = 0; i < size;i++ ) {
                TagBase tag = load(bytes, ref startIndex);
                data.Add(tag.name, tag);
            }
        }

        protected override void save(List<byte> bytes) {
            bytes.Add((byte)data.Count());
            foreach(TagBase tag in data.Values){
                tag.saveTag(bytes);
            }
        }

        public void add(TagBase tag) {
            data.Add(tag.name, tag);
        }
    }

    public class Tag3dIntArray : TagBase {

        public Int32[, ,] data;

        public Tag3dIntArray() {

        }

        public Tag3dIntArray(String name, int[, ,] array) {
            this.name = name;
            data = array;
        }

        public override void loadTag(byte[] bytes, ref int startIndex) {
            byte xSize = bytes[startIndex]; startIndex++;
            byte ySize = bytes[startIndex]; startIndex++;
            byte zSize = bytes[startIndex]; startIndex++;

            data = new Int32[xSize, ySize, zSize];

            for (int i = 0; i < xSize; i++) {
                for (int j = 0; j < ySize; j++) {
                    for (int k = 0; k < zSize; k++) {
                        data[i, j, k] = (bytes[startIndex] << 24) | (bytes[startIndex + 1] << 16) | (bytes[startIndex + 2] << 8) | (bytes[startIndex + 3]);
                        startIndex += 4;
                    }
                }
            }

        }

        protected override void save(List<byte> bytes) {
            bytes.Add((byte)data.GetLength(0));
            bytes.Add((byte)data.GetLength(1));
            bytes.Add((byte)data.GetLength(2));

            for (int i = 0; i < (byte)data.GetLength(0); i++) {
                for (int j = 0; j < (byte)data.GetLength(1); j++) {
                    for (int k = 0; k < (byte)data.GetLength(2); k++) {
                        bytes.Add((byte)(data[i, j, k] >> 24));
                        bytes.Add((byte)(data[i, j, k] >> 16));
                        bytes.Add((byte)(data[i, j, k] >> 8));
                        bytes.Add((byte)(data[i, j, k]));
                    }
                }
            }
        }

    }
}
