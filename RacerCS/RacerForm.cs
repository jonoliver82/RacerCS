using System;
using System.Drawing;
using System.Windows.Forms;

namespace RacerCS
{
    public partial class RacerForm : Form
    {
        private Game _game;
        private float _scaleX = 1;
        private float _scaleY = 1;

        public RacerForm()
        {
            InitializeComponent();
            _game = new Game();
            ClientSize = new Size(_game.Width, _game.Height);
        }

        private void RacerForm_Load(object sender, EventArgs e)
        {
            _game.SetGameMode(GameMode.Splash);
            DoubleBuffered = true;
            Application.Idle += Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {            
            e.Graphics.ScaleTransform(_scaleX, _scaleY);
            _game.Render(e.Graphics);
        }

        private void RacerForm_KeyDown(object sender, KeyEventArgs e)
        {
            _game.KeyDown(e);
        }

        private void RacerForm_KeyUp(object sender, KeyEventArgs e)
        {
            _game.KeyUp(e);
        }

        private void RacerForm_Resize(object sender, EventArgs e)
        {
            _scaleX = (float)ClientSize.Width / (float)_game.Width;
            _scaleY = (float)ClientSize.Height / (float)_game.Height;
        }
    }
}
