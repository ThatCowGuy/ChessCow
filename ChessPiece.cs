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
        // these are just for readability
        public const bool IS_WHITE = true;
        public const bool IS_BLACK = false;

        public static int count = 0;
        public bool is_white = false;
        public bool has_moved = false;

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
            this.alive = false;
            //Dispose(true);
            //System.GC.SuppressFinalize(this);
            Console.WriteLine("My Name was {0}", this.name);

            ChessPiece.count--;
            Console.WriteLine("Piece destroyed. Count: {0}", count);
        }
        public ChessPiece(bool is_white, int x, int y)
        {
            this.set(x, y);
            this.is_white = is_white;
        }

        public virtual List<Move> get_all_moves(ChessBoard board) { return new List<Move>(); }

        public int threat_count(ChessBoard board)
        {
            int threats = 0;
            foreach (Move move in board.all_legal_moves)
            {
                if (move.target_piece == this)
                    threats++;
            }
            return threats;
        }

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
        public Pawn(bool is_white, int x, int y)
        {
            this.set(x, y);
            this.is_white = is_white;
            if (this.is_white == true)
                this.rep = new Bitmap(Bitmap.FromFile("../../assets/white_pawn.png"), 64, 64);
            if (this.is_white == false)
                this.rep = new Bitmap(Bitmap.FromFile("../../assets/black_pawn.png"), 64, 64);
            this.name = "Pawn";
        }

        public override List<Move> get_all_moves(ChessBoard board)
        {
            List<Move> moves = new List<Move>();
            Move potential_move;

            // for pawns, we need to differentiate between the colors
            int direction = +1;
            if (this.is_white == false) direction = -1;

            potential_move = new Move(board, this, this.x - 1, this.y + 1 * direction, Move.AttackState.PURE_ATTACK);
            if (potential_move.is_legal(board) == true)
                moves.Add(potential_move);
            potential_move = new Move(board, this, this.x + 1, this.y + 1 * direction, Move.AttackState.PURE_ATTACK);
            if (potential_move.is_legal(board) == true)
                moves.Add(potential_move);

            potential_move = new Move(board, this, this.x, this.y + 1 * direction, Move.AttackState.PURE_MOVEMENT);
            if (potential_move.is_legal(board) == false)
                return moves;
            moves.Add(potential_move);

            // pawn starter move
            if (this.has_moved == false)
            {
                potential_move = new Move(board, this, this.x, this.y + 2 * direction, Move.AttackState.PURE_MOVEMENT);
                if (potential_move.is_legal(board) == false)
                    return moves;
                moves.Add(potential_move);
            }

            return moves;
        }
    }
    public class Rook : ChessPiece
    {
        public Rook(bool is_white, int x, int y)
        {
            this.set(x, y);
            this.is_white = is_white;
            if (this.is_white == true)
                this.rep = new Bitmap(Bitmap.FromFile("../../assets/white_rook.png"), 64, 64);
            if (this.is_white == false)
                this.rep = new Bitmap(Bitmap.FromFile("../../assets/black_rook.png"), 64, 64);
            this.name = "Rook";
        }

        public override List<Move> get_all_moves(ChessBoard board)
        {
            List<Move> moves = new List<Move>();
            Move potential_move;

            // Rook Moves are so STRAIGHT FORWARD (HAaaa) that I dont need an OOB-check
            for (int x = this.x - 1; x >= 0; x--)
            {
                potential_move = new Move(board, this, x, this.y, Move.AttackState.BOTH);
                if (potential_move.is_legal(board) == false)
                    break;
                moves.Add(potential_move);
                // cant go through more than 1 enemy pieces
                if (potential_move.target_piece != null)
                    break;
            }
            for (int x = this.x + 1; x <= 7; x++)
            {
                potential_move = new Move(board, this, x, this.y, Move.AttackState.BOTH);
                if (potential_move.is_legal(board) == false)
                    break;
                moves.Add(potential_move);
                // cant go through more than 1 enemy pieces
                if (potential_move.target_piece != null)
                    break;
            }
            for (int y = this.y - 1; y >= 0; y--)
            {
                potential_move = new Move(board, this, this.x, y, Move.AttackState.BOTH);
                if (potential_move.is_legal(board) == false)
                    break;
                moves.Add(potential_move);
                // cant go through more than 1 enemy pieces
                if (potential_move.target_piece != null)
                    break;
            }
            for (int y = this.y + 1; y <= 7; y++)
            {
                potential_move = new Move(board, this, this.x, y, Move.AttackState.BOTH);
                if (potential_move.is_legal(board) == false)
                    break;
                moves.Add(potential_move);
                // cant go through more than 1 enemy pieces
                if (potential_move.target_piece != null)
                    break;
            }

            return moves;
        }
    }
    public class Knight : ChessPiece
    {
        public Knight(bool is_white, int x, int y)
        {
            this.set(x, y);
            this.is_white = is_white;
            if (this.is_white == true)
                this.rep = new Bitmap(Bitmap.FromFile("../../assets/white_knight.png"), 64, 64);
            if (this.is_white == false)
                this.rep = new Bitmap(Bitmap.FromFile("../../assets/black_knight.png"), 64, 64);
            this.name = "Knight";
        }

        public override List<Move> get_all_moves(ChessBoard board)
        {
            List<Move> moves = new List<Move>();
            Move potential_move;

            potential_move = new Move(board, this, this.x + 2, this.y + 1, Move.AttackState.BOTH);
            if (potential_move.is_legal(board) == true)
                moves.Add(potential_move);
            potential_move = new Move(board, this, this.x + 1, this.y + 2, Move.AttackState.BOTH);
            if (potential_move.is_legal(board) == true)
                moves.Add(potential_move);

            potential_move = new Move(board, this, this.x - 1, this.y + 2, Move.AttackState.BOTH);
            if (potential_move.is_legal(board) == true)
                moves.Add(potential_move);
            potential_move = new Move(board, this, this.x - 2, this.y + 1, Move.AttackState.BOTH);
            if (potential_move.is_legal(board) == true)
                moves.Add(potential_move);

            potential_move = new Move(board, this, this.x - 2, this.y - 1, Move.AttackState.BOTH);
            if (potential_move.is_legal(board) == true)
                moves.Add(potential_move);
            potential_move = new Move(board, this, this.x - 1, this.y - 2, Move.AttackState.BOTH);
            if (potential_move.is_legal(board) == true)
                moves.Add(potential_move);

            potential_move = new Move(board, this, this.x + 1, this.y - 2, Move.AttackState.BOTH);
            if (potential_move.is_legal(board) == true)
                moves.Add(potential_move);
            potential_move = new Move(board, this, this.x + 2, this.y - 1, Move.AttackState.BOTH);
            if (potential_move.is_legal(board) == true)
                moves.Add(potential_move);

            return moves;
        }
    }
    public class Bishop : ChessPiece
    {
        public Bishop(bool is_white, int x, int y)
        {
            this.set(x, y);
            this.is_white = is_white;
            if (this.is_white == true)
                this.rep = new Bitmap(Bitmap.FromFile("../../assets/white_bishop.png"), 64, 64);
            if (this.is_white == false)
                this.rep = new Bitmap(Bitmap.FromFile("../../assets/black_bishop.png"), 64, 64);
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
                potential_move = new Move(board, this, x, y, Move.AttackState.BOTH);
                if (potential_move.is_legal(board) == false)
                    break;
                moves.Add(potential_move);
                // cant go through more than 1 enemy pieces
                if (potential_move.target_piece != null)
                    break;
            }
            for (x = this.x - 1, y = this.y + 1; x >= 0 && y <= 7; x--, y++)
            {
                potential_move = new Move(board, this, x, y, Move.AttackState.BOTH);
                if (potential_move.is_legal(board) == false)
                    break;
                moves.Add(potential_move);
                // cant go through more than 1 enemy pieces
                if (potential_move.target_piece != null)
                    break;
            }
            for (x = this.x - 1, y = this.y - 1; x >= 0 && y >= 0; x--, y--)
            {
                potential_move = new Move(board, this, x, y, Move.AttackState.BOTH);
                if (potential_move.is_legal(board) == false)
                    break;
                moves.Add(potential_move);
                // cant go through more than 1 enemy pieces
                if (potential_move.target_piece != null)
                    break;
            }
            for (x = this.x + 1, y = this.y - 1; x <= 7 && y >= 0; x++, y--)
            {
                potential_move = new Move(board, this, x, y, Move.AttackState.BOTH);
                if (potential_move.is_legal(board) == false)
                    break;
                moves.Add(potential_move);
                // cant go through more than 1 enemy pieces
                if (potential_move.target_piece != null)
                    break;
            }

            return moves;
        }
    }
    public class Queen : ChessPiece
    {
        public Queen(bool is_white, int x, int y)
        {
            this.set(x, y);
            this.is_white = is_white;
            if (this.is_white == true)
                this.rep = new Bitmap(Bitmap.FromFile("../../assets/white_queen.png"), 64, 64);
            if (this.is_white == false)
                this.rep = new Bitmap(Bitmap.FromFile("../../assets/black_queen.png"), 64, 64);
            this.name = "Queen";
        }
        public override List<Move> get_all_moves(ChessBoard board)
        {
            List<Move> moves = new List<Move>();
            // Move potential_move;

            // add all Rook Moves
            ChessPiece dummy_rook = new Rook(this.is_white, this.x, this.y);
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
            ChessPiece dummy_bishop = new Bishop(this.is_white, this.x, this.y);
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
        public King(bool is_white, int x, int y)
        {
            this.set(x, y);
            this.is_white = is_white;
            if (this.is_white == true)
                this.rep = new Bitmap(Bitmap.FromFile("../../assets/white_king.png"), 64, 64);
            if (this.is_white == false)
                this.rep = new Bitmap(Bitmap.FromFile("../../assets/black_king.png"), 64, 64);
            this.name = "King";
        }
        public override List<Move> get_all_moves(ChessBoard board)
        {
            List<Move> moves = new List<Move>();
            Move potential_move;

            int mov_x = this.x;
            int mov_y = this.y;
            potential_move = new Move(board, this, ++mov_x, mov_y, Move.AttackState.BOTH);
            if (potential_move.is_legal(board) == true)
                moves.Add(potential_move);
            potential_move = new Move(board, this, mov_x, ++mov_y, Move.AttackState.BOTH);
            if (potential_move.is_legal(board) == true)
                moves.Add(potential_move);
            potential_move = new Move(board, this, --mov_x, mov_y, Move.AttackState.BOTH);
            if (potential_move.is_legal(board) == true)
                moves.Add(potential_move);
            potential_move = new Move(board, this, --mov_x, mov_y, Move.AttackState.BOTH);
            if (potential_move.is_legal(board) == true)
                moves.Add(potential_move);
            potential_move = new Move(board, this, mov_x, --mov_y, Move.AttackState.BOTH);
            if (potential_move.is_legal(board) == true)
                moves.Add(potential_move);
            potential_move = new Move(board, this, mov_x, --mov_y, Move.AttackState.BOTH);
            if (potential_move.is_legal(board) == true)
                moves.Add(potential_move);
            potential_move = new Move(board, this, ++mov_x, mov_y, Move.AttackState.BOTH);
            if (potential_move.is_legal(board) == true)
                moves.Add(potential_move);
            potential_move = new Move(board, this, ++mov_x, mov_y, Move.AttackState.BOTH);
            if (potential_move.is_legal(board) == true)
                moves.Add(potential_move);

            return moves;
        }
    }
}
