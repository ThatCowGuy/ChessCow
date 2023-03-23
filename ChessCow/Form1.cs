using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.Odbc;



namespace ChessCow
{
    public partial class Form1 : Form
    {
        public Chessboard board = new Chessboard();


        public Form1()
        {
            InitializeComponent();
        }

        private void ChessBoardPanel_Paint(object sender, PaintEventArgs e)
        {
            // Creating a Graphics Object when the "Paint" thing in the Form is called
            Graphics g = e.Graphics;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            this.board.draw(g);
        }
    }
}
