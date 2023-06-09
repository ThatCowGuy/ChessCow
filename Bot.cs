﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessCow2
{
    class Bot
    {
        public static System.Random RNG = new Random();

        // these are statically available, so that it is accessible in all recursions
        public static double current_best_eval_change;
        public static double starting_eval;
        public static bool is_white;
        public static int depth = 1;
        public static int sim_count;

        public static Move get_random_move(ChessBoard board)
        {
            // dont attempt to move if the game is over
            if (board.gamestate != ChessBoard.GameState.ONGOING)
                return null;

            if (board.whites_turn == true)
            {
                int move_index = Bot.RNG.Next(0, board.white_legal_moves.Count);
                return board.white_legal_moves.ElementAt(move_index);
            }
            if (board.whites_turn == false)
            {
                int move_index = Bot.RNG.Next(0, board.black_legal_moves.Count);
                return board.white_legal_moves.ElementAt(move_index);
            }
            return null;
        }
        public static Move get_one_best_move(ChessBoard board)
        {
            // dont attempt to move if the game is over
            if (board.gamestate != ChessBoard.GameState.ONGOING)
                return null;

            Bot.current_best_eval_change = -100000;
            Bot.starting_eval = evaluate_position(board, board.whites_turn);
            Bot.is_white = board.whites_turn;

            Bot.sim_count = 0;
            List<Move> best_moves = Bot.get_best_moves(board, Bot.starting_eval, Bot.depth);
            Console.WriteLine("Simulated Moves in Total: {0}", Bot.sim_count);

            if (best_moves.Count == 0) return null;

            int move_index = Bot.RNG.Next(0, best_moves.Count);
            return best_moves.ElementAt(move_index);
        }

        public static List<Move> get_best_moves(ChessBoard board, double previous_eval, int depth)
        {
            List<Move> current_moves = new List<Move>();
            if (board.whites_turn == true)
                current_moves.AddRange(board.white_legal_moves);
            if (board.whites_turn == false)
                current_moves.AddRange(board.black_legal_moves);

            // figure out the eval change for every move
            double max_eval_change = -100000;
            foreach (Move move in current_moves)
            {
                ChessBoard simulation_outer = new ChessBoard(board);
                simulation_outer.execute_move(new Move(move, simulation_outer));
                simulation_outer.calc_legal_moves();
                Bot.sim_count++;

                double current_eval = evaluate_position(simulation_outer, !simulation_outer.whites_turn); // NOTE: NOT-board.whites_turn

                if (depth > 0)
                {
                    List<Move> best_counter_moves = get_best_moves(simulation_outer, current_eval, (depth - 1));

                    if (best_counter_moves.Count > 0)
                    {
                        ChessBoard simulation_inner = new ChessBoard(simulation_outer);
                        simulation_inner.execute_move(new Move(best_counter_moves.ElementAt(0), simulation_inner));
                        simulation_inner.calc_legal_moves();

                        // NOTE: using NOT whites turn here
                        current_eval = evaluate_position(simulation_inner, simulation_inner.whites_turn);
                    }
                    else
                    {
                        Console.WriteLine(move.ToString() + "-> Results in a Game-Over");
                        if (simulation_outer.whites_turn == true)
                        {
                            if (simulation_outer.white_pieces[ChessPiece.KING].is_checked(simulation_outer) == true)
                                current_eval = 10000;
                            else
                                current_eval = 0;
                        }
                        if (simulation_outer.whites_turn == false)
                        {
                            if (simulation_outer.black_pieces[ChessPiece.KING].is_checked(simulation_outer) == true)
                                current_eval = 10000;
                            else
                                current_eval = 0;
                        }
                    }
                }

                move.eval_change = current_eval - previous_eval;
                if (move.eval_change > max_eval_change)
                    max_eval_change = move.eval_change;

                if (depth == Bot.depth)
                {
                    Console.WriteLine("{0} -- EvalChange: {1}", move.ToString(), move.eval_change);
                    if (move.eval_change > Bot.current_best_eval_change)
                    {
                        Bot.current_best_eval_change = move.eval_change;
                        Console.WriteLine("[!!] NEW BEST: {0}", move.eval_change);
                    }
                }
            }
            if (depth == Bot.depth)
                Console.WriteLine(">>> BEST = {0}", max_eval_change);

            List<Move> best_moves = new List<Move>();
            foreach (Move move in current_moves)
            {
                if (move.eval_change == max_eval_change)
                    best_moves.Add(move);
            }
            return best_moves;
        }

        public static double MULT_threat_self = 0.4;
        public static double MULT_threat_opponent = 0.6;

        public static double MULT_take_pieces = 5.0;
        public static double MULT_lose_pieces = 5.0;

        public static double MULT_board_control_self = 0.07;
        public static double MULT_board_control_opponent = 0.05;
        public static double MULT_board_protection_mod = 0.5;

        public static double evaluate_position(ChessBoard board, bool for_white)
        {
            double value = 0;
            // I remove value from a move that threatens myself
            // and from moves that result in me having less pieces..

            if (for_white == true)
            {
                value += board.total_threat_level_black() * MULT_threat_opponent;
                value -= board.total_threat_level_white() * MULT_threat_self;

                value += board.total_piece_value_white() * MULT_take_pieces;
                value -= board.total_piece_value_black() * MULT_lose_pieces;

                value += board.space_control_evaluation(true, MULT_board_protection_mod) * MULT_board_control_self;
                value -= board.space_control_evaluation(false, MULT_board_protection_mod) * MULT_board_control_opponent;

                // opponent in checkmate
                if (board.whites_turn == false && board.black_legal_moves.Count == 0 && board.checking_black_king() == true)
                    value = 10000;
                // self in checkmate
                if (board.whites_turn == true && board.white_legal_moves.Count == 0 && board.checking_white_king() == true)
                    value = -10000;
            }
            if (for_white == false)
            {
                value += board.total_threat_level_white() * MULT_threat_opponent;
                value -= board.total_threat_level_black() * MULT_threat_self;

                value += board.total_piece_value_black() * MULT_take_pieces;
                value -= board.total_piece_value_white() * MULT_lose_pieces;

                value += board.space_control_evaluation(false, MULT_board_protection_mod) * MULT_board_control_self;
                value -= board.space_control_evaluation(true, MULT_board_protection_mod) * MULT_board_control_opponent;

                // opponent in checkmate
                if (board.whites_turn == true && board.white_legal_moves.Count == 0 && board.checking_white_king() == true)
                    value = +10000;
                // self in checkmate
                if (board.whites_turn == false && board.black_legal_moves.Count == 0 && board.checking_black_king() == true)
                    value = -10000;
            }

            return value;
        }
    }
}
