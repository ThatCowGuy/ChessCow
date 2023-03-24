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
    public class ChessPiece
    {
        public static int count = 0;
        public bool is_white = false;

        public Image rep;
        public bool alive = true;
        public bool selected = false;

        public int x;
        public int y;
        public Rectangle personal_space;

        public string name;

        public Rectangle index_to_rect(int x, int y)
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

        void set_x(int x)
        {
            this.x = x;
        }
        void set_y(int y)
        {
            this.y = y;
        }
        public void set(int x, int y)
        {
            this.set_x(x);
            this.set_y(y);

            this.personal_space = ChessBoard.index_to_rect(this.x, this.y);
        }

        public ChessPiece()
        {
            ChessPiece.count++; Console.WriteLine("Piece created. Count: {0}", count);
        }
        public void Dispose()
        {
            //Dispose(true);
            System.GC.SuppressFinalize(this);
            ChessPiece.count--; Console.WriteLine("Piece destroyed. Count: {0}", count);
        }
        public ChessPiece(int x, int y)
        {
            this.set(x, y);
        }

        public virtual List<Move> get_all_moves(ChessBoard board) { return new List<Move>(); }

        public void draw(Graphics g)
        { 
            if (this.alive == true)
                g.DrawImage(this.rep, this.personal_space);
        }

        public static bool position_out_of_bounds(int x, int y)
        {
            if (x < 0) return true;
            if (x > 7) return true;
            if (y < 0) return true;
            if (y > 7) return true;
            return false;
        }
    }

    public class Pawn : ChessPiece
    {
        public Pawn(int x, int y)
        {
            this.set(x, y);
            this.rep = new Bitmap(Bitmap.FromFile("../../assets/white_pawn.png"), 64, 64);
            this.name = "Pawn";
        }

        public override List<Move> get_all_moves(ChessBoard board)
        {
            List<Move> moves = new List<Move>();
            Move potential_move;

            // for pawns, we need to differentiate between the colors
            if (this.is_white)
            {
                potential_move = new Move(this, this.x, this.y + 1);
                if (potential_move.collides_with_ally(board) == true)
                    return moves;
                if (potential_move.is_out_of_bounds() == false)
                    moves.Add(potential_move);

                // pawn starter move
                if (this.y == 1)
                {
                    potential_move = new Move(this, this.x, this.y + 2);
                    if (potential_move.collides_with_ally(board) == true)
                        return moves;
                    if (potential_move.is_out_of_bounds() == false)
                        moves.Add(potential_move);
                }
            }
            else
            {
                potential_move = new Move(this, this.x, this.y - 1);
                if (potential_move.collides_with_ally(board) == true)
                    return moves;
                if (potential_move.is_out_of_bounds() == false)
                    moves.Add(potential_move);

                // pawn starter move
                if (this.y == 1)
                {
                    potential_move = new Move(this, this.x, this.y - 2);
                    if (potential_move.collides_with_ally(board) == true)
                        return moves;
                    if (potential_move.is_out_of_bounds() == false)
                        moves.Add(potential_move);
                }
            }

            return moves;
        }
    }
    public class Rook : ChessPiece
    {
        public Rook (int x, int y)
        {
            this.set(x, y);
            this.rep = new Bitmap(Bitmap.FromFile("../../assets/white_rook.png"), 64, 64);
            this.name = "Rook";
        }

        public override List<Move> get_all_moves(ChessBoard board)
        {
            List<Move> moves = new List<Move>();
            Move potential_move;

            // Rook Moves are so STRAIGHT FORWARD (HAaaa) that I dont need an OOB-check
            for (int x = this.x - 1; x >= 0; x--)
            {
                potential_move = new Move(this, x, this.y);
                if (potential_move.collides_with_ally(board) == true)
                    break;
                moves.Add(potential_move);
            }
            for (int x = this.x + 1; x <= 7; x++)
            {
                potential_move = new Move(this, x, this.y);
                if (potential_move.collides_with_ally(board) == true)
                    break;
                moves.Add(potential_move);
            }
            for (int y = this.y - 1; y >= 0; y--)
            {
                potential_move = new Move(this, this.x, y);
                if (potential_move.collides_with_ally(board) == true)
                    break;
                moves.Add(potential_move);
            }
            for (int y = this.y + 1; y <= 7; y++)
            {
                potential_move = new Move(this, this.x, y);
                if (potential_move.collides_with_ally(board) == true)
                    break;
                moves.Add(potential_move);
            }

            return moves;
        }
    }
    public class Knight : ChessPiece
    {
        public Knight(int x, int y)
        {
            this.set(x, y);
            this.rep = new Bitmap(Bitmap.FromFile("../../assets/white_knight.png"), 64, 64);
            this.name = "Knight";
        }

        public override List<Move> get_all_moves(ChessBoard board)
        {
            List<Move> moves = new List<Move>();
            Move potential_move;

            potential_move = new Move(this, this.x + 2, this.y + 1);
            if (potential_move.is_out_of_bounds() == false && potential_move.collides_with_ally(board) == false)
                moves.Add(potential_move);
            potential_move = new Move(this, this.x + 1, this.y + 2);
            if (potential_move.is_out_of_bounds() == false && potential_move.collides_with_ally(board) == false)
                moves.Add(potential_move);

            potential_move = new Move(this, this.x - 1, this.y + 2);
            if (potential_move.is_out_of_bounds() == false && potential_move.collides_with_ally(board) == false)
                moves.Add(potential_move);
            potential_move = new Move(this, this.x - 2, this.y + 1);
            if (potential_move.is_out_of_bounds() == false && potential_move.collides_with_ally(board) == false)
                moves.Add(potential_move);

            potential_move = new Move(this, this.x - 2, this.y - 1);
            if (potential_move.is_out_of_bounds() == false && potential_move.collides_with_ally(board) == false)
                moves.Add(potential_move);
            potential_move = new Move(this, this.x - 1, this.y - 2);
            if (potential_move.is_out_of_bounds() == false && potential_move.collides_with_ally(board) == false)
                moves.Add(potential_move);

            potential_move = new Move(this, this.x + 1, this.y - 2);
            if (potential_move.is_out_of_bounds() == false && potential_move.collides_with_ally(board) == false)
                moves.Add(potential_move);
            potential_move = new Move(this, this.x + 2, this.y - 1);
            if (potential_move.is_out_of_bounds() == false && potential_move.collides_with_ally(board) == false)
                moves.Add(potential_move);

            return moves;
        }
    }
    public class Bishop : ChessPiece
    {
        public Bishop(int x, int y)
        {
            this.set(x, y);
            this.rep = new Bitmap(Bitmap.FromFile("../../assets/white_bishop.png"), 64, 64);
            this.name = "Bishop";
        }

        public override List<Move> get_all_moves(ChessBoard board)
        {
            List<Move> moves = new List<Move>();
            Move potential_move;

            int x, y;

            // Bishop Moves are so straight forward that I dont need an OOB-check
            for (x = this.x + 1, y = this.y + 1; x <= 7 && y <= 7; x++, y++)
            {
                potential_move = new Move(this, x, y);
                if (potential_move.collides_with_ally(board) == true)
                    break;
                moves.Add(potential_move);
            }
            for (x = this.x - 1, y = this.y + 1; x >= 0 && y <= 7; x--, y++)
            {
                potential_move = new Move(this, x, y);
                if (potential_move.collides_with_ally(board) == true)
                    break;
                moves.Add(potential_move);
            }
            for (x = this.x - 1, y = this.y - 1; x >= 0 && y >= 0; x--, y--)
            {
                potential_move = new Move(this, x, y);
                if (potential_move.collides_with_ally(board) == true)
                    break;
                moves.Add(potential_move);
            }
            for (x = this.x + 1, y = this.y - 1; x<= 7 && y >= 0; x++, y--)
            {
                potential_move = new Move(this, x, y);
                if (potential_move.collides_with_ally(board) == true)
                    break;
                moves.Add(potential_move);
            }

            return moves;
        }
    }
    public class Queen : ChessPiece
    {
        public Queen(int x, int y)
        {
            this.set(x, y);
            this.rep = new Bitmap(Bitmap.FromFile("../../assets/white_queen.png"), 64, 64);
            this.name = "Queen";

            for (int i = 0; i < 1000; i++)
            {
                ChessPiece dummy_rook = new Rook(this.x, this.y);
                dummy_rook.Dispose();
            }
        }
        public override List<Move> get_all_moves(ChessBoard board)
        {
            List<Move> moves = new List<Move>();
            Move potential_move;

            // add all Rook Moves
            ChessPiece dummy_rook = new Rook(this.x, this.y);
            dummy_rook.is_white = this.is_white;
            List<Move> rook_moves = dummy_rook.get_all_moves(board);
            // need to override which piece is actually moving...
            foreach (Move move in rook_moves)
            {
                move.moving_piece = this;
                moves.Add(move);
            }
            dummy_rook.Dispose();

            // add all Bishop Moves
            ChessPiece dummy_bishop = new Bishop(this.x, this.y);
            dummy_bishop.is_white = this.is_white;
            List<Move> bishop_moves = dummy_bishop.get_all_moves(board);
            // need to override which piece is actually moving...
            foreach (Move move in bishop_moves)
            {
                move.moving_piece = this;
                moves.Add(move);
            }
            dummy_bishop.Dispose();

            return moves;
        }
    }
    public class King : ChessPiece
    {
        public King(int x, int y)
        {
            System.Console.WriteLine("DA BAUS\n");

            this.set(x, y);
            this.rep = new Bitmap(Bitmap.FromFile("../../assets/white_king.png"), 64, 64);
            this.name = "King";
        }
    }
}
