using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser {
    public class Profiler {

        public static FpsProfiler updateFps = new FpsProfiler();
        public static FpsProfiler renderFps = new FpsProfiler();

    }

    public class FpsProfiler {

        private double totalTime = 0;
        private double frames = 0;
        public int fps { get; private set; }

        public void update(double time) {
            totalTime += time;
            if (totalTime < 1.0) {
                frames++;
            } else {
                fps = (int)frames;
                totalTime = 0;
                frames = 0;
            }
        }
    }
}
