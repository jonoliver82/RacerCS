using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RacerCS
{
    public class Player
    {
        public double Position { get; set; }

        public double Speed { get; set; }

        public double Acceleration { get; set; }

        public double Deceleration { get; set; }

        public double Breaking { get; set; }

        public double Turning { get; set; }

        public double PositionX { get; set; }

        public double MaxSpeed { get; set; }

        public CarSprite Sprite { get; set; }
    }
}
