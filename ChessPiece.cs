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
        public const int KING = 12;

        // these are just for readability
        public const bool IS_WHITE = true;
        public const bool IS_BLACK = false;

        public static int count = 0;
        public bool is_white = false;
        public int move_count = 0;

        public Image rep;
        public bool alive = true;
        public bool selected = false;

        public int x;
        public int y;
        public Rectangle personal_space;

        public string name;
        public int value;

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

        // public virtual List<Move> get_all_moves(ChessBoard board, ChessPiece mover, bool consider_selfcheck) { return new List<Move>(); }

        public int threat_count(ChessBoard board)
        {
            int threats = 0;
            List<Move> enemy_legal_moves = new List<Move>();
            if (this.is_white == true) enemy_legal_moves = board.black_legal_moves;
            if (this.is_white == false) enemy_legal_moves = board.white_legal_moves;

            foreach (Move move in enemy_legal_moves)
            {
                if (move.target_piece == this)
                    threats++;
            }
            return threats;
        }

        // the threat level any piece is experiencing is based on the amount of attackers and
        // protectors that the piece has, and all their values... basically, the best-case
        // cost of the full exchange if it were to occur

        // actually, fuck me, this doesnt really work, because there are edge cases where a 2nd
        // protecting move is self-checking.. position = white{ke8, rf7, bg6, f5} black{bh5}
        // only either whites rook OR their bishop can protect the pawn on f5, not both.....

        // still better than the previous version
        public double threat_level(ChessBoard board)
        {
            List<Move> self_legal_moves = new List<Move>();
            List<Move> enemy_legal_moves = new List<Move>();

            List<int> threats = new List<int>();
            List<int> protectors = new List<int>();

            if (this.is_white == true)
            {
                self_legal_moves = board.white_protect_moves;
                enemy_legal_moves = board.black_legal_moves;
            }
            if (this.is_white == false)
            {
                self_legal_moves = board.black_protect_moves;
                enemy_legal_moves = board.white_legal_moves;
            }

            // How many threats are there ?
            foreach (Move move in enemy_legal_moves)
            {
                if (move.target_piece == this)
                    threats.Add(move.moving_piece.value);
            }
            if (threats.Count == 0) return 0;

            // How many protectors are there ?
            foreach (Move move in self_legal_moves)
            {
                if (board.occupation[move.target_x, move.target_y] == this)
                    protectors.Add(move.moving_piece.value);
            }

            // lesser pieces should take first, I will unroll these from the back
            threats.Sort();
            threats.Reverse();
            protectors.Sort();
            protectors.Reverse();

            int threat_level = this.value;
            while (protectors.Count > 0)
            {
                // we can definetly take this threat if it took us
                threat_level -= threats.Last();
                threats.RemoveAt(threats.Count - 1);

                // if they have another threat, we lose our protector
                if (threats.Count > 0)
                {
                    threat_level += protectors.Last();
                    protectors.RemoveAt(protectors.Count - 1);
                }
                else break;
            }

            // worst case scenario is actually that I just leave my piece hanging
            if (threat_level > this.value)
                return this.value;

            // I cannot "gain" anything from losing pieces, because
            // Im assuming my enemy can see that they would lose the exchange
            if (threat_level < 0)
                return 0;

            return (double) threat_level;
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
            this.value = 1;
            this.set(x, y);
            this.is_white = is_white;
            if (this.is_white == true)
                this.rep = new Bitmap(Bitmap.FromFile("../../assets/white_pawn.png"), 64, 64);
            if (this.is_white == false)
                this.rep = new Bitmap(Bitmap.FromFile("../../assets/black_pawn.png"), 64, 64);
            this.name = (this.is_white ? "White " : "Black ") + "Pawn";
        }

        public static List<Move> get_all_moves(ChessBoard board, ChessPiece mover, bool consider_selfcheck)
        {
            List<Move> moves = new List<Move>();
            Move potential_move;

            // for pawns, we need to differentiate between the colors
            int direction = +1;
            if (mover.is_white == false) direction = -1;

            potential_move = new Move(board, mover, mover.x - 1, mover.y + 1 * direction, Move.AttackState.PURE_ATTACK);
            if (potential_move.is_legal(board, consider_selfcheck) == true)
                moves.Add(potential_move);
            potential_move = new Move(board, mover, mover.x + 1, mover.y + 1 * direction, Move.AttackState.PURE_ATTACK);
            if (potential_move.is_legal(board, consider_selfcheck) == true)
                moves.Add(potential_move);

            potential_move = new Move(board, mover, mover.x, mover.y + 1 * direction, Move.AttackState.PURE_MOVEMENT);
            if (potential_move.is_legal(board, consider_selfcheck) == false)
                return moves;
            moves.Add(potential_move);

            // pawn starter move
            if (mover.move_count == 0)
            {
                // if the 1-step move collides with ANYthing, we return
                if (board.occupation[potential_move.target_x, potential_move.target_y] != null) return moves;

                potential_move = new Move(board, mover, mover.x, mover.y + 2 * direction, Move.AttackState.PURE_MOVEMENT);
                if (potential_move.is_legal(board, consider_selfcheck) == false)
                    return moves;
                moves.Add(potential_move);
            }

            return moves;
        }
    }
    public class Knight : ChessPiece
    {
        public Knight(bool is_white, int x, int y)
        {
            this.value = 3;
            this.set(x, y);
            this.is_white = is_white;
            if (this.is_white == true)
                this.rep = new Bitmap(Bitmap.FromFile("../../assets/white_knight.png"), 64, 64);
            if (this.is_white == false)
                this.rep = new Bitmap(Bitmap.FromFile("../../assets/black_knight.png"), 64, 64);
            this.name = (this.is_white ? "White " : "Black ") + "Knight";
        }

        public static List<Move> get_all_moves(ChessBoard board, ChessPiece mover, bool consider_selfcheck)
        {
            List<Move> moves = new List<Move>();
            Move potential_move;

            potential_move = new Move(board, mover, mover.x + 2, mover.y + 1, Move.AttackState.BOTH);
            if (potential_move.is_legal(board, consider_selfcheck) == true)
                moves.Add(potential_move);
            potential_move = new Move(board, mover, mover.x + 1, mover.y + 2, Move.AttackState.BOTH);
            if (potential_move.is_legal(board, consider_selfcheck) == true)
                moves.Add(potential_move);

            potential_move = new Move(board, mover, mover.x - 1, mover.y + 2, Move.AttackState.BOTH);
            if (potential_move.is_legal(board, consider_selfcheck) == true)
                moves.Add(potential_move);
            potential_move = new Move(board, mover, mover.x - 2, mover.y + 1, Move.AttackState.BOTH);
            if (potential_move.is_legal(board, consider_selfcheck) == true)
                moves.Add(potential_move);

            potential_move = new Move(board, mover, mover.x - 2, mover.y - 1, Move.AttackState.BOTH);
            if (potential_move.is_legal(board, consider_selfcheck) == true)
                moves.Add(potential_move);
            potential_move = new Move(board, mover, mover.x - 1, mover.y - 2, Move.AttackState.BOTH);
            if (potential_move.is_legal(board, consider_selfcheck) == true)
                moves.Add(potential_move);

            potential_move = new Move(board, mover, mover.x + 1, mover.y - 2, Move.AttackState.BOTH);
            if (potential_move.is_legal(board, consider_selfcheck) == true)
                moves.Add(potential_move);
            potential_move = new Move(board, mover, mover.x + 2, mover.y - 1, Move.AttackState.BOTH);
            if (potential_move.is_legal(board, consider_selfcheck) == true)
                moves.Add(potential_move);

            return moves;
        }
    }
    public class Rook : ChessPiece
    {
        public Rook(bool is_white, int x, int y)
        {
            this.value = 5;
            this.set(x, y);
            this.is_white = is_white;
            if (this.is_white == true)
                this.rep = new Bitmap(Bitmap.FromFile("../../assets/white_rook.png"), 64, 64);
            if (this.is_white == false)
                this.rep = new Bitmap(Bitmap.FromFile("../../assets/black_rook.png"), 64, 64);
            this.name = (this.is_white ? "White " : "Black ") + "Rook";
        }

        public static List<Move> get_all_moves(ChessBoard board, ChessPiece mover, bool consider_selfcheck)
        {
            List<Move> moves = new List<Move>();
            Move potential_move;

            // Rook Moves are so STRAIGHT FORWARD (HAaaa) that I dont need an OOB-check
            for (int x = mover.x - 1; x >= 0; x--)
            {
                potential_move = new Move(board, mover, x, mover.y, Move.AttackState.BOTH);
                if (potential_move.is_legal(board, consider_selfcheck) == true)
                    moves.Add(potential_move);
                // cant go through more than 1 enemy pieces
                if (potential_move.target_piece != null)
                    break;
            }
            for (int x = mover.x + 1; x <= 7; x++)
            {
                potential_move = new Move(board, mover, x, mover.y, Move.AttackState.BOTH);
                if (potential_move.is_legal(board, consider_selfcheck) == true)
                    moves.Add(potential_move);
                // cant go through more than 1 enemy pieces
                if (potential_move.target_piece != null)
                    break;
            }
            for (int y = mover.y - 1; y >= 0; y--)
            {
                potential_move = new Move(board, mover, mover.x, y, Move.AttackState.BOTH);
                if (potential_move.is_legal(board, consider_selfcheck) == true)
                    moves.Add(potential_move);
                // cant go through more than 1 enemy pieces
                if (potential_move.target_piece != null)
                    break;
            }
            for (int y = mover.y + 1; y <= 7; y++)
            {
                potential_move = new Move(board, mover, mover.x, y, Move.AttackState.BOTH);
                if (potential_move.is_legal(board, consider_selfcheck) == true)
                    moves.Add(potential_move);
                // cant go through more than 1 enemy pieces
                if (potential_move.target_piece != null)
                    break;
            }

            return moves;
        }
    }
    
    public class Bishop : ChessPiece
    {
        public Bishop(bool is_white, int x, int y)
        {
            this.value = 3;
            this.set(x, y);
            this.is_white = is_white;
            if (this.is_white == true)
                this.rep = new Bitmap(Bitmap.FromFile("../../assets/white_bishop.png"), 64, 64);
            if (this.is_white == false)
                this.rep = new Bitmap(Bitmap.FromFile("../../assets/black_bishop.png"), 64, 64);
            this.name = (this.is_white ? "White " : "Black ") + "Bishop";
        }

        public static List<Move> get_all_moves(ChessBoard board, ChessPiece mover, bool consider_selfcheck)
        {
            List<Move> moves = new List<Move>();
            Move potential_move;

            int x, y;

            // Bishop Moves are so straight forward that I dont need an OOB-check
            for (x = mover.x + 1, y = mover.y + 1; x <= 7 && y <= 7; x++, y++)
            {
                potential_move = new Move(board, mover, x, y, Move.AttackState.BOTH);
                if (potential_move.is_legal(board, consider_selfcheck) == true)
                    moves.Add(potential_move);
                // cant go through more than 1 enemy pieces
                if (potential_move.target_piece != null)
                    break;
            }
            for (x = mover.x - 1, y = mover.y + 1; x >= 0 && y <= 7; x--, y++)
            {
                potential_move = new Move(board, mover, x, y, Move.AttackState.BOTH);
                if (potential_move.is_legal(board, consider_selfcheck) == true)
                    moves.Add(potential_move);
                // cant go through more than 1 enemy pieces
                if (potential_move.target_piece != null)
                    break;
            }
            for (x = mover.x - 1, y = mover.y - 1; x >= 0 && y >= 0; x--, y--)
            {
                potential_move = new Move(board, mover, x, y, Move.AttackState.BOTH);
                if (potential_move.is_legal(board, consider_selfcheck) == true)
                    moves.Add(potential_move);
                // cant go through more than 1 enemy pieces
                if (potential_move.target_piece != null)
                    break;
            }
            for (x = mover.x + 1, y = mover.y - 1; x <= 7 && y >= 0; x++, y--)
            {
                potential_move = new Move(board, mover, x, y, Move.AttackState.BOTH);
                if (potential_move.is_legal(board, consider_selfcheck) == true)
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
            this.value = 8;
            this.set(x, y);
            this.is_white = is_white;
            if (this.is_white == true)
                this.rep = new Bitmap(Bitmap.FromFile("../../assets/white_queen.png"), 64, 64);
            if (this.is_white == false)
                this.rep = new Bitmap(Bitmap.FromFile("../../assets/black_queen.png"), 64, 64);
            this.name = (this.is_white ? "White " : "Black ") + "Queen";
        }
        public static List<Move> get_all_moves(ChessBoard board, ChessPiece mover, bool consider_selfcheck)
        {
            List<Move> moves = new List<Move>();
            Move potential_move;

            int x, y;

            // I tried doing these by creating dummy Rook and a dummy Bishop, but that messes
            // with the board, because I simulate the board state for check-checks...

            // Rook Moves are so STRAIGHT FORWARD (HAaaa) that I dont need an OOB-check
            for (x = mover.x - 1; x >= 0; x--)
            {
                potential_move = new Move(board, mover, x, mover.y, Move.AttackState.BOTH);
                if (potential_move.is_legal(board, consider_selfcheck) == true)
                    moves.Add(potential_move);
                // cant go through more than 1 enemy pieces
                if (potential_move.target_piece != null)
                    break;
            }
            for (x = mover.x + 1; x <= 7; x++)
            {
                potential_move = new Move(board, mover, x, mover.y, Move.AttackState.BOTH);
                if (potential_move.is_legal(board, consider_selfcheck) == true)
                    moves.Add(potential_move);
                // cant go through more than 1 enemy pieces
                if (potential_move.target_piece != null)
                    break;
            }
            for (y = mover.y - 1; y >= 0; y--)
            {
                potential_move = new Move(board, mover, mover.x, y, Move.AttackState.BOTH);
                if (potential_move.is_legal(board, consider_selfcheck) == true)
                    moves.Add(potential_move);
                // cant go through more than 1 enemy pieces
                if (potential_move.target_piece != null)
                    break;
            }
            for (y = mover.y + 1; y <= 7; y++)
            {
                potential_move = new Move(board, mover, mover.x, y, Move.AttackState.BOTH);
                if (potential_move.is_legal(board, consider_selfcheck) == true)
                    moves.Add(potential_move);
                // cant go through more than 1 enemy pieces
                if (potential_move.target_piece != null)
                    break;
            }
            // Bishop Moves are so straight forward that I dont need an OOB-check
            for (x = mover.x + 1, y = mover.y + 1; x <= 7 && y <= 7; x++, y++)
            {
                potential_move = new Move(board, mover, x, y, Move.AttackState.BOTH);
                if (potential_move.is_legal(board, consider_selfcheck) == true)
                    moves.Add(potential_move);
                // cant go through more than 1 enemy pieces
                if (potential_move.target_piece != null)
                    break;
            }
            for (x = mover.x - 1, y = mover.y + 1; x >= 0 && y <= 7; x--, y++)
            {
                potential_move = new Move(board, mover, x, y, Move.AttackState.BOTH);
                if (potential_move.is_legal(board, consider_selfcheck) == true)
                    moves.Add(potential_move);
                // cant go through more than 1 enemy pieces
                if (potential_move.target_piece != null)
                    break;
            }
            for (x = mover.x - 1, y = mover.y - 1; x >= 0 && y >= 0; x--, y--)
            {
                potential_move = new Move(board, mover, x, y, Move.AttackState.BOTH);
                if (potential_move.is_legal(board, consider_selfcheck) == true)
                    moves.Add(potential_move);
                // cant go through more than 1 enemy pieces
                if (potential_move.target_piece != null)
                    break;
            }
            for (x = mover.x + 1, y = mover.y - 1; x <= 7 && y >= 0; x++, y--)
            {
                potential_move = new Move(board, mover, x, y, Move.AttackState.BOTH);
                if (potential_move.is_legal(board, consider_selfcheck) == true)
                    moves.Add(potential_move);
                // cant go through more than 1 enemy pieces
                if (potential_move.target_piece != null)
                    break;
            }

            return moves;
        }
    }
    public class King : ChessPiece
    {
        public King(bool is_white, int x, int y)
        {
            this.value = 12;
            this.set(x, y);
            this.is_white = is_white;
            if (this.is_white == true)
                this.rep = new Bitmap(Bitmap.FromFile("../../assets/white_king.png"), 64, 64);
            if (this.is_white == false)
                this.rep = new Bitmap(Bitmap.FromFile("../../assets/black_king.png"), 64, 64);
            this.name = (this.is_white ? "White " : "Black ") + "King";
        }
        public static List<Move> get_all_moves(ChessBoard board, ChessPiece mover, bool consider_selfcheck)
        {
            List<Move> moves = new List<Move>();
            Move potential_move;

            int mov_x = mover.x;
            int mov_y = mover.y;
            potential_move = new Move(board, mover, ++mov_x, mov_y, Move.AttackState.BOTH);
            if (potential_move.is_legal(board, consider_selfcheck) == true)
                moves.Add(potential_move);
            potential_move = new Move(board, mover, mov_x, ++mov_y, Move.AttackState.BOTH);
            if (potential_move.is_legal(board, consider_selfcheck) == true)
                moves.Add(potential_move);
            potential_move = new Move(board, mover, --mov_x, mov_y, Move.AttackState.BOTH);
            if (potential_move.is_legal(board, consider_selfcheck) == true)
                moves.Add(potential_move);
            potential_move = new Move(board, mover, --mov_x, mov_y, Move.AttackState.BOTH);
            if (potential_move.is_legal(board, consider_selfcheck) == true)
                moves.Add(potential_move);
            potential_move = new Move(board, mover, mov_x, --mov_y, Move.AttackState.BOTH);
            if (potential_move.is_legal(board, consider_selfcheck) == true)
                moves.Add(potential_move);
            potential_move = new Move(board, mover, mov_x, --mov_y, Move.AttackState.BOTH);
            if (potential_move.is_legal(board, consider_selfcheck) == true)
                moves.Add(potential_move);
            potential_move = new Move(board, mover, ++mov_x, mov_y, Move.AttackState.BOTH);
            if (potential_move.is_legal(board, consider_selfcheck) == true)
                moves.Add(potential_move);
            potential_move = new Move(board, mover, ++mov_x, mov_y, Move.AttackState.BOTH);
            if (potential_move.is_legal(board, consider_selfcheck) == true)
                moves.Add(potential_move);

            return moves;
        }
    }
}
