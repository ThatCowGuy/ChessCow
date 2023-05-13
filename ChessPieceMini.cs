using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessCow2
{
    public class ChessPieceMini
    {
        // position
        public byte x;
        public byte y;

        public int descr_bitfield;
        // these define the piece
        public static int WHITE     = 0b_1_0000_0000;
        public static int PAWN      = 0b_0000_0001;
        public static int ROOK      = 0b_0000_0010;
        public static int KNIGHT    = 0b_0000_0100;
        public static int BISHOP    = 0b_0000_1000;
        public static int QUEEN     = 0b_0001_0000;
        public static int KING      = 0b_0010_0000;
        public static int EN_PASS   = 0b_0100_0000;
    }
}
