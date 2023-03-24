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



namespace ChessCow2
{
    public partial class Form1 : Form
    {
        public ChessBoard board = new ChessBoard();


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

        private void ChessBoardPanel_MouseClick(object sender, MouseEventArgs e)
        {
            int tile_x = (e.X - ChessBoard.boarder_w) / ChessBoard.tile_w;
            int tile_y = (e.Y - ChessBoard.boarder_w) / ChessBoard.tile_h;
            if (tile_x < 0 || tile_x > 7) return;
            if (tile_y < 0 || tile_y > 7) return;

            // flip y so that 0 is at the bottom
            tile_y = 7 - tile_y;

            this.board.process_click(tile_x, tile_y);
            this.ChessBoardPanel.Refresh();
        }
    }
}
