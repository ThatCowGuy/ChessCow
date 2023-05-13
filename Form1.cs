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
        public ChessGame game = new ChessGame();

        public Form1()
        {
            InitializeComponent();
            update_textboxes();
            this.game = new ChessGame();
        }

        private void ChessBoardPanel_Paint(object sender, PaintEventArgs e)
        {
            // Creating a Graphics Object when the "Paint" thing in the Form is called
            Graphics g = e.Graphics;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            this.game.draw(g);
        }

        public void update_textboxes()
        {
            this.textBox1.Text = string.Format("{0}", this.game.current_state.total_piece_value_white());
            this.textBox1.Refresh();

            // I want to display the threat that white exerts
            this.textBox2.Text = string.Format("{0}", this.game.current_state.total_threat_level_black());
            this.textBox2.Refresh();

            this.textBox3.Text = string.Format("{0}", this.game.current_state.space_control_evaluation(true, Bot.MULT_board_protection_mod));
            this.textBox3.Refresh();



            this.textBox6.Text = string.Format("{0}", this.game.current_state.total_piece_value_black());
            this.textBox6.Refresh();

            this.textBox5.Text = string.Format("{0}", this.game.current_state.total_threat_level_white());
            this.textBox5.Refresh();

            this.textBox4.Text = string.Format("{0}", this.game.current_state.space_control_evaluation(false, Bot.MULT_board_protection_mod));
            this.textBox4.Refresh();



            this.textBox7.Text = string.Format("{0}", this.game.board_states.Count - 1);
            this.textBox7.Refresh();

            this.textBox8.Text = string.Format("{0}", this.game.displayed_state_index);
            this.textBox8.Refresh();
        }

        private void ChessBoardPanel_MouseClick(object sender, MouseEventArgs e)
        {
            int tile_x = (e.X - ChessBoard.boarder_w) / ChessBoard.tile_w;
            int tile_y = (e.Y - ChessBoard.boarder_w) / ChessBoard.tile_h;
            if (tile_x < 0 || tile_x > 7) return;
            if (tile_y < 0 || tile_y > 7) return;

            // flip y so that 0 is at the bottom
            tile_y = 7 - tile_y;

            MouseEventArgs mouse_e = (MouseEventArgs)e;
            if (mouse_e.Button == MouseButtons.Right)
            {
                Console.WriteLine(string.Format("Inspecting Tile ({0}|{1}):", tile_x, tile_y));
                if (this.game.current_state.occupation[tile_x, tile_y] != null)
                    Console.WriteLine(this.game.current_state.occupation[tile_x, tile_y]);

                else Console.WriteLine("NULL");
            }

            this.game.process_click(tile_x, tile_y);
            this.ChessBoardPanel.Refresh();
            // also redraw some GUI stuff
            label1.Refresh();
            update_textboxes();

            if (this.game.current_state.whites_turn == false && true)
            {
                this.game.play_move(Bot.get_one_best_move(this.game.current_state));
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
            if (this.game.current_state.gamestate == ChessBoard.GameState.ONGOING)
            {
                if (this.game.current_state.whites_turn == true)
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
            else if (this.game.current_state.gamestate == ChessBoard.GameState.WHITE_CHECKMATE)
            {
                label1.Text = "Checkmate!";
                label1.BackColor = Color.FromArgb(255, 255, 155, 155);
            }
            else if (this.game.current_state.gamestate == ChessBoard.GameState.BLACK_CHECKMATE)
            {
                label1.Text = "Checkmate!";
                label1.BackColor = Color.FromArgb(255, 255, 155, 155);
            }
            else if (this.game.current_state.gamestate == ChessBoard.GameState.STALEMATE)
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

            ChessPiece hover_piece = this.game.current_state.get_piece_at(tile_x, tile_y);

            if (hover_piece != null)
                Console.WriteLine("Hovering over {0} on ({1}|{2})...", hover_piece.name, tile_x, tile_y);
        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            return;
        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {
            try
            {
                int new_index = Int32.Parse(textBox8.Text);

                // check if the new index is even valid
                if (new_index >= 0 && new_index < this.game.board_states.Count)
                    this.game.displayed_state_index = new_index;

                // and fix the content of this textbox just in case
                textBox8.Text = string.Format("{0}", this.game.displayed_state_index);

                this.Refresh();
            }
            catch (Exception error)
            {
                // fix the content of this textbox just in case
                textBox8.Text = string.Format("{0}", this.game.displayed_state_index);

                this.Refresh();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int new_index = this.game.displayed_state_index - 1;

            // check if the new index is even valid
            if (new_index >= 0 && new_index < this.game.board_states.Count)
                this.game.displayed_state_index = new_index;

            // and fix the content of this textbox just in case
            textBox8.Text = string.Format("{0}", this.game.displayed_state_index);

            this.Refresh();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            int new_index = this.game.displayed_state_index + 1;

            // check if the new index is even valid
            if (new_index >= 0 && new_index < this.game.board_states.Count)
                this.game.displayed_state_index = new_index;

            // and fix the content of this textbox just in case
            textBox8.Text = string.Format("{0}", this.game.displayed_state_index);

            this.Refresh();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.game.displayed_state_index = this.game.board_states.Count - 1;
            textBox8.Text = string.Format("{0}", this.game.displayed_state_index);
            this.Refresh();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            using (StreamWriter writer = new StreamWriter("../../pieces_log.txt"))
            {
                ChessPiece piece = null;
                string line;
                for (int i = 0; i < 16; i++)
                {
                    piece = this.game.current_state.white_pieces[i];
                    line = string.Format("{0} on ({1}|{2}) is ", piece.name, piece.x, piece.y);
                    if (piece.alive == true) line += "alive.";
                    if (piece.alive == false) line += "DEAD!";
                    writer.WriteLine(line);
                }
                writer.WriteLine("---------------------");
                foreach (Move move in this.game.current_state.white_legal_moves)
                { 
                    writer.WriteLine(move);
                }
                writer.WriteLine("==========================");

                for (int i = 0; i < 16; i++)
                {
                    piece = this.game.current_state.black_pieces[i];
                    line = string.Format("{0} on ({1}|{2}) is ", piece.name, piece.x, piece.y);
                    if (piece.alive == true) line += "alive.";
                    if (piece.alive == false) line += "DEAD!";
                    writer.WriteLine(line);
                }
                writer.WriteLine("---------------------");
                foreach (Move move in this.game.current_state.black_legal_moves)
                {
                    writer.WriteLine(move);
                }
                writer.WriteLine("==========================");

                piece = this.game.current_state.en_passant_target;
                line = string.Format("{0} on ({1}|{2}) is ", piece.name, piece.x, piece.y);
                if (piece.alive == true) line += "alive.";
                if (piece.alive == false) line += "DEAD!";
                writer.WriteLine(line);
            }
        }
    }
}
