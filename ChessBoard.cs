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
        public int turn;
        public bool piece_selected = false;

        public ChessPiece[] white_pieces;
        public ChessPiece[] black_pieces;
        public ChessPiece[,] occupation;
        public ChessPiece selected_piece;

        public List<Move> legal_moves = new List<Move>();

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
                this.white_pieces[index] = new Pawn(index, 1);
            this.white_pieces[index++] = new Rook(0, 0);
            this.white_pieces[index++] = new Knight(1, 0);
            this.white_pieces[index++] = new Bishop(2, 0);
            this.white_pieces[index++] = new Queen(3, 0);
            this.white_pieces[index++] = new King(4, 0);
            this.white_pieces[index++] = new Bishop(5, 0);
            this.white_pieces[index++] = new Knight(6, 0);
            this.white_pieces[index++] = new Rook(7, 0);
            // appoint all the allocated pieces to the board
            for (index = 0; index < 16; index++)
            {
                this.white_pieces[index].is_white = true;

                int x = this.white_pieces[index].x;
                int y = this.white_pieces[index].y;
                this.occupation[x, y] = this.white_pieces[index];
            }

            this.black_pieces = new ChessPiece[16];
        }

        public void process_click(int tile_x, int tile_y)
        {
            if (this.piece_selected == false)
            {
                if (this.occupation[tile_x, tile_y] != null)
                {
                    this.selected_piece = this.occupation[tile_x, tile_y];
                    this.selected_piece.selected = true;
                    this.piece_selected = true;

                    // fill in the moves list
                    this.legal_moves = this.selected_piece.get_all_moves(this);

                    Console.WriteLine("Clicked on {0}", this.selected_piece.name);
                }
            }
            else
            {
                // see if the clicked space is a legal move of the selected piece
                Move move = new Move(this.selected_piece, tile_x, tile_y);

                if (this.legal_moves.Contains(move))
                {
                    this.play_move(this.selected_piece, move);
                }

                this.selected_piece.selected = false;
                this.selected_piece = null;
                this.piece_selected = false;

                // empty out the moves list
                this.legal_moves = new List<Move>();

                Console.WriteLine("Deselected Piece");
            }


        }

        public void play_move(ChessPiece piece, Move move)
        {
            // clear old occupation
            this.occupation[piece.x, piece.y] = null;

            // set new position
            piece.set(move.target_x, move.target_y);
            // and update the new occupation
            this.occupation[piece.x, piece.y] = piece;
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
                //this.black_pieces[i].draw(g);
            }

            foreach (Move move in this.legal_moves)
            {
                g.DrawImage(Move.legal_move_rep, index_to_rect(move.target_x, move.target_y));
            }

            Pen pen = new Pen(Color.Gray, boarder_w * 2);
            g.DrawRectangle(pen, 0, 0, full_w, full_h);
            pen.Dispose();
        }
    }
}
