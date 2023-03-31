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
            // also redraw some GUI stuff
            label1.Refresh();

            if (this.board.whites_turn == false)
            {
                Bot.depth = 0;
                this.board.play_move(Bot.get_best_move(this.board));
                this.ChessBoardPanel.Refresh();
                // also redraw some GUI stuff
                label1.Refresh();
            }

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Paint(object sender, PaintEventArgs e)
        {
            if (board.gamestate == ChessBoard.GameState.ONGOING)
            {
                if (board.whites_turn == true)
                {
                    label1.Text = "White to Play";
                    label1.BackColor = Color.FromArgb(255, 255, 255, 255);
                }
                else
                {
                    label1.Text = "Black to Play";
                    label1.BackColor = Color.FromArgb(255, 155, 155, 155);
                }
            }
            else if (board.gamestate == ChessBoard.GameState.WHITE_CHECKMATE)
            {
                label1.Text = "Checkmate!";
                label1.BackColor = Color.FromArgb(255, 255, 155, 155);
            }
            else if (board.gamestate == ChessBoard.GameState.BLACK_CHECKMATE)
            {
                label1.Text = "Checkmate!";
                label1.BackColor = Color.FromArgb(255, 255, 155, 155);
            }
            else if (board.gamestate == ChessBoard.GameState.STALEMATE)
            {
                label1.Text = "Stalemate";
                label1.BackColor = Color.FromArgb(255, 155, 155, 155);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void None_Click(object sender, EventArgs e)
        {

        }
    }
}
