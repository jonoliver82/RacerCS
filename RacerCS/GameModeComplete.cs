using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RacerCS
{
    public class GameModeComplete : GameModeBase
    {
        public GameModeComplete() : base("spritesheet.high.png")
        {
        }

        public override void Render(Game game, Graphics g)
        {
            ClearScreen(g, "#dc9");
            DrawBackground(g, 0);
            DrawString(g, "You did it!", 120, 130);
            DrawString(g, "Press Space to race again", 60, 150);

            if (_keys[Keys.Space])
            {
                game.SetGameMode(GameMode.Race);
            }
        }
    }
}
