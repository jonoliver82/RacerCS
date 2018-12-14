using System.Drawing;
using System.Windows.Forms;

namespace RacerCS
{
    public class GameModeSplash : GameModeBase
    {
        private Rectangle _logoSource = new Rectangle(357, 9, 115, 20);

        public GameModeSplash(Game game) : base(game, "spritesheet.high.png")
        {
        }

        public override void Render(Graphics g)
        {
            ClearScreen(g, "#000");

            var dest = new Rectangle(100, 20, _logoSource.Width, _logoSource.Height);
            DrawImage(g, _logoSource, dest, 1);

            DrawString(g, "Instructions:", 100, 90);
            DrawString(g, "Space to start, arrows to drive", 30, 100);
            DrawString(g, "Credits:", 120, 120);
            DrawString(g, "Code, Art: Selim Arsever", 55, 130);
            DrawString(g, "Font: spicypixel.net", 70, 140);
            DrawString(g, "C# Conversion: jonoliver82", 70, 150);

            if(_keys[Keys.Space])
            {                
                Game.SetGameMode(GameMode.Race);
            }
        }
    }
}
