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
    public class Move : IEquatable<Move>
    {
        public static Image legal_move_rep = new Bitmap(Bitmap.FromFile("../../assets/legal_move.png"), 64, 64);
        public static Image move_rep = new Bitmap(Bitmap.FromFile("../../assets/move.png"), 64, 64);

        public ChessPiece moving_piece;
        public int target_x;
        public int target_y;

        public bool legal = false;

        public Move(ChessPiece piece, int x, int y)
        {
            this.moving_piece = piece;
            this.target_x = x;
            this.target_y = y;
        }


        public bool collides_with_ally(ChessBoard board)
        {
            ChessPiece target_space = board.occupation[this.target_x, this.target_y];

            // if there is no piece on the target space, we don't collide with anything
            if (target_space == null) return false;

            // if there is a piece of the same color, we collide with an ally
            if (target_space.is_white == this.moving_piece.is_white) return true;

            // else, we don't
            return false;
        }
        public bool is_out_of_bounds()
        {
            if (this.target_x < 0) return true;
            if (this.target_x > 7) return true;
            if (this.target_y < 0) return true;
            if (this.target_y > 7) return true;
            return false;
        }

        public bool Equals(Move other)
        {
            if (this.moving_piece != other.moving_piece) return false;
            if (this.target_x != other.target_x) return false;
            if (this.target_y != other.target_y) return false;
            return true;
        }
    }
}
