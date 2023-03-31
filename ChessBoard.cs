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
    public class ChessBoard
    {
        public GameState gamestate = GameState.ONGOING;

        public int turn;
        public bool whites_turn = true;
        public bool piece_selected = false;

        public ChessPiece[] white_pieces;
        public ChessPiece[] black_pieces;
        public ChessPiece[,] occupation;
        public ChessPiece selected_piece;

        public Move last_played_move = null;

        public List<Move> piece_legal_moves = new List<Move>();
        public List<Move> white_legal_moves = new List<Move>();
        public List<Move> black_legal_moves = new List<Move>();
        public List<Move> white_protect_moves = new List<Move>();
        public List<Move> black_protect_moves = new List<Move>();
        public List<Move> white_old_legal_moves = new List<Move>();
        public List<Move> black_old_legal_moves = new List<Move>();

        public static int xdim = 8;
        public static int ydim = 8;

        public static int field_w = 512;
        public static int field_h = 512;
        public static int tile_w = field_w / ChessBoard.xdim;
        public static int tile_h = field_h / ChessBoard.ydim;
        public static int boarder_w = 5;
        public static int full_w = field_w + (2 * boarder_w);
        public static int full_h = field_h + (2 * boarder_w);

        public enum GameState
        {
            ONGOING,
            WHITE_CHECKMATE,
            BLACK_CHECKMATE,
            STALEMATE,
            DRAW
        };

        public ChessBoard()
        {
            this.init_normal();
        }

        public ChessPiece get_occupation(int tile_x, int tile_y)
        {
            return this.occupation[tile_x, tile_y];
        }

        public void init_normal()
        {
            System.Console.WriteLine("Initializing Chessboard normally...\n");

            this.occupation = new ChessPiece[8, 8];
            int index = 0;

            // allocate and create all the white pieces
            this.white_pieces = new ChessPiece[16];
            for (index = 0; index < 8; index++)
                this.white_pieces[index] = new Pawn(ChessPiece.IS_WHITE, index, 1);
            this.white_pieces[index++] = new Rook(ChessPiece.IS_WHITE, 0, 0);
            this.white_pieces[index++] = new Knight(ChessPiece.IS_WHITE, 1, 0);
            this.white_pieces[index++] = new Bishop(ChessPiece.IS_WHITE, 2, 0);
            this.white_pieces[index++] = new Queen(ChessPiece.IS_WHITE, 3, 0);
            this.white_pieces[index++] = new King(ChessPiece.IS_WHITE, 4, 0);
            this.white_pieces[index++] = new Bishop(ChessPiece.IS_WHITE, 5, 0);
            this.white_pieces[index++] = new Knight(ChessPiece.IS_WHITE, 6, 0);
            this.white_pieces[index++] = new Rook(ChessPiece.IS_WHITE, 7, 0);
            // appoint all the allocated pieces to the board
            for (index = 0; index < 16; index++)
            {
                int x = this.white_pieces[index].x;
                int y = this.white_pieces[index].y;
                this.occupation[x, y] = this.white_pieces[index];
            }

            // allocate and create all the black pieces
            this.black_pieces = new ChessPiece[16];
            for (index = 0; index < 8; index++)
                this.black_pieces[index] = new Pawn(ChessPiece.IS_BLACK, index, 6);
            this.black_pieces[index++] = new Rook(ChessPiece.IS_BLACK, 0, 7);
            this.black_pieces[index++] = new Knight(ChessPiece.IS_BLACK, 1, 7);
            this.black_pieces[index++] = new Bishop(ChessPiece.IS_BLACK, 2, 7);
            this.black_pieces[index++] = new Queen(ChessPiece.IS_BLACK, 3, 7);
            this.black_pieces[index++] = new King(ChessPiece.IS_BLACK, 4, 7);
            this.black_pieces[index++] = new Bishop(ChessPiece.IS_BLACK, 5, 7);
            this.black_pieces[index++] = new Knight(ChessPiece.IS_BLACK, 6, 7);
            this.black_pieces[index++] = new Rook(ChessPiece.IS_BLACK, 7, 7);
            // appoint all the allocated pieces to the board
            for (index = 0; index < 16; index++)
            {
                int x = this.black_pieces[index].x;
                int y = this.black_pieces[index].y;
                this.occupation[x, y] = this.black_pieces[index];
            }

            this.calc_legal_moves(true);
        }

        public List<Move> get_all_moves(bool for_white, bool consider_selfcheck)
        {
            List<Move> moves = new List<Move>();

            if (for_white == true)
            {
                for (int i = 0; i < 8; i++)
                    moves.AddRange(Pawn.get_all_moves(this, this.white_pieces[i], consider_selfcheck));
                moves.AddRange(Rook.get_all_moves(this, this.white_pieces[8], consider_selfcheck));
                moves.AddRange(Knight.get_all_moves(this, this.white_pieces[9], consider_selfcheck));
                moves.AddRange(Bishop.get_all_moves(this, this.white_pieces[10], consider_selfcheck));
                moves.AddRange(Queen.get_all_moves(this, this.white_pieces[11], consider_selfcheck));
                moves.AddRange(King.get_all_moves(this, this.white_pieces[12], consider_selfcheck));
                moves.AddRange(Bishop.get_all_moves(this, this.white_pieces[13], consider_selfcheck));
                moves.AddRange(Knight.get_all_moves(this, this.white_pieces[14], consider_selfcheck));
                moves.AddRange(Rook.get_all_moves(this, this.white_pieces[15], consider_selfcheck));
            }
            if (for_white == false)
            {
                for (int i = 0; i < 8; i++)
                    moves.AddRange(Pawn.get_all_moves(this, this.black_pieces[i], consider_selfcheck));
                moves.AddRange(Rook.get_all_moves(this, this.black_pieces[8], consider_selfcheck));
                moves.AddRange(Knight.get_all_moves(this, this.black_pieces[9], consider_selfcheck));
                moves.AddRange(Bishop.get_all_moves(this, this.black_pieces[10], consider_selfcheck));
                moves.AddRange(Queen.get_all_moves(this, this.black_pieces[11], consider_selfcheck));
                moves.AddRange(King.get_all_moves(this, this.black_pieces[12], consider_selfcheck));
                moves.AddRange(Bishop.get_all_moves(this, this.black_pieces[13], consider_selfcheck));
                moves.AddRange(Knight.get_all_moves(this, this.black_pieces[14], consider_selfcheck));
                moves.AddRange(Rook.get_all_moves(this, this.black_pieces[15], consider_selfcheck));
            }
            return moves;
        }

        public List<Move> get_piece_moves(ChessPiece piece)
        {
            List<Move> piece_moves = new List<Move>();
            if (piece == null)
                return piece_moves;

            if (piece.is_white == true)
            {
                foreach (Move move in this.white_legal_moves)
                {
                    if (move.moving_piece == piece)
                        piece_moves.Add(move);
                }
            }
            if (piece.is_white == false)
            {
                foreach (Move move in this.black_legal_moves)
                {
                    if (move.moving_piece == piece)
                        piece_moves.Add(move);
                }
            }
            return piece_moves;
        }

        public void calc_legal_moves(bool consider_selfcheck)
        {
            this.black_old_legal_moves = this.black_legal_moves;
            this.white_old_legal_moves = this.white_legal_moves;

            this.white_legal_moves = this.get_all_moves(true, consider_selfcheck);
            this.black_legal_moves = this.get_all_moves(false, consider_selfcheck);

            this.white_protect_moves = new List<Move>();
            foreach (Move move in this.white_legal_moves)
            {
                if (move.attack_state == Move.AttackState.PROTECTION)
                    this.white_protect_moves.Add(move);
            }
            this.white_legal_moves.RemoveAll(o => o.attack_state == Move.AttackState.PROTECTION);

            this.black_protect_moves = new List<Move>();
            foreach (Move move in this.black_legal_moves)
            {
                if (move.attack_state == Move.AttackState.PROTECTION)
                    this.black_protect_moves.Add(move);
            }
            this.black_legal_moves.RemoveAll(o => o.attack_state == Move.AttackState.PROTECTION);

            //this.white_legal_moves = this.white_legal_moves.OrderBy(o => o.value).ToList();
            //this.black_legal_moves = this.black_legal_moves.OrderBy(o => o.value).ToList();
        }
        public void restore_legal_moves()
        {
            this.black_legal_moves = this.black_old_legal_moves;
            this.white_legal_moves = this.white_old_legal_moves;
        }



        public void process_click(int tile_x, int tile_y)
        {
            if (this.piece_selected == false)
            {
                if (this.occupation[tile_x, tile_y] != null)
                {
                    // check if the selected piece is "yours"
                    if (this.occupation[tile_x, tile_y].is_white != this.whites_turn)
                        return;

                    this.selected_piece = this.occupation[tile_x, tile_y];
                    this.selected_piece.selected = true;
                    this.piece_selected = true;

                    // just calling this to read some outputs
                    this.selected_piece.threat_level(this);

                    // fill in the moves list
                    //this.piece_legal_moves = this.selected_piece.get_all_moves(this, this.selected_piece, true);

                    //int threats = this.selected_piece.threat_count(this);
                    //Console.WriteLine("Threats: {0}", threats);
                }
            }
            else
            {
                // see if the clicked space is a legal move of the selected piece
                Move selected_move = new Move(this, this.selected_piece, tile_x, tile_y, Move.AttackState.BOTH);

                foreach (Move move in this.get_piece_moves(this.selected_piece))
                {
                    if (selected_move.Equals(move) == true)
                    {
                        this.play_move(move);
                    }
                }

                this.selected_piece.selected = false;
                this.selected_piece = null;
                this.piece_selected = false;

                // empty out the moves list
                this.piece_legal_moves = new List<Move>();
            }
        }

        public double total_threat_level_white()
        {
            double total_threat_level = 0;
            for (int i = 0; i < 16; i++)
            {
                if (this.white_pieces[i].alive == true)
                    total_threat_level += this.white_pieces[i].threat_level(this);
            }
            return total_threat_level;
        }
        public double total_threat_level_black()
        {
            double total_threat_level = 0;
            for (int i = 0; i < 16; i++)
            {
                if (this.black_pieces[i].alive == true)
                    total_threat_level += this.black_pieces[i].threat_level(this);
            }
            return total_threat_level;
        }
        public double total_piece_value_white()
        {
            double total_piece_value = 0;
            for (int i = 0; i < 16; i++)
            {
                if (this.white_pieces[i].alive == true)
                    total_piece_value += this.white_pieces[i].value;
            }
            return total_piece_value;
        }
        public double total_piece_value_black()
        {
            double total_piece_value = 0;
            for (int i = 0; i < 16; i++)
            {
                if (this.black_pieces[i].alive == true)
                    total_piece_value += this.black_pieces[i].value;
            }
            return total_piece_value;
        }


        public void play_move(Move move)
        {
            // some outputs
            Console.WriteLine("Playing {0} to {1}|{2}", move.moving_piece.name, move.target_x, move.target_y);
            if (move.target_piece != null)
            {
                Console.WriteLine(">>> Disposing of Piece: {0}", move.target_piece.name);
                //move.target_piece.alive = false; -- this happens in execute_move() already
            }

            this.execute_move(move);
            this.last_played_move = move;

            // and update the legal moves array
            this.calc_legal_moves(true);

            // check if the next player can even move
            if (whites_turn == true)
            {
                if (this.white_legal_moves.Any() == false)
                {
                    if (this.white_pieces[12].threat_count(this) > 0)
                        this.gamestate = GameState.WHITE_CHECKMATE;

                    else this.gamestate = GameState.STALEMATE;
                }
            }
            if (whites_turn == false)
            {
                if (this.black_legal_moves.Any() == false)
                {
                    if (this.black_pieces[12].threat_count(this) > 0)
                        this.gamestate = GameState.BLACK_CHECKMATE;

                    else this.gamestate = GameState.STALEMATE;
                }
            }
        }

        public void simulate_move(Move move, bool consider_selfcheck)
        {
            this.execute_move(move);

            // and update the legal moves array
            this.calc_legal_moves(consider_selfcheck);
        }

        // this LITERALLY only executes the move; NO legality checks prior or afterwards,
        // no outputs, no.. anything
        public void execute_move(Move move)
        {
            if (move == null)
            {
                Console.WriteLine("Trying to execute NULL move...");
                return;
            }

            // clear old occupation
            this.occupation[move.moving_piece.x, move.moving_piece.y] = null;

            // check if we are taking a piece and remove it if so
            if (move.target_piece != null)
            {
                // Console.WriteLine("Disposing of Piece...");
                if (whites_turn == true)
                    Console.WriteLine("HUA");
                move.target_piece.alive = false;
            }

            // set new position
            move.moving_piece.set(move.target_x, move.target_y);
            move.moving_piece.move_count++;
            // and update the new occupation
            this.occupation[move.moving_piece.x, move.moving_piece.y] = move.moving_piece;

            // change whose turn it is
            this.whites_turn = !this.whites_turn;
        }


        public void undo_move(Move move)
        {
            // check if we took a piece and reinstate it if so
            if (move.target_piece != null)
            {
                //Console.WriteLine("Disposing of Piece...");
                move.target_piece.alive = true;
            }
            // this is the correct thing to do, even if target_piece == NULL
            this.occupation[move.target_x, move.target_y] = move.target_piece;

            // set old position
            move.moving_piece.set(move.origin_x, move.origin_y);
            move.moving_piece.move_count--;
            // and update the new occupation
            this.occupation[move.origin_x, move.origin_y] = move.moving_piece;

            // change whose turn it is
            this.whites_turn = !this.whites_turn;
            // and update the legal moves array
            this.restore_legal_moves();
        }


        public bool checking_white_king()
        {
            return (this.white_pieces[12].threat_count(this) > 0);
        }
        public bool checking_black_king()
        {
            return (this.black_pieces[12].threat_count(this) > 0);
        }
        public static Rectangle index_to_rect(int x, int y)
        {
            // flip y index so 0 is at the bottom
            y = 7 - y;

            Rectangle rect = new Rectangle(
                ChessBoard.boarder_w + (ChessBoard.tile_w * x),
                ChessBoard.boarder_w + (ChessBoard.tile_h * y),
                ChessBoard.tile_w, ChessBoard.tile_h
            );
            return rect;
        }

        public void draw(Graphics g)
        {
            SolidBrush brush = new SolidBrush(Color.White);

            // draw 1 big white square
            g.FillRectangle(brush, boarder_w, boarder_w, field_w, field_h);

            // and then the 32 black squares on top
            brush.Color = System.Drawing.Color.Black;
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    int ID = (row % 2) + (col % 2);
                    if (ID % 2 == 1)
                    {
                        g.FillRectangle(brush, boarder_w + (tile_w * col), boarder_w + (tile_h * row), tile_w, tile_h);
                    }
                }
            }

            for (int i = 0; i < 16; i++)
            {
                this.white_pieces[i].draw(g);
                this.black_pieces[i].draw(g);
            }




            foreach (Move move in this.get_piece_moves(this.selected_piece))
            {
                if (move.attack_state == Move.AttackState.BOTH)
                {
                    if (move.target_piece != null)
                        g.DrawImage(Move.attack_move_rep, index_to_rect(move.target_x, move.target_y));
                    else
                        g.DrawImage(Move.legal_move_rep, index_to_rect(move.target_x, move.target_y));
                }
                else if (move.attack_state == Move.AttackState.PURE_ATTACK)
                    g.DrawImage(Move.attack_move_rep, index_to_rect(move.target_x, move.target_y));
                else if (move.attack_state == Move.AttackState.PURE_MOVEMENT)
                    g.DrawImage(Move.legal_move_rep, index_to_rect(move.target_x, move.target_y));
                // AttackState.PROTECTION is obviously ignored
            }
            // draw a selector around the selected piece
            if (this.piece_selected == true)
                g.DrawImage(Move.selector, index_to_rect(this.selected_piece.x, this.selected_piece.y));
            
            Pen pen = new Pen(Color.Gray, boarder_w * 2);
            g.DrawRectangle(pen, 0, 0, full_w, full_h);

            // draw cute check flags
            if (this.checking_white_king() == true)
                g.DrawImage(Move.check_flag, index_to_rect(this.white_pieces[12].x, this.white_pieces[12].y));
            if (this.checking_black_king() == true)
                g.DrawImage(Move.check_flag, index_to_rect(this.black_pieces[12].x, this.black_pieces[12].y));

            if (this.last_played_move != null)
            {
                g.DrawImage(Move.move_marker, index_to_rect(this.last_played_move.origin_x, this.last_played_move.origin_y));
                g.DrawImage(Move.move_marker, index_to_rect(this.last_played_move.target_x, this.last_played_move.target_y));
            }

            brush.Dispose();
            pen.Dispose();
        }
    }
}
