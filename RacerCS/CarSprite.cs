using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RacerCS
{
    public class CarSprite
    {
        public CarSprite(Rectangle location, int x, int y)
        {
            Location = location;
            X = x;
            Y = y;
        }

        public Rectangle Location { get; set; }

        public int X { get; set; }

        public int Y { get; set; }
    }
}
