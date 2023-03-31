﻿using System;
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
        // these are just for readability
        // (only the Pawn needs these)
        public enum AttackState
        {
            PURE_MOVEMENT,
            PURE_ATTACK,
            BOTH,
            PROTECTION
        };

        public static Image legal_move_rep = new Bitmap(Bitmap.FromFile("../../assets/legal_move.png"), 64, 64);
        public static Image attack_move_rep = new Bitmap(Bitmap.FromFile("../../assets/attack_move.png"), 64, 64);
        public static Image selector = new Bitmap(Bitmap.FromFile("../../assets/selector.png"), 64, 64);
        public static Image check_flag = new Bitmap(Bitmap.FromFile("../../assets/check_flag.png"), 64, 64);
        public static Image move_marker = new Bitmap(Bitmap.FromFile("../../assets/move_marker.png"), 64, 64);

        public ChessPiece moving_piece; // who is moving ?
        public ChessPiece target_piece; // is this move targetting someone ?
        public int target_x;
        public int target_y;
        public AttackState attack_state;
        // these 2 are just for undoing Moves
        public int origin_x;
        public int origin_y;

        public double eval_change = 0;

        public bool legal = false;

        public Move(ChessBoard board, ChessPiece piece, int x, int y, AttackState attack_state)
        {
            this.moving_piece = piece;
            this.target_x = x;
            this.target_y = y;
            this.attack_state = attack_state;
            this.calc_target_piece(board);
            // for undoing moves
            this.origin_x = this.moving_piece.x;
            this.origin_y = this.moving_piece.y;
        }

        public bool is_legal(ChessBoard board, bool consider_selfcheck)
        {
            // out of bounds is obviously illegal
            if (this.is_out_of_bounds() == true) return false;
            // cant move to a space where your own piece is standing
            if (this.target_piece != null)
            {
                if (this.target_piece.is_white == this.moving_piece.is_white)
                {
                    // protection moves cannot be made, but they need to exist
                    this.attack_state = AttackState.PROTECTION;
                }
            }

            if (this.attack_state == AttackState.PURE_ATTACK)
            {
                // check if this move has an enemy target
                if (this.target_piece == null)
                    return false;
            }
            else if (this.attack_state == AttackState.PURE_MOVEMENT)
            {
                // check if this move bumps into another piece
                if (board.occupation[this.target_x, this.target_y] != null)
                    return false;
            }
            // It would be A LOT Better, to pretend I am the King after sim, DO NOT EVEN get legal enemy moves,
            // and see if the king could hit an enemy piece when moving like that enemy piece !!!
            if (consider_selfcheck == true)
            {
                // and finally, check if the move results in a check for your own king
                board.simulate_move(this, false);

                bool check = false;
                if (board.whites_turn == true)
                {
                    if (board.black_pieces[12].threat_count(board) > 0)
                        check = true;
                }
                if (board.whites_turn == false)
                {
                    if (board.white_pieces[12].threat_count(board) > 0)
                        check = true;
                }
                board.undo_move(this);
                if (check == true)
                    return false;
            }

            // no triggers met = legal move
            return true;
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
        public void calc_target_piece(ChessBoard board)
        {
            // if the move is oob or a pure Movement move, we definetly dont have a target
            if (this.is_out_of_bounds() == true || this.attack_state == AttackState.PURE_MOVEMENT)
            {
                this.target_piece = null;
                return;
            }
            ChessPiece target_space = board.occupation[this.target_x, this.target_y];
            // if there is no piece on the target space, we don't collide with anything
            if (target_space == null)
            {
                this.target_piece = null;
                return;
            }

            this.target_piece = target_space;
            return;
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
