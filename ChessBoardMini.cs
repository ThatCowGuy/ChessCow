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
    public class ChessBoardMini
    {
        // images
        public static Image IMG_white_pawn;
        public static Image IMG_white_rook;
        public static Image IMG_white_knight;
        public static Image IMG_white_bishop;
        public static Image IMG_white_queen;
        public static Image IMG_white_king;
        //-----------------------------
        public static Image IMG_black_pawn;
        public static Image IMG_black_rook;
        public static Image IMG_black_knight;
        public static Image IMG_black_bishop;
        public static Image IMG_black_queen;
        public static Image IMG_black_king;
        //-----------------------------
        public static Image IMG_board;

        // sizing
        public static int field_len = 512;
        public static int square_len = ChessBoardMini.field_len / 8;
        public static int rim_width = 5;
        public static int full_len = ChessBoardMini.field_len + (2 * ChessBoardMini.rim_width);

        public bool whites_turn = true;
        // inspired by stockfish
        // pieces will be stored within a single 64b-U-INT; each bit
        // is 1 square on the board, 1=piece is there, 0=not.
        public ulong white_pawns;
        public ulong white_rooks;
        public ulong white_knights;
        public ulong white_bishops;
        public ulong white_queens;
        public ulong white_kings;
        //-----------------------------
        public ulong black_pawns;
        public ulong black_rooks;
        public ulong black_knights;
        public ulong black_bishops;
        public ulong black_queens;
        public ulong black_kings;

        public int[,] board_content = new int[8, 8];

        public bool xy_is_white_piece(int x, int y)
        {
            ulong bit_index = xy_position_to_bitrep(x, y);
            return index_is_white_piece(bit_index);
        }
        public bool index_is_white_piece(ulong index)
        {
            if ((index & white_pawns) > 0) return true;
            if ((index & white_rooks) > 0) return true;
            if ((index & white_knights) > 0) return true;
            if ((index & white_bishops) > 0) return true;
            if ((index & white_queens) > 0) return true;
            if ((index & white_kings) > 0) return true;
            return false;
        }
        public bool xy_is_black_piece(int x, int y)
        {
            ulong bit_index = xy_position_to_bitrep(x, y);
            return index_is_black_piece(bit_index);
        }
        public bool index_is_black_piece(ulong index)
        {
            if ((index & black_pawns) > 0) return true;
            if ((index & black_rooks) > 0) return true;
            if ((index & black_knights) > 0) return true;
            if ((index & black_bishops) > 0) return true;
            if ((index & black_queens) > 0) return true;
            if ((index & black_kings) > 0) return true;
            return false;
        }
        public bool xy_is_piece_of_color(int x, int y, bool color)
        {
            ulong bit_index = xy_position_to_bitrep(x, y);
            return index_is_piece_of_color(bit_index, color);
        }
        public bool index_is_piece_of_color(ulong index, bool color)
        {
            if (color == ChessGame.WHITE)
                return index_is_white_piece(index);
            if (color == ChessGame.BLACK)
                return index_is_black_piece(index);
            return false;
        }

        public static ulong T_ROW =
            xy_position_to_bitrep(0, 7) |
            xy_position_to_bitrep(1, 7) |
            xy_position_to_bitrep(2, 7) |
            xy_position_to_bitrep(3, 7) |
            xy_position_to_bitrep(4, 7) |
            xy_position_to_bitrep(5, 7) |
            xy_position_to_bitrep(6, 7) |
            xy_position_to_bitrep(7, 7);
        public static ulong B_ROW =
            xy_position_to_bitrep(0, 0) |
            xy_position_to_bitrep(1, 0) |
            xy_position_to_bitrep(2, 0) |
            xy_position_to_bitrep(3, 0) |
            xy_position_to_bitrep(4, 0) |
            xy_position_to_bitrep(5, 0) |
            xy_position_to_bitrep(6, 0) |
            xy_position_to_bitrep(7, 0);
        public static ulong L_COL =
            xy_position_to_bitrep(0, 0) |
            xy_position_to_bitrep(0, 1) |
            xy_position_to_bitrep(0, 2) |
            xy_position_to_bitrep(0, 3) |
            xy_position_to_bitrep(0, 4) |
            xy_position_to_bitrep(0, 5) |
            xy_position_to_bitrep(0, 6) |
            xy_position_to_bitrep(0, 7);
        public static ulong R_COL =
            xy_position_to_bitrep(7, 0) |
            xy_position_to_bitrep(7, 1) |
            xy_position_to_bitrep(7, 2) |
            xy_position_to_bitrep(7, 3) |
            xy_position_to_bitrep(7, 4) |
            xy_position_to_bitrep(7, 5) |
            xy_position_to_bitrep(7, 6) |
            xy_position_to_bitrep(7, 7);

        public static ulong xy_position_to_bitrep(int x, int y)
        {
            int index = (y * 8) + x;
            return ((ulong)1 << index);
        }

        public ChessBoardMini()
        {
            this.load_images();
            this.white_normal_init();
            this.black_normal_init();
        }
        public void load_images()
        {
            IMG_white_pawn = new Bitmap(Bitmap.FromFile("../../assets/white_pawn.png"), 64, 64);
            IMG_white_rook = new Bitmap(Bitmap.FromFile("../../assets/white_rook.png"), 64, 64);
            IMG_white_knight = new Bitmap(Bitmap.FromFile("../../assets/white_knight.png"), 64, 64);
            IMG_white_bishop = new Bitmap(Bitmap.FromFile("../../assets/white_bishop.png"), 64, 64);
            IMG_white_queen = new Bitmap(Bitmap.FromFile("../../assets/white_queen.png"), 64, 64);
            IMG_white_king = new Bitmap(Bitmap.FromFile("../../assets/white_king.png"), 64, 64);

            IMG_black_pawn = new Bitmap(Bitmap.FromFile("../../assets/black_pawn.png"), 64, 64);
            IMG_black_rook = new Bitmap(Bitmap.FromFile("../../assets/black_rook.png"), 64, 64);
            IMG_black_knight = new Bitmap(Bitmap.FromFile("../../assets/black_knight.png"), 64, 64);
            IMG_black_bishop = new Bitmap(Bitmap.FromFile("../../assets/black_bishop.png"), 64, 64);
            IMG_black_queen = new Bitmap(Bitmap.FromFile("../../assets/black_queen.png"), 64, 64);
            IMG_black_king = new Bitmap(Bitmap.FromFile("../../assets/black_king.png"), 64, 64);

            IMG_board = new Bitmap(Bitmap.FromFile("../../assets/chess_board_black.png"), 512, 512);
        }
        public void white_normal_init()
        {
            white_pawns = 0;
            white_pawns |= xy_position_to_bitrep(0, 1);
            white_pawns |= xy_position_to_bitrep(1, 1);
            white_pawns |= xy_position_to_bitrep(2, 1);
            white_pawns |= xy_position_to_bitrep(3, 1);
            white_pawns |= xy_position_to_bitrep(4, 1);
            white_pawns |= xy_position_to_bitrep(5, 1);
            white_pawns |= xy_position_to_bitrep(6, 1);
            white_pawns |= xy_position_to_bitrep(7, 1);

            white_rooks = 0;
            white_rooks |= xy_position_to_bitrep(0, 0);
            white_rooks |= xy_position_to_bitrep(7, 3);

            white_knights = 0;
            white_knights |= xy_position_to_bitrep(1, 0);
            white_knights |= xy_position_to_bitrep(6, 0);

            white_bishops = 0;
            white_bishops |= xy_position_to_bitrep(2, 0);
            white_bishops |= xy_position_to_bitrep(5, 3);

            white_queens = 0;
            white_queens |= xy_position_to_bitrep(3, 0);

            white_kings = 0;
            white_kings |= xy_position_to_bitrep(4, 0);
        }
        public void black_normal_init()
        {
            black_pawns = 0;
            black_pawns |= xy_position_to_bitrep(0, 6);
            black_pawns |= xy_position_to_bitrep(1, 6);
            black_pawns |= xy_position_to_bitrep(2, 6);
            black_pawns |= xy_position_to_bitrep(3, 6);
            black_pawns |= xy_position_to_bitrep(4, 6);
            black_pawns |= xy_position_to_bitrep(5, 6);
            black_pawns |= xy_position_to_bitrep(6, 6);
            black_pawns |= xy_position_to_bitrep(7, 6);

            black_rooks = 0;
            black_rooks |= xy_position_to_bitrep(0, 7);
            black_rooks |= xy_position_to_bitrep(7, 7);

            black_knights = 0;
            black_knights |= xy_position_to_bitrep(1, 7);
            black_knights |= xy_position_to_bitrep(6, 7);

            black_bishops = 0;
            black_bishops |= xy_position_to_bitrep(2, 7);
            black_bishops |= xy_position_to_bitrep(5, 7);

            black_queens = 0;
            black_queens |= xy_position_to_bitrep(3, 7);

            black_kings = 0;
            black_kings |= xy_position_to_bitrep(4, 7);
        }

        // << 8     == up
        // >> 8     == down
        // << 1     == left
        // >> 1     == right

        public int[] get_bishop_moves(ulong pos_index)
        {
            for (ulong sensor_index = pos_index; (sensor_index & (ChessBoardMini.T_ROW | ChessBoardMini.R_COL)) == 0; sensor_index <<= 7)
            {

            }
        }

        public static Rectangle xy_to_rect(int x, int y)
        {
            int index = (y * 8) + x;
            return index_to_rect(index);
        }
        public static Rectangle index_to_rect(int index)
        {
            int x = (index % 8);
            int y = (index / 8);

            // flip y so 0 is at the bottom
            y = (7 - y);

            return new Rectangle(
                ChessBoardMini.rim_width + (ChessBoardMini.square_len * x),
                ChessBoardMini.rim_width + (ChessBoardMini.square_len * y),
                ChessBoardMini.square_len, ChessBoardMini.square_len
            );
        }
        public void draw(Graphics g)
        {
            g.DrawImage(ChessBoardMini.IMG_board,
                ChessBoardMini.rim_width,
                ChessBoardMini.rim_width,
                ChessBoardMini.field_len,
                ChessBoardMini.field_len);

            for (int index = 0; index < 8*8; index++)
            {
                ulong index_bit_rep = ((ulong)1 << index);

                if ((this.white_pawns & index_bit_rep) > 0)
                    g.DrawImage(ChessBoardMini.IMG_white_pawn, index_to_rect(index));
                if ((this.white_rooks & index_bit_rep) > 0)
                    g.DrawImage(ChessBoardMini.IMG_white_rook, index_to_rect(index));
                if ((this.white_knights & index_bit_rep) > 0)
                    g.DrawImage(ChessBoardMini.IMG_white_knight, index_to_rect(index));
                if ((this.white_bishops & index_bit_rep) > 0)
                    g.DrawImage(ChessBoardMini.IMG_white_bishop, index_to_rect(index));
                if ((this.white_queens & index_bit_rep) > 0)
                    g.DrawImage(ChessBoardMini.IMG_white_queen, index_to_rect(index));
                if ((this.white_kings & index_bit_rep) > 0)
                    g.DrawImage(ChessBoardMini.IMG_white_king, index_to_rect(index));

                if ((this.black_pawns & index_bit_rep) > 0)
                    g.DrawImage(ChessBoardMini.IMG_black_pawn, index_to_rect(index));
                if ((this.black_rooks & index_bit_rep) > 0)
                    g.DrawImage(ChessBoardMini.IMG_black_rook, index_to_rect(index));
                if ((this.black_knights & index_bit_rep) > 0)
                    g.DrawImage(ChessBoardMini.IMG_black_knight, index_to_rect(index));
                if ((this.black_bishops & index_bit_rep) > 0)
                    g.DrawImage(ChessBoardMini.IMG_black_bishop, index_to_rect(index));
                if ((this.black_queens & index_bit_rep) > 0)
                    g.DrawImage(ChessBoardMini.IMG_black_queen, index_to_rect(index));
                if ((this.black_kings & index_bit_rep) > 0)
                    g.DrawImage(ChessBoardMini.IMG_black_king, index_to_rect(index));
            }

            Pen pen = new Pen(Color.Gray, ChessBoardMini.rim_width * 2);
            g.DrawRectangle(pen, 0, 0, ChessBoardMini.full_len, ChessBoardMini.full_len);
            pen.Dispose();
        }
    }
}
