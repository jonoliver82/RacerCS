using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RacerCS
{
    public abstract class GameModeBase
    {
        protected Dictionary<Keys, bool> _keys;
        private Image _spriteSheet;
        private Rectangle _backgroundLocation;

        protected GameModeBase(string spriteSheetFile)
        {
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

        public abstract void Render(Game game, Graphics g);

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

        protected void DrawImage(Graphics g, Rectangle sourceImage, double destinationX, double destinationY, int scale)
        {
            g.DrawImage(_spriteSheet, (float)destinationX, (float)destinationY, sourceImage, GraphicsUnit.Pixel);
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
            var first = playerPosition / 2 % (_backgroundLocation.Width);
            DrawImage(g, _backgroundLocation, first - _backgroundLocation.Width + 1, 0, 1);
            DrawImage(g, _backgroundLocation, first + _backgroundLocation.Width - 1, 0, 1);
            DrawImage(g, _backgroundLocation, first, 0, 1);
        }
    }
}
