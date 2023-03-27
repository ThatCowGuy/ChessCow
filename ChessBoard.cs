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
        public System.Random RNG = new Random();
        public int turn;
        public bool whites_turn = true;
        public bool piece_selected = false;

        public ChessPiece[] white_pieces;
        public ChessPiece[] black_pieces;
        public ChessPiece[,] occupation;
        public ChessPiece selected_piece;

        public List<Move> piece_legal_moves = new List<Move>();
        public List<Move> all_legal_moves = new List<Move>();

        public static int xdim = 8;
        public static int ydim = 8;

        public static int field_w = 512;
        public static int field_h = 512;
        public static int tile_w = field_w / ChessBoard.xdim;
        public static int tile_h = field_h / ChessBoard.ydim;
        public static int boarder_w = 5;
        public static int full_w = field_w + (2 * boarder_w);
        public static int full_h = field_h + (2 * boarder_w);

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
        }

        public bool check_check(bool testing_white_king)
        {
            List<Move> legal_opponent_moves = new List<Move>();
            for (int i = 0; i < 16; i++)
            {
                if (testing_white_king == true)
                {
                    if (this.black_pieces[i].alive == true)
                        legal_opponent_moves.AddRange(this.black_pieces[i].get_all_moves(this));
                }
                else if (testing_white_king == false)
                {
                    if (this.white_pieces[i].alive == true)
                        legal_opponent_moves.AddRange(this.white_pieces[i].get_all_moves(this));
                }
            }
            foreach (Move move in legal_opponent_moves)
            {
                if (testing_white_king == true)
                {
                    if (move.target_piece == white_pieces[12])
                        return true;
                }
                else if (testing_white_king == false)
                {
                    if (move.target_piece == black_pieces[12])
                        return true;
                }
            }
            return false;
        }

        public void calc_legal_moves()
        {
            this.all_legal_moves = new List<Move>();
            for (int i = 0; i < 16; i++)
            {
                if (this.whites_turn == true)
                {
                    if (this.white_pieces[i].alive == true)
                        this.all_legal_moves.AddRange(this.white_pieces[i].get_all_moves(this));
                }
                else if (this.whites_turn == false)
                {
                    if (this.black_pieces[i].alive == true)
                        this.all_legal_moves.AddRange(this.black_pieces[i].get_all_moves(this));
                }
            }
        }

        public void play_random_move()
        {
            int move_index = this.RNG.Next(0, this.all_legal_moves.Count);
            this.play_move(this.all_legal_moves.ElementAt(move_index));
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

                    // fill in the moves list
                    this.piece_legal_moves = this.selected_piece.get_all_moves(this);

                    Console.WriteLine("Clicked on {0}", this.selected_piece.name);
                }
            }
            else
            {
                // see if the clicked space is a legal move of the selected piece
                Move selected_move = new Move(this, this.selected_piece, tile_x, tile_y, Move.AttackState.BOTH);

                foreach (Move move in this.piece_legal_moves)
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

                Console.WriteLine("Deselected Piece");
            }
        }

        public void play_move(Move move)
        {
            // clear old occupation
            this.occupation[move.moving_piece.x, move.moving_piece.y] = null;

            // check if we are taking a piece and remove it if so
            ChessPiece target_piece = move.target_piece;
            if (target_piece != null)
            {
                Console.WriteLine("Disposing of Piece...");
                target_piece.Dispose();
            }

            // set new position
            move.moving_piece.set(move.target_x, move.target_y);
            move.moving_piece.has_moved = true;
            // and update the new occupation
            this.occupation[move.moving_piece.x, move.moving_piece.y] = move.moving_piece;

            // change whose turn it is
            this.whites_turn = !this.whites_turn;
            // and update the legal moves array
            this.calc_legal_moves();

            if (this.whites_turn == false)
                this.play_random_move();

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
            brush.Dispose();

            for (int i = 0; i < 16; i++)
            {
                this.white_pieces[i].draw(g);
                this.black_pieces[i].draw(g);
            }

            foreach (Move move in this.piece_legal_moves)
            {
                if (move.attack_state == Move.AttackState.BOTH && move.target_piece != null)
                    g.DrawImage(Move.attack_move_rep, index_to_rect(move.target_x, move.target_y));
                else if (move.attack_state == Move.AttackState.PURE_ATTACK)
                    g.DrawImage(Move.attack_move_rep, index_to_rect(move.target_x, move.target_y));

                else g.DrawImage(Move.legal_move_rep, index_to_rect(move.target_x, move.target_y));
            }
            // draw a selector around the selected piece
            if (this.piece_selected == true)
                g.DrawImage(Move.selector, index_to_rect(this.selected_piece.x, this.selected_piece.y));
            
            Pen pen = new Pen(Color.Gray, boarder_w * 2);
            g.DrawRectangle(pen, 0, 0, full_w, full_h);
            pen.Dispose();
        }
    }
}
