﻿using System;
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
        public EnPassantTarget en_passant_target;

        public Move last_played_move = null;

        public List<Move> piece_legal_moves = new List<Move>();
        public List<Move> white_legal_moves = new List<Move>();
        public List<Move> black_legal_moves = new List<Move>();

        public List<Move> white_protect_moves = new List<Move>();
        public List<Move> black_protect_moves = new List<Move>();

        public List<Move> white_control_moves = new List<Move>();
        public List<Move> black_control_moves = new List<Move>();

        public List<Move> white_old_legal_moves = new List<Move>();
        public List<Move> black_old_legal_moves = new List<Move>();

        public List<Move> white_old_protect_moves = new List<Move>();
        public List<Move> black_old_protect_moves = new List<Move>();

        public List<Move> white_old_control_moves = new List<Move>();
        public List<Move> black_old_control_moves = new List<Move>();

        public static int xdim = 8;
        public static int ydim = 8;

        public static int field_w = 512;
        public static int field_h = 512;
        public static int tile_w = field_w / ChessBoard.xdim;
        public static int tile_h = field_h / ChessBoard.ydim;
        public static int boarder_w = 5;
        public static int full_w = field_w + (2 * boarder_w);
        public static int full_h = field_h + (2 * boarder_w);

        // cloning
        public ChessBoard(ChessBoard board)
        {
            this.whites_turn = board.whites_turn;

            this.occupation = new ChessPiece[8, 8];
            int index = 0;

            // allocate and create all the white pieces
            this.white_pieces = new ChessPiece[16];
            for (index = 0; index < 8; index++)
                this.white_pieces[index] = new Pawn((Pawn)board.white_pieces[index]);
            this.white_pieces[index] = new Rook((Rook)board.white_pieces[index++]);
            this.white_pieces[index] = new Knight((Knight)board.white_pieces[index++]);
            this.white_pieces[index] = new Bishop((Bishop)board.white_pieces[index++]);
            this.white_pieces[index] = new Queen((Queen)board.white_pieces[index++]);
            this.white_pieces[index] = new King((King)board.white_pieces[index++]);
            this.white_pieces[index] = new Bishop((Bishop)board.white_pieces[index++]);
            this.white_pieces[index] = new Knight((Knight)board.white_pieces[index++]);
            this.white_pieces[index] = new Rook((Rook)board.white_pieces[index++]);
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
                this.black_pieces[index] = new Pawn((Pawn)board.black_pieces[index]);
            this.black_pieces[index] = new Rook((Rook)board.black_pieces[index++]);
            this.black_pieces[index] = new Knight((Knight)board.black_pieces[index++]);
            this.black_pieces[index] = new Bishop((Bishop)board.black_pieces[index++]);
            this.black_pieces[index] = new Queen((Queen)board.black_pieces[index++]);
            this.black_pieces[index] = new King((King)board.black_pieces[index++]);
            this.black_pieces[index] = new Bishop((Bishop)board.black_pieces[index++]);
            this.black_pieces[index] = new Knight((Knight)board.black_pieces[index++]);
            this.black_pieces[index] = new Rook((Rook)board.black_pieces[index++]);
            // appoint all the allocated pieces to the board
            for (index = 0; index < 16; index++)
            {
                int x = this.black_pieces[index].x;
                int y = this.black_pieces[index].y;
                this.occupation[x, y] = this.black_pieces[index];
            }

            // fake en passant target for easier access
            //this.en_passant_target = new EnPassantTarget(ChessPiece.IS_WHITE, 0, 0);
            //this.en_passant_target.alive = false;

            this.last_played_move = board.last_played_move;

            this.piece_legal_moves = new List<Move>();
            foreach (Move move in board.piece_legal_moves)
                this.piece_legal_moves.Add(new Move(move, this));
            this.white_legal_moves = new List<Move>();
            foreach (Move move in board.white_legal_moves)
                this.white_legal_moves.Add(new Move(move, this));
            this.black_legal_moves = new List<Move>();
            foreach (Move move in board.black_legal_moves)
                this.black_legal_moves.Add(new Move(move, this));

            this.white_protect_moves = new List<Move>();
            foreach (Move move in board.white_protect_moves)
                this.white_protect_moves.Add(new Move(move, this));
            this.black_protect_moves = new List<Move>();
            foreach (Move move in board.black_protect_moves)
                this.black_protect_moves.Add(new Move(move, this));

            this.white_control_moves = new List<Move>();
            foreach (Move move in board.white_control_moves)
                this.white_control_moves.Add(new Move(move, this));
            this.black_control_moves = new List<Move>();
            foreach (Move move in board.black_control_moves)
                this.black_control_moves.Add(new Move(move, this));

            // dont care
            this.white_old_legal_moves = new List<Move>();
            this.black_old_legal_moves = new List<Move>();
            this.white_old_protect_moves = new List<Move>();
            this.black_old_protect_moves = new List<Move>();
            this.white_old_control_moves = new List<Move>();
            this.black_old_control_moves = new List<Move>();
        }
        public ChessBoard()
        {
            this.init_normal();
        }
        public void init_normal()
        {
            System.Console.WriteLine("Initializing Chessboard normally...\n");

            this.occupation = new ChessPiece[8, 8];
            int index = 0;
            int UID;
            
            UID = 0;
            // allocate and create all the white pieces
            this.white_pieces = new ChessPiece[16];
            for (index = 0; index < 8; index++)
                this.white_pieces[index] = new Pawn(ChessPiece.IS_WHITE, index, 1, UID++);
            this.white_pieces[index++] = new Rook(ChessPiece.IS_WHITE, 0, 0, UID++);
            this.white_pieces[index++] = new Knight(ChessPiece.IS_WHITE, 1, 0, UID++);
            this.white_pieces[index++] = new Bishop(ChessPiece.IS_WHITE, 2, 0, UID++);
            this.white_pieces[index++] = new Queen(ChessPiece.IS_WHITE, 3, 0, UID++);
            this.white_pieces[index++] = new King(ChessPiece.IS_WHITE, 4, 0, UID++);
            this.white_pieces[index++] = new Bishop(ChessPiece.IS_WHITE, 5, 0, UID++);
            this.white_pieces[index++] = new Knight(ChessPiece.IS_WHITE, 6, 0, UID++);
            this.white_pieces[index++] = new Rook(ChessPiece.IS_WHITE, 7, 0, UID++);
            // appoint all the allocated pieces to the board
            for (index = 0; index < 16; index++)
            {
                int x = this.white_pieces[index].x;
                int y = this.white_pieces[index].y;
                this.occupation[x, y] = this.white_pieces[index];
            }

            UID = 0;
            // allocate and create all the black pieces
            this.black_pieces = new ChessPiece[16];
            for (index = 0; index < 8; index++)
                this.black_pieces[index] = new Pawn(ChessPiece.IS_BLACK, index, 6, UID++);
            this.black_pieces[index++] = new Rook(ChessPiece.IS_BLACK, 0, 7, UID++);
            this.black_pieces[index++] = new Knight(ChessPiece.IS_BLACK, 1, 7, UID++);
            this.black_pieces[index++] = new Bishop(ChessPiece.IS_BLACK, 2, 7, UID++);
            this.black_pieces[index++] = new Queen(ChessPiece.IS_BLACK, 3, 7, UID++);
            this.black_pieces[index++] = new King(ChessPiece.IS_BLACK, 4, 7, UID++);
            this.black_pieces[index++] = new Bishop(ChessPiece.IS_BLACK, 5, 7, UID++);
            this.black_pieces[index++] = new Knight(ChessPiece.IS_BLACK, 6, 7, UID++);
            this.black_pieces[index++] = new Rook(ChessPiece.IS_BLACK, 7, 7, UID++);
            // appoint all the allocated pieces to the board
            for (index = 0; index < 16; index++)
            {
                int x = this.black_pieces[index].x;
                int y = this.black_pieces[index].y;
                this.occupation[x, y] = this.black_pieces[index];
            }

            // fake en passant target for easier access
            this.en_passant_target = new EnPassantTarget(ChessPiece.IS_WHITE, 0, 0, UID++);
            this.en_passant_target.alive = false;

            this.calc_legal_moves();
        }

        public enum GameState
        {
            ONGOING,
            WHITE_CHECKMATE,
            BLACK_CHECKMATE,
            STALEMATE,
            DRAW
        };


        public ChessPiece get_occupation(int tile_x, int tile_y)
        {
            if (tile_x < 0 || tile_x >= 8) return null;
            if (tile_y < 0 || tile_y >= 8) return null;
            return this.occupation[tile_x, tile_y];
        }


        public List<Move> get_all_moves(bool for_white)
        {
            List<Move> moves = new List<Move>();

            if (for_white == true)
            {
                for (int i = 0; i < 16; i++)
                    if (this.white_pieces[i].alive == true)
                        moves.AddRange(this.white_pieces[i].get_all_moves(this));
            }
            if (for_white == false)
            {
                for (int i = 0; i < 16; i++)
                    if (this.black_pieces[i].alive == true)
                        moves.AddRange(this.black_pieces[i].get_all_moves(this));
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

        public void calc_legal_moves()
        {
            this.white_legal_moves = this.get_all_moves(true);
            this.black_legal_moves = this.get_all_moves(false);

            this.white_protect_moves = new List<Move>();
            this.white_control_moves = new List<Move>();
            foreach (Move move in this.white_legal_moves)
            {
                if (move.attack_state == Move.AttackState.PROTECTION)
                    this.white_protect_moves.Add(move);
                else if (move.attack_state == Move.AttackState.PURE_CONTROL)
                    this.white_control_moves.Add(move);
            }
            this.white_legal_moves.RemoveAll(o => o.attack_state == Move.AttackState.PROTECTION);
            this.white_legal_moves.RemoveAll(o => o.attack_state == Move.AttackState.PURE_CONTROL);

            this.black_protect_moves = new List<Move>();
            this.black_control_moves = new List<Move>();
            foreach (Move move in this.black_legal_moves)
            {
                if (move.attack_state == Move.AttackState.PROTECTION)
                    this.black_protect_moves.Add(move);
                else if (move.attack_state == Move.AttackState.PURE_CONTROL)
                    this.black_control_moves.Add(move);
            }
            this.black_legal_moves.RemoveAll(o => o.attack_state == Move.AttackState.PROTECTION);
            this.black_legal_moves.RemoveAll(o => o.attack_state == Move.AttackState.PURE_CONTROL);
        }
        public void restore_legal_moves()
        {
            this.white_legal_moves = this.white_old_legal_moves;
            this.black_legal_moves = this.black_old_legal_moves;

            this.white_protect_moves = this.white_old_protect_moves;
            this.black_protect_moves = this.black_old_protect_moves;

            this.white_control_moves = this.white_old_control_moves;
            this.black_control_moves = this.black_old_control_moves;
        }


        public double space_control_evaluation(bool for_white, double protection_mod)
        {
            double[] control_matrix = new double[64];

            List<Move> legal_moves;
            List<Move> protect_moves;
            List<Move> control_moves;
            if (for_white == true)
            {
                legal_moves = this.white_legal_moves;
                protect_moves = this.white_protect_moves;
                control_moves = this.white_control_moves;
            }
            else //if (for_white == false)
            {
                legal_moves = this.black_legal_moves;
                protect_moves = this.black_protect_moves;
                control_moves = this.black_control_moves;
            }

            // first, count everything white has as a positive
            foreach (Move move in control_moves)
            {
                // Console.WriteLine("CONTROL {0}", move.ToString());
                int target = (move.target_x + (move.target_y * 8));

                if (control_matrix[target] == 0)
                    control_matrix[target] += 1.0;
                else
                    control_matrix[target] += protection_mod;
            }
            foreach (Move move in legal_moves)
            {
                // this doesnt control shit
                if (move.attack_state == Move.AttackState.PURE_MOVEMENT) continue;

                // Console.WriteLine("LEGAL {0}", move.ToString());

                int target = (move.target_x + (move.target_y * 8));

                if (control_matrix[target] == 0)
                    control_matrix[target] += 1.0;
                else
                    control_matrix[target] += protection_mod;
            }
            foreach (Move move in protect_moves)
            {
                // Console.WriteLine("PROTECT {0}", move.ToString());
                int target = (move.target_x + (move.target_y * 8));

                control_matrix[target] += protection_mod;
            }

            return control_matrix.Sum();
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
            // test for empty move
            if (move == null) return;

            // some outputs
            Console.WriteLine("Playing {0}.", move.ToString());
            if (move.target_piece != null)
            {
                Console.WriteLine("~~~ Disposing of Piece: {0}", move.target_piece.name);
                //move.target_piece.alive = false; -- this happens in execute_move() now
            }
            Console.WriteLine("");

            this.execute_move(move);
            this.last_played_move = move;

            // and update the legal moves array
            this.calc_legal_moves();

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

        // honestly, execut could do the calc-moves part too, but simulate needs it to not happen (or have it happen and dont matter)
        public void play_move_silent(Move move)
        {
            this.execute_move(move);

            // and update the legal moves array
            this.calc_legal_moves();
        }

        public ChessPiece get_piece_at(int x, int y)
        {
            return this.occupation[x, y];
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
        public void undo_simulation(Move move)
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
            if (this.white_pieces[ChessPiece.KING].is_checked(this) == true)
                g.DrawImage(Move.check_flag, index_to_rect(this.white_pieces[12].x, this.white_pieces[12].y));
            if (this.black_pieces[ChessPiece.KING].is_checked(this) == true)
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
