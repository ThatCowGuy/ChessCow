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
    public class ChessGame
    {
        public List<ChessBoard> board_states = new List<ChessBoard>();
        public ChessBoard current_state;
        public int displayed_state_index;

        public ChessBoardMini mini_board;

        // images
        public static Image IMG_selector;
        public int selector_x;
        public int selector_y;

        public static bool WHITE = true;
        public static bool BLACK = false;

        public ChessGame()
        {
            this.board_states = new List<ChessBoard>();

            ChessBoard initial = new ChessBoard();
            this.add_state(initial);
            this.current_state = initial;
            this.displayed_state_index = 0;

            this.mini_board = new ChessBoardMini();
            ChessGame.IMG_selector = new Bitmap(Bitmap.FromFile("../../assets/selector.png"), 64, 64);
            this.selector_x = -1;
            this.selector_y = -1;
        }

        public void add_state(ChessBoard board)
        {
            // insert new addition into the 2nd to last position; pushing current state up
            if (this.board_states.Count > 0)
                this.board_states.Insert(this.board_states.Count - 1, board);
            else
                this.board_states.Add(board);
               
            // reset the displayed state index
            this.displayed_state_index = this.board_states.Count - 1;
        }
        public void pop_state_soft()
        {
            // pop last element
            this.board_states.RemoveAt(this.board_states.Count - 1);

            // but dont touch current state !
        }
        public void pop_state_hard()
        {
            // pop last element
            this.board_states.RemoveAt(this.board_states.Count - 1);
            // and update current state
            this.current_state = this.board_states.ElementAt(this.board_states.Count - 1);
            this.displayed_state_index = this.board_states.Count - 1;
        }

        public ChessBoard get_latest_state()
        {
            return this.board_states.ElementAt(this.board_states.Count - 1);
        }

        public void process_click(int tile_x, int tile_y)
        {
            if (this.selector_x == -1)
            {
                if (mini_board.xy_is_piece_of_color(tile_x, tile_y, mini_board.whites_turn) == true)
                {
                    this.selector_x = tile_x;
                    this.selector_y = tile_y;

                    // fill in the moves list
                    //this.current_state.piece_legal_moves = this.current_state.selected_piece.get_all_moves(this, this.current_state.selected_piece, true);

                    //int threats = this.current_state.selected_piece.threat_count(this);
                    //Console.WriteLine("Threats: {0}", threats);
                }
            }
            else
            {
                this.selector_x = -1;
                this.selector_y = -1;

                // see if the clicked space is a legal move of the selected piece

                // empty out the moves list
                this.current_state.piece_legal_moves = new List<Move>();
            }
        }

        public void execute_move(Move move)
        {
            // insert a copy of the current state into the list
            ChessBoard last_state = new ChessBoard(this.current_state);
            this.add_state(last_state);

            // then make changes on current state itself
            this.current_state.execute_move(move);
        }
        public void play_move(Move move)
        {
            // insert a copy of the current state into the list
            ChessBoard last_state = new ChessBoard(this.current_state);
            this.add_state(last_state);

            // then make changes on current state itself
            this.current_state.play_move(move);

            System.Console.WriteLine("State Count = {0}", this.board_states.Count);
        }

        public void undo_move()
        {
            this.pop_state_hard();
        }


        public void draw(Graphics g)
        {
            //this.board_states.ElementAt(this.displayed_state_index).draw(g);
            this.mini_board.draw(g);

            if (this.selector_x != -1)
                g.DrawImage(ChessGame.IMG_selector, ChessBoardMini.xy_to_rect(selector_x, selector_y));
        }
    }
}
