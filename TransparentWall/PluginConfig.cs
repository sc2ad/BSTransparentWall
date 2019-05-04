using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransparentWall
{
    internal class PluginConfig
    {
        public bool RegenerateConfig = true;
        public bool TransparentWallEnabled = true;
        public bool HMD = true;
        public bool CameraPlus = true;
        public bool LIV = true;
        public string ExcludedCams = "";
        public int WallLayer = 25;
    }
}
