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
            update_textboxes();
        }

        private void ChessBoardPanel_Paint(object sender, PaintEventArgs e)
        {
            // Creating a Graphics Object when the "Paint" thing in the Form is called
            Graphics g = e.Graphics;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            this.board.draw(g);
        }

        public void update_textboxes()
        {
            this.textBox1.Text = string.Format("{0}", this.board.total_piece_value_white());
            this.textBox1.Refresh();

            // I want to display the threat that white exerts
            this.textBox2.Text = string.Format("{0}", this.board.total_threat_level_black());
            this.textBox2.Refresh();

            this.textBox3.Text = string.Format("{0}", this.board.space_control_evaluation(true, Bot.MULT_board_protection_mod));
            this.textBox3.Refresh();



            this.textBox6.Text = string.Format("{0}", this.board.total_piece_value_black());
            this.textBox6.Refresh();

            this.textBox5.Text = string.Format("{0}", this.board.total_threat_level_white());
            this.textBox5.Refresh();

            this.textBox4.Text = string.Format("{0}", this.board.space_control_evaluation(false, Bot.MULT_board_protection_mod));
            this.textBox4.Refresh();
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
            update_textboxes();

            if (this.board.whites_turn == false && false)
            {
                Bot.depth = 2;
                this.board.play_move(Bot.get_best_move(this.board));
                this.ChessBoardPanel.Refresh();
                // also redraw some GUI stuff
                label1.Refresh();
                update_textboxes();
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

        private void ChessBoardPanel_MouseHover(object sender, EventArgs e)
        {
            Point rel_pos = ChessBoardPanel.PointToClient(Cursor.Position);

            int tile_x = (rel_pos.X - ChessBoard.boarder_w) / ChessBoard.tile_w;
            int tile_y = (rel_pos.Y - ChessBoard.boarder_w) / ChessBoard.tile_h;
            if (tile_x < 0 || tile_x > 7) return;
            if (tile_y < 0 || tile_y > 7) return;

            // flip y so that 0 is at the bottom
            tile_y = 7 - tile_y;

            ChessPiece hover_piece = this.board.get_piece_at(tile_x, tile_y);

            if (hover_piece != null)
                Console.WriteLine("Hovering over {0} on ({1}|{2})...", hover_piece.name, tile_x, tile_y);
        }
    }
}
