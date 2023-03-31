using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessCow2
{
    class Bot
    {
        public static System.Random RNG = new Random();

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
        public static Move get_best_move(ChessBoard board)
        {
            // dont attempt to move if the game is over
            if (board.gamestate != ChessBoard.GameState.ONGOING)
                return null;

            Bot.current_best_eval_change = -100000;
            Bot.starting_eval = evaluate_position(board, board.whites_turn);
            Bot.is_white = board.whites_turn;

            List<Move> best_moves = Bot.get_best_moves(board, Bot.starting_eval, Bot.depth);

            if (best_moves.Count == 0) return null;

            int move_index = Bot.RNG.Next(0, best_moves.Count);
            return best_moves.ElementAt(move_index);
        }

        // these are statically available, so that it is accessible in all recursions
        public static double current_best_eval_change;
        public static double starting_eval;
        public static bool is_white;
        public static int depth;

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
                board.simulate_move(move, true);
                double current_eval = evaluate_position(board, !board.whites_turn);

                if (depth > 0)
                {
                    List<Move> best_counter_moves = get_best_moves(board, current_eval, (depth - 1));

                    if (best_counter_moves.Count > 0)
                    {
                        // I really should test every best move here.. or actually every-every move...
                        board.simulate_move(best_counter_moves.ElementAt(0), true);
                        // NOTE: using NOT whites turn here
                        current_eval = evaluate_position(board, board.whites_turn);

                        board.undo_move(best_counter_moves.ElementAt(0));
                    }
                    else
                    {
                        Console.WriteLine("Move: {0} => {1}|{2} == {3}", move.moving_piece.name, move.target_x, move.target_y, move.eval_change);
                        Console.WriteLine("this moves results in a game end");
                        if (board.whites_turn == true)
                        {
                            if (board.checking_white_king() == true)
                                current_eval = 10000;
                            else
                                current_eval = 0;
                        }
                        if (board.whites_turn == false)
                        {
                            if (board.checking_black_king() == true)
                                current_eval = 10000;
                            else
                                current_eval = 0;
                        }
                    }
                }
                board.undo_move(move);

                move.eval_change = current_eval - previous_eval;
                if (move.eval_change > max_eval_change)
                    max_eval_change = move.eval_change;

                if (depth == Bot.depth)
                {
                    Console.WriteLine("Move: {0} => {1}|{2} == {3}", move.moving_piece.name, move.target_x, move.target_y, move.eval_change);
                    if (move.eval_change > Bot.current_best_eval_change)
                    {
                        Bot.current_best_eval_change = move.eval_change;
                        Console.WriteLine("[!!] NEW BEST: {0}", move.eval_change);
                    }
                }
            }
            if (depth == Bot.depth)
                Console.WriteLine("BEST = {0}", max_eval_change);

            List<Move> best_moves = new List<Move>();
            foreach (Move move in current_moves)
            {
                if (move.eval_change == max_eval_change)
                    best_moves.Add(move);
            }
            return best_moves;
        }

        public static double evaluate_position(ChessBoard board, bool for_white)
        {
            double value = 0;
            // I remove value from a move that threatens myself
            // and from moves that result in me having less pieces..

            double MULT_threat_self = 0.4;
            double MULT_threat_opponent = 0.6;

            double MULT_take_pieces = 5.0;
            double MULT_lose_pieces = 5.0;

            if (for_white == true)
            {
                value += board.total_threat_level_black() * MULT_threat_opponent;
                value -= board.total_threat_level_white() * MULT_threat_self;

                value += board.total_piece_value_white() * MULT_take_pieces;
                value -= board.total_piece_value_black() * MULT_lose_pieces;

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
