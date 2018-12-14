using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RacerCS
{
    public class GameModeSplash : GameModeBase
    {
        private Rectangle _logoSource = new Rectangle(357, 9, 115, 20);

        public GameModeSplash() : base("spritesheet.high.png")
        {
        }

        public override void Render(Game game, Graphics g)
        {
            ClearScreen(g, "#000");
            DrawImage(g, _logoSource, 100, 20, 1);

            DrawString(g, "Instructions:", 100, 90);
            DrawString(g, "Space to start, arrows to drive:", 30, 100);
            DrawString(g, "Credits:", 120, 120);
            DrawString(g, "Code, Art: Selim Arsever", 55, 130);
            DrawString(g, "Font: spicypixel.net", 70, 140);
            DrawString(g, "C# Conversion: jonoliver82", 70, 150);

            if(_keys[Keys.Space])
            {                
                game.SetGameMode(GameMode.Race);
            }
        }
    }
}
