using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RacerCS
{
    public class Game
    {
        private RenderInformation _renderInfo;
        private GameModeBase _gameMode;

        public Game()
        {
            _renderInfo = new RenderInformation
            {
                Width = 320,
                Height = 240,
                DepthOfField = 150,
                CameraDistance = 30,
                CameraHeight = 100,
            };
        }

        public void SetGameMode(GameMode mode)
        {
            if (mode == GameMode.Splash)
            {
                _gameMode = new GameModeSplash(this);
            }
            else if (mode == GameMode.Race)
            {
                _gameMode = new GameModeRacer(this);
            }
            else if (mode == GameMode.Complete)
            {
                _gameMode = new GameModeComplete(this);
            }
        }

        public void Render(Graphics g)
        {
            _gameMode.Render(g);
        }

        public void KeyDown(KeyEventArgs e)
        {
            _gameMode.KeyDown(e);
        }

        public void KeyUp(KeyEventArgs e)
        {
            _gameMode.KeyUp(e);
        }

        public int Width => _renderInfo.Width;

        public int Height => _renderInfo.Height;

        public int DepthOfField => _renderInfo.DepthOfField;

        public int CameraHeight => _renderInfo.CameraHeight;

        public int CameraDistance => _renderInfo.CameraDistance;
    }
}
