using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace RacerCS
{
    public abstract class GameModeBase
    {
        protected Dictionary<Keys, bool> _keys;
        private Image _spriteSheet;
        private Rectangle _backgroundLocation;

        private Game _game;

        protected GameModeBase(Game game, string spriteSheetFile)
        {
            _game = game;
            _keys = new Dictionary<Keys, bool>
            {
                { Keys.W, false },
                { Keys.A, false },
                { Keys.S, false },
                { Keys.D, false },
                { Keys.Up, false },
                { Keys.Down, false },
                { Keys.Left, false },
                { Keys.Right, false },
                { Keys.Space, false },
            };

            LoadSpriteSheet(spriteSheetFile);
            _backgroundLocation = new Rectangle(0, 9, 320, 120);
        }

        public abstract void Render(Graphics g);

        public Game Game => _game;

        public void KeyDown(KeyEventArgs e)
        {
            _keys[e.KeyCode] = true;
        }

        public void KeyUp(KeyEventArgs e)
        {
            _keys[e.KeyCode] = false;
        }

        protected void LoadSpriteSheet(string fileName)
        {
            _spriteSheet = Bitmap.FromFile(fileName);
        }

        protected void ClearScreen(Graphics g, string htmlColor)
        {
            g.Clear(ColorTranslator.FromHtml(htmlColor));
        }

        protected void DrawImage(Graphics g, Rectangle source, Rectangle destination, int scale)
        {
            g.DrawImage(_spriteSheet, destination, source, GraphicsUnit.Pixel);
        }

        protected void DrawString(Graphics g, string value, int x, int y)
        {
            value = value.ToUpper();
            var currentX = x;
            for (int i = 0; i < value.Length; i++)
            {
                var spriteX = (value[i] - 32) * 8;
                var sourceRect = new RectangleF(spriteX, 0, 8, 8);
                g.DrawImage(_spriteSheet, (float)currentX, y, sourceRect, GraphicsUnit.Pixel);
                currentX += 8;
            }
        }

        protected void DrawBackground(Graphics g, double playerPosition)
        {
            var first = (int)(playerPosition / 2 % (_backgroundLocation.Width));

            var dest1 = new Rectangle(first - _backgroundLocation.Width + 1, 0, _backgroundLocation.Width, _backgroundLocation.Height);
            DrawImage(g, _backgroundLocation, dest1, 1);

            var dest2 = new Rectangle(first + _backgroundLocation.Width - 1, 0, _backgroundLocation.Width, _backgroundLocation.Height);
            DrawImage(g, _backgroundLocation, dest2, 1);

            var dest3 = new Rectangle(first, 0, _backgroundLocation.Width, _backgroundLocation.Height);
            DrawImage(g, _backgroundLocation, dest3, 1);
        }
    }
}
