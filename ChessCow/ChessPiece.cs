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


namespace ChessCow
{
    public class ChessPiece
    {
        public Image rep;

        public int x;
        public int y;
        public int x_draw;
        public int y_draw;
        public string name;

        void set_x(int x)
        {
            this.x = x;
            this.x_draw = Chessboard.boarder_w + (Chessboard.tile_w * this.x);
        }
        void set_y(int y)
        {
            this.y = y;
            this.y_draw = Chessboard.boarder_w + (Chessboard.tile_h * this.y);
        }
        public void set(int x, int y)
        {
            this.set_x(x);
            this.set_y(y);
        }

        public void draw(Graphics g) { System.Console.WriteLine("Trying to draw generic Piece...\n"); }
    }

    public class Pawn : ChessPiece
    {
        public Pawn()
        {
            System.Console.WriteLine("IM A PAWN\n");
            this.rep = new Bitmap(Bitmap.FromFile("../../../assets/white_pawn.png"), 64, 64);
        }
        public new void draw(Graphics g)
        {
            g.DrawImage(this.rep, x_draw, y_draw, Chessboard.tile_w, Chessboard.tile_w);
        }
    }
}
