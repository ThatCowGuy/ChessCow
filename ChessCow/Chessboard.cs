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
    public class Chessboard
    {
        public Image w_pawn;
        public Image w_rook;
        public Image w_knight;
        public Image w_bishop;
        public Image w_queen;
        public Image w_king;

        public ChessPiece[] white_pieces;
        public ChessPiece[] black_pieces;

        public static int xdim = 8;
        public static int ydim = 8;

        public static int field_w = 512;
        public static int field_h = 512;
        public static int tile_w = field_w / Chessboard.xdim;
        public static int tile_h = field_h / Chessboard.ydim;
        public static int boarder_w = 5;
        public static int full_w = field_w + (2 * boarder_w);
        public static int full_h = field_h + (2 * boarder_w);

        public Chessboard()
        {
            System.Console.WriteLine("A\n");
            this.white_pieces = new ChessPiece[16];
            this.white_pieces[0] = new Pawn();
            this.white_pieces[1] = new Pawn();
        }

        public void draw(Graphics g)
        {
            System.Console.WriteLine("drawtime\n");

            SolidBrush brush = new SolidBrush(Color.White);
            Pen pen = new Pen(Color.Gray, boarder_w * 2);
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    int ID = (row % 2) + (col % 2);
                    if (ID % 2 == 0) brush.Color = System.Drawing.Color.White;
                    if (ID % 2 == 1) brush.Color = System.Drawing.Color.Black;

                    g.FillRectangle(brush, boarder_w + (tile_w * col), boarder_w + (tile_h * row), tile_w, tile_h);


                }
            }

            this.white_pieces[0].set(3, 4);
            this.white_pieces[0].draw(g);

            g.DrawRectangle(pen, 0, 0, full_w, full_h);
            pen.Dispose();
            brush.Dispose();
        }
    }
}
