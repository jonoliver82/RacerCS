using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RacerCS
{
    public class SpriteBufferEntry
    {
        public int Y { get; set; }

        public int X { get; set; }

        public int YMax { get; set; }

        public double Scale { get; set; }

        public Rectangle SourceLocation { get; set; }
    }
}
