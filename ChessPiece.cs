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

        // piece ID is a bitfield that tells the piece type
        // (faster than string)
        public const int PAWN_ID    = 0b00000001;
        public const int ROOK_ID    = 0b00000010;
        public const int KNIGHT_ID  = 0b00000100;
        public const int BISHOP_ID  = 0b00001000;
        public const int KING_ID    = 0b00010000;
        public const int QUEEN_ID   = 0b00100000;
        public const int EN_PASS_ID = 0b01000000;
        public int ID; // what type of piece am I ?
        public int UID; // whats my number on the board ?

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


        public override string ToString()
        {
            string description = string.Format("{0} on ({1}|{2}):\n", this.name, this.x, this.y);
            description += string.Format("Alive: {0}\n", this.alive);
            return description;
        }
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
            // ChessPiece.count++; Console.WriteLine("Piece created. Count: {0}", count);
        }
        public void Dispose()
        {
            this.alive = false;
            //Dispose(true);
            //System.GC.SuppressFinalize(this);

            // ChessPiece.count--;
            Console.WriteLine("{0} destroyed. Count: {1}", this.name, count);
        }
        public ChessPiece(bool is_white, int x, int y, int UID)
        {
            this.set(x, y);
            this.is_white = is_white;
        }

        public virtual List<Move> get_all_moves(ChessBoard board) { return new List<Move>(); }

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

        public bool is_checked(ChessBoard board)
        {
            // placeholder
            ChessPiece piece = null;

            // first, we check ROOK, QUEEN and KING threats from up down left right
            for (int y = this.y + 1; y < 8; y++)
            {
                piece = board.occupation[this.x, y];
                if (piece == null) continue;
                if (piece.alive == false) continue;
                if (piece.is_white == this.is_white) break;

                // first step, check for enemy king
                if (y == this.y + 1)
                {
                    if ((piece.ID & ChessPiece.KING_ID) > 0)
                        return true;
                }
                if ((piece.ID & (ChessPiece.ROOK_ID | ChessPiece.QUEEN_ID)) > 0)
                    return true;

                // if we get here, we have found an opponent piece that does not
                // threaten us, but blocks any threats comming from this direction
                // so we can stop looking here
                break;
            }
            for (int y = this.y - 1; y >= 0; y--)
            {
                piece = board.occupation[this.x, y];
                if (piece == null) continue;
                if (piece.alive == false) continue;
                if (piece.is_white == this.is_white) break;

                if (y == this.y - 1)
                {
                    if ((piece.ID & ChessPiece.KING_ID) > 0)
                        return true;
                }
                if ((piece.ID & (ChessPiece.ROOK_ID | ChessPiece.QUEEN_ID)) > 0)
                    return true;

                break;
            }
            for (int x = this.x - 1; x >= 0; x--)
            {
                piece = board.occupation[x, this.y];
                if (piece == null) continue;
                if (piece.alive == false) continue;
                if (piece.is_white == this.is_white) break;

                if (x == this.x - 1)
                {
                    if ((piece.ID & ChessPiece.KING_ID) > 0)
                        return true;
                }
                if ((piece.ID & (ChessPiece.ROOK_ID | ChessPiece.QUEEN_ID)) > 0)
                    return true;

                break;
            }
            for (int x = this.x + 1; x < 8; x++)
            {
                piece = board.occupation[x, this.y];
                if (piece == null) continue;
                if (piece.alive == false) continue;
                if (piece.is_white == this.is_white) break;

                if (x == this.x + 1)
                {
                    if ((piece.ID & ChessPiece.KING_ID) > 0)
                        return true;
                }
                if ((piece.ID & (ChessPiece.ROOK_ID | ChessPiece.QUEEN_ID)) > 0)
                    return true;

                break;
            }

            // now the diagonals, checking QUEEN, BISHOP, KING and PAWN
            for (int x = this.x + 1, y = this.y + 1; (x < 8 && y < 8); x++, y++)
            {
                piece = board.occupation[x, y];
                if (piece == null) continue;
                if (piece.alive == false) continue;
                if (piece.is_white == this.is_white) break;

                // first step, check for enemy king (1 coord is enough)
                if (x == this.x + 1)
                {
                    if ((piece.ID & ChessPiece.KING_ID) > 0)
                        return true;

                    // extra pawn check (only for white king)
                    if (this.is_white == true)
                        if ((piece.ID & ChessPiece.PAWN_ID) > 0)
                            return true;
                }
                if ((piece.ID & (ChessPiece.BISHOP_ID | ChessPiece.QUEEN_ID)) > 0)
                    return true;

                // if we get here, we have found an opponent piece that does not
                // threaten us, but blocks any threats comming from this direction
                // so we can stop looking here
                break;
            }
            for (int x = this.x - 1, y = this.y + 1; (x >= 0 && y < 8); x--, y++)
            {
                piece = board.occupation[x, y];
                if (piece == null) continue;
                if (piece.alive == false) continue;
                if (piece.is_white == this.is_white) break;

                if (x == this.x - 1)
                {
                    if ((piece.ID & ChessPiece.KING_ID) > 0)
                        return true;

                    // extra pawn check (only for white king)
                    if (this.is_white == true)
                        if ((piece.ID & ChessPiece.PAWN_ID) > 0)
                            return true;
                }
                if ((piece.ID & (ChessPiece.BISHOP_ID | ChessPiece.QUEEN_ID)) > 0)
                    return true;

                break;
            }
            for (int x = this.x - 1, y = this.y - 1; (x >= 0 && y >= 0); x--, y--)
            {
                piece = board.occupation[x, y];
                if (piece == null) continue;
                if (piece.alive == false) continue;
                if (piece.is_white == this.is_white) break;

                if (x == this.x - 1)
                {
                    if ((piece.ID & ChessPiece.KING_ID) > 0)
                        return true;

                    // extra pawn check (only for black king)
                    if (this.is_white == false)
                        if ((piece.ID & ChessPiece.PAWN_ID) > 0)
                            return true;
                }
                if ((piece.ID & (ChessPiece.BISHOP_ID | ChessPiece.QUEEN_ID)) > 0)
                    return true;

                break;
            }
            for (int x = this.x + 1, y = this.y - 1; (x < 8 && y >= 0); x++, y--)
            {
                piece = board.occupation[x, y];
                if (piece == null) continue;
                if (piece.alive == false) continue;
                if (piece.is_white == this.is_white) break;

                if (x == this.x + 1)
                {
                    if ((piece.ID & ChessPiece.KING_ID) > 0)
                        return true;

                    // extra pawn check (only for black king)
                    if (this.is_white == false)
                        if ((piece.ID & ChessPiece.PAWN_ID) > 0)
                            return true;
                }
                if ((piece.ID & (ChessPiece.BISHOP_ID | ChessPiece.QUEEN_ID)) > 0)
                    return true;

                break;
            }

            // and KNIGHT checks
            piece = board.get_occupation(this.x + 2, this.y + 1);
            if (piece != null && piece.alive == true && piece.is_white != this.is_white && (piece.ID & ChessPiece.KNIGHT_ID) > 0)
                return true;
            piece = board.get_occupation(this.x + 1, this.y + 2);
            if (piece != null && piece.alive == true && piece.is_white != this.is_white && (piece.ID & ChessPiece.KNIGHT_ID) > 0)
                return true;
            piece = board.get_occupation(this.x - 1, this.y + 2);
            if (piece != null && piece.alive == true && piece.is_white != this.is_white && (piece.ID & ChessPiece.KNIGHT_ID) > 0)
                return true;
            piece = board.get_occupation(this.x - 2, this.y + 1);
            if (piece != null && piece.alive == true && piece.is_white != this.is_white && (piece.ID & ChessPiece.KNIGHT_ID) > 0)
                return true;

            piece = board.get_occupation(this.x + 2, this.y - 1);
            if (piece != null && piece.alive == true && piece.is_white != this.is_white && (piece.ID & ChessPiece.KNIGHT_ID) > 0)
                return true;
            piece = board.get_occupation(this.x + 1, this.y - 2);
            if (piece != null && piece.alive == true && piece.is_white != this.is_white && (piece.ID & ChessPiece.KNIGHT_ID) > 0)
                return true;
            piece = board.get_occupation(this.x - 1, this.y - 2);
            if (piece != null && piece.alive == true && piece.is_white != this.is_white && (piece.ID & ChessPiece.KNIGHT_ID) > 0)
                return true;
            piece = board.get_occupation(this.x - 2, this.y - 1);
            if (piece != null && piece.alive == true && piece.is_white != this.is_white && (piece.ID & ChessPiece.KNIGHT_ID) > 0)
                return true;

            // no threats detected !
            return false;
        }
    }

    public class EnPassantTarget : ChessPiece
    {
        public EnPassantTarget(bool is_white, int x, int y, int UID)
        {
            this.value = 0;
            this.ID = ChessPiece.EN_PASS_ID;
            this.UID = UID;
            this.set(x, y);
            this.is_white = is_white;
            this.name = (this.is_white ? "White " : "Black ") + "EnPassant";

            this.rep = new Bitmap(Bitmap.FromFile("../../assets/en_passant.png"), 64, 64);
        }
    }

    public class Pawn : ChessPiece
    {
        public Pawn(bool is_white, int x, int y, int UID)
        {
            this.value = 1;
            this.ID = ChessPiece.PAWN_ID;
            this.UID = UID;
            this.set(x, y);
            this.is_white = is_white;
            if (this.is_white == true)
                this.rep = new Bitmap(Bitmap.FromFile("../../assets/white_pawn.png"), 64, 64);
            if (this.is_white == false)
                this.rep = new Bitmap(Bitmap.FromFile("../../assets/black_pawn.png"), 64, 64);
            this.name = (this.is_white ? "White " : "Black ") + "Pawn";
        }
        // for cloning purposes
        public Pawn(Pawn piece)
        {
            this.value = piece.value;
            this.ID = piece.ID;
            this.UID = piece.UID;
            this.set(piece.x, piece.y);
            this.is_white = piece.is_white;
            this.rep = piece.rep;
            this.name = piece.name;

            this.alive = piece.alive;
            this.move_count = piece.move_count;
            this.selected = piece.selected; // neccessary ?
        }
        public override List<Move> get_all_moves(ChessBoard board)
        {
            List<Move> moves = new List<Move>();
            Move potential_move;

            // for pawns, we need to differentiate between the colors
            int direction = +1;
            if (this.is_white == false) direction = -1;

            // first, check the 2 attacks
            potential_move = new Move(board, this, this.x - 1, this.y + 1 * direction, Move.AttackState.PURE_ATTACK);
            if (potential_move.is_legal(board) == true)
                moves.Add(potential_move);
            potential_move = new Move(board, this, this.x + 1, this.y + 1 * direction, Move.AttackState.PURE_ATTACK);
            if (potential_move.is_legal(board) == true)
                moves.Add(potential_move);

            // then the 1-forward move
            potential_move = new Move(board, this, this.x, this.y + 1 * direction, Move.AttackState.PURE_MOVEMENT);
            if (potential_move.is_legal(board) == true)
                moves.Add(potential_move);

            // pawn starter move
            if (this.move_count == 0)
            {
                // if the 1-step move collides with ANYthing, we return
                if (board.occupation[potential_move.target_x, potential_move.target_y] != null) return moves;

                potential_move = new Move(board, this, this.x, this.y + 2 * direction, Move.AttackState.PURE_MOVEMENT);
                if (potential_move.is_legal(board) == true)
                    moves.Add(potential_move);
            }
            // done
            return moves;
        }
    }
    public class Knight : ChessPiece
    {
        public Knight(bool is_white, int x, int y, int UID)
        {
            this.value = 3;
            this.ID = ChessPiece.KNIGHT_ID;
            this.UID = UID;
            this.set(x, y);
            this.is_white = is_white;
            if (this.is_white == true)
                this.rep = new Bitmap(Bitmap.FromFile("../../assets/white_knight.png"), 64, 64);
            if (this.is_white == false)
                this.rep = new Bitmap(Bitmap.FromFile("../../assets/black_knight.png"), 64, 64);
            this.name = (this.is_white ? "White " : "Black ") + "Knight";
        }
        // for cloning purposes
        public Knight(Knight piece)
        {
            this.value = piece.value;
            this.ID = piece.ID;
            this.UID = piece.UID;
            this.set(piece.x, piece.y);
            this.is_white = piece.is_white;
            this.rep = piece.rep;
            this.name = piece.name;

            this.alive = piece.alive;
            this.move_count = piece.move_count;
            this.selected = piece.selected; // neccessary ?
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

            potential_move = new Move(board, this, this.x + 2, this.y - 1, Move.AttackState.BOTH);
            if (potential_move.is_legal(board) == true)
                moves.Add(potential_move);
            potential_move = new Move(board, this, this.x + 1, this.y - 2, Move.AttackState.BOTH);
            if (potential_move.is_legal(board) == true)
                moves.Add(potential_move);
            potential_move = new Move(board, this, this.x - 1, this.y - 2, Move.AttackState.BOTH);
            if (potential_move.is_legal(board) == true)
                moves.Add(potential_move);
            potential_move = new Move(board, this, this.x - 2, this.y - 1, Move.AttackState.BOTH);
            if (potential_move.is_legal(board) == true)
                moves.Add(potential_move);

            return moves;
        }
    }
    public class Rook : ChessPiece
    {
        public Rook(bool is_white, int x, int y, int UID)
        {
            this.value = 5;
            this.ID = ChessPiece.ROOK_ID;
            this.UID = UID;
            this.set(x, y);
            this.is_white = is_white;
            if (this.is_white == true)
                this.rep = new Bitmap(Bitmap.FromFile("../../assets/white_rook.png"), 64, 64);
            if (this.is_white == false)
                this.rep = new Bitmap(Bitmap.FromFile("../../assets/black_rook.png"), 64, 64);
            this.name = (this.is_white ? "White " : "Black ") + "Rook";
        }
        // for cloning purposes
        public Rook(Rook piece)
        {
            this.value = piece.value;
            this.ID = piece.ID;
            this.UID = piece.UID;
            this.set(piece.x, piece.y);
            this.is_white = piece.is_white;
            this.rep = piece.rep;
            this.name = piece.name;

            this.alive = piece.alive;
            this.move_count = piece.move_count;
            this.selected = piece.selected; // neccessary ?
        }

        public override List<Move> get_all_moves(ChessBoard board)
        {
            List<Move> moves = new List<Move>();
            Move potential_move;

            // Rook Moves are so STRAIGHT FORWARD (HAaaa) that I dont need an OOB-check
            for (int x = this.x - 1; x >= 0; x--)
            {
                potential_move = new Move(board, this, x, this.y, Move.AttackState.BOTH);
                if (potential_move.is_legal(board) == true)
                    moves.Add(potential_move);
                // cant go through more than 1 enemy pieces
                if (potential_move.target_piece != null)
                    break;
            }
            for (int x = this.x + 1; x <= 7; x++)
            {
                potential_move = new Move(board, this, x, this.y, Move.AttackState.BOTH);
                if (potential_move.is_legal(board) == true)
                    moves.Add(potential_move);
                // cant go through more than 1 enemy pieces
                if (potential_move.target_piece != null)
                    break;
            }
            for (int y = this.y - 1; y >= 0; y--)
            {
                potential_move = new Move(board, this, this.x, y, Move.AttackState.BOTH);
                if (potential_move.is_legal(board) == true)
                    moves.Add(potential_move);
                // cant go through more than 1 enemy pieces
                if (potential_move.target_piece != null)
                    break;
            }
            for (int y = this.y + 1; y <= 7; y++)
            {
                potential_move = new Move(board, this, this.x, y, Move.AttackState.BOTH);
                if (potential_move.is_legal(board) == true)
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
        public Bishop(bool is_white, int x, int y, int UID)
        {
            this.value = 3;
            this.ID = ChessPiece.BISHOP_ID;
            this.UID = UID;
            this.set(x, y);
            this.is_white = is_white;
            if (this.is_white == true)
                this.rep = new Bitmap(Bitmap.FromFile("../../assets/white_bishop.png"), 64, 64);
            if (this.is_white == false)
                this.rep = new Bitmap(Bitmap.FromFile("../../assets/black_bishop.png"), 64, 64);
            this.name = (this.is_white ? "White " : "Black ") + "Bishop";
        }
        // for cloning purposes
        public Bishop(Bishop piece)
        {
            this.value = piece.value;
            this.ID = piece.ID;
            this.UID = piece.UID;
            this.set(piece.x, piece.y);
            this.is_white = piece.is_white;
            this.rep = piece.rep;
            this.name = piece.name;

            this.alive = piece.alive;
            this.move_count = piece.move_count;
            this.selected = piece.selected; // neccessary ?
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
                if (potential_move.is_legal(board) == true)
                    moves.Add(potential_move);
                // cant go through more than 1 enemy pieces
                if (potential_move.target_piece != null)
                    break;
            }
            for (x = this.x - 1, y = this.y + 1; x >= 0 && y <= 7; x--, y++)
            {
                potential_move = new Move(board, this, x, y, Move.AttackState.BOTH);
                if (potential_move.is_legal(board) == true)
                    moves.Add(potential_move);
                // cant go through more than 1 enemy pieces
                if (potential_move.target_piece != null)
                    break;
            }
            for (x = this.x - 1, y = this.y - 1; x >= 0 && y >= 0; x--, y--)
            {
                potential_move = new Move(board, this, x, y, Move.AttackState.BOTH);
                if (potential_move.is_legal(board) == true)
                    moves.Add(potential_move);
                // cant go through more than 1 enemy pieces
                if (potential_move.target_piece != null)
                    break;
            }
            for (x = this.x + 1, y = this.y - 1; x <= 7 && y >= 0; x++, y--)
            {
                potential_move = new Move(board, this, x, y, Move.AttackState.BOTH);
                if (potential_move.is_legal(board) == true)
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
        public Queen(bool is_white, int x, int y, int UID)
        {
            this.value = 9;
            this.ID = ChessPiece.QUEEN_ID;
            this.UID = UID;
            this.set(x, y);
            this.is_white = is_white;
            if (this.is_white == true)
                this.rep = new Bitmap(Bitmap.FromFile("../../assets/white_queen.png"), 64, 64);
            if (this.is_white == false)
                this.rep = new Bitmap(Bitmap.FromFile("../../assets/black_queen.png"), 64, 64);
            this.name = (this.is_white ? "White " : "Black ") + "Queen";
        }
        // for cloning purposes
        public Queen(Queen piece)
        {
            this.value = piece.value;
            this.ID = piece.ID;
            this.UID = piece.UID;
            this.set(piece.x, piece.y);
            this.is_white = piece.is_white;
            this.rep = piece.rep;
            this.name = piece.name;

            this.alive = piece.alive;
            this.move_count = piece.move_count;
            this.selected = piece.selected; // neccessary ?
        }
        public override List<Move> get_all_moves(ChessBoard board)
        {
            List<Move> moves = new List<Move>();
            Move potential_move;

            int x, y;

            // I tried doing these by creating dummy Rook and a dummy Bishop, but that messes
            // with the board, because I simulate the board state for check-checks...

            // Rook Moves are so STRAIGHT FORWARD (HAaaa) that I dont need an OOB-check
            for (x = this.x - 1; x >= 0; x--)
            {
                potential_move = new Move(board, this, x, this.y, Move.AttackState.BOTH);
                if (potential_move.is_legal(board) == true)
                    moves.Add(potential_move);
                // cant go through more than 1 enemy pieces
                if (potential_move.target_piece != null)
                    break;
            }
            for (x = this.x + 1; x <= 7; x++)
            {
                potential_move = new Move(board, this, x, this.y, Move.AttackState.BOTH);
                if (potential_move.is_legal(board) == true)
                    moves.Add(potential_move);
                // cant go through more than 1 enemy pieces
                if (potential_move.target_piece != null)
                    break;
            }
            for (y = this.y - 1; y >= 0; y--)
            {
                potential_move = new Move(board, this, this.x, y, Move.AttackState.BOTH);
                if (potential_move.is_legal(board) == true)
                    moves.Add(potential_move);
                // cant go through more than 1 enemy pieces
                if (potential_move.target_piece != null)
                    break;
            }
            for (y = this.y + 1; y <= 7; y++)
            {
                potential_move = new Move(board, this, this.x, y, Move.AttackState.BOTH);
                if (potential_move.is_legal(board) == true)
                    moves.Add(potential_move);
                // cant go through more than 1 enemy pieces
                if (potential_move.target_piece != null)
                    break;
            }
            // Bishop Moves are so straight forward that I dont need an OOB-check
            for (x = this.x + 1, y = this.y + 1; x <= 7 && y <= 7; x++, y++)
            {
                potential_move = new Move(board, this, x, y, Move.AttackState.BOTH);
                if (potential_move.is_legal(board) == true)
                    moves.Add(potential_move);
                // cant go through more than 1 enemy pieces
                if (potential_move.target_piece != null)
                    break;
            }
            for (x = this.x - 1, y = this.y + 1; x >= 0 && y <= 7; x--, y++)
            {
                potential_move = new Move(board, this, x, y, Move.AttackState.BOTH);
                if (potential_move.is_legal(board) == true)
                    moves.Add(potential_move);
                // cant go through more than 1 enemy pieces
                if (potential_move.target_piece != null)
                    break;
            }
            for (x = this.x - 1, y = this.y - 1; x >= 0 && y >= 0; x--, y--)
            {
                potential_move = new Move(board, this, x, y, Move.AttackState.BOTH);
                if (potential_move.is_legal(board) == true)
                    moves.Add(potential_move);
                // cant go through more than 1 enemy pieces
                if (potential_move.target_piece != null)
                    break;
            }
            for (x = this.x + 1, y = this.y - 1; x <= 7 && y >= 0; x++, y--)
            {
                potential_move = new Move(board, this, x, y, Move.AttackState.BOTH);
                if (potential_move.is_legal(board) == true)
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
        public King(bool is_white, int x, int y, int UID)
        {
            this.value = 12;
            this.ID = ChessPiece.KING_ID;
            this.UID = UID;
            this.set(x, y);
            this.is_white = is_white;
            if (this.is_white == true)
                this.rep = new Bitmap(Bitmap.FromFile("../../assets/white_king.png"), 64, 64);
            if (this.is_white == false)
                this.rep = new Bitmap(Bitmap.FromFile("../../assets/black_king.png"), 64, 64);
            this.name = (this.is_white ? "White " : "Black ") + "King";
        }
        // for cloning purposes
        public King(King piece)
        {
            this.value = piece.value;
            this.ID = piece.ID;
            this.UID = piece.UID;
            this.set(piece.x, piece.y);
            this.is_white = piece.is_white;
            this.rep = piece.rep;
            this.name = piece.name;

            this.alive = piece.alive;
            this.move_count = piece.move_count;
            this.selected = piece.selected; // neccessary ?
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
