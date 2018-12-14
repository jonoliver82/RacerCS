using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RacerCS
{
    public partial class RacerForm : Form
    {
        private Game _game;

        public RacerForm()
        {
            InitializeComponent();
            _game = new Game();

            ClientSize = new Size(_game.Width, _game.Height);
        }

        private void RacerForm_Load(object sender, EventArgs e)
        {
            DoubleBuffered = true;
            Application.Idle += Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
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
    }
}
