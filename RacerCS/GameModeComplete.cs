using System.Drawing;
using System.Windows.Forms;

namespace RacerCS
{
    public class GameModeComplete : GameModeBase
    {
        public GameModeComplete(Game game) : base(game, "spritesheet.high.png")
        {
        }

        public override void Render(Graphics g)
        {
            ClearScreen(g, "#dc9");
            DrawBackground(g, 0);
            DrawString(g, "You did it!", 120, 130);
            DrawString(g, "Press Space to race again", 60, 150);

            if (_keys[Keys.Space])
            {
                Game.SetGameMode(GameMode.Race);
            }
        }
    }
}
