using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cecs475.BoardGames.ComputerOpponent {
    /// <summary>
    /// A pair of an IGameMove that was the best move to apply for a given board state,
    /// and the Weight of the board that resulted.
    /// </summary>
    internal struct MinimaxBestMove {
        public int Weight { get; set; }
        public IGameMove Move { get; set; }
    }

    /// <summary>
    /// A minimax with alpha-beta pruning implementation of IGameAi.
    /// </summary>
    public class MinimaxAi : IGameAi {
        private readonly int _mMaxDepth;

        public MinimaxAi(int maxDepth) {
            _mMaxDepth = maxDepth;
        }

        // The public calls this function, which kicks off the minimax search.
        public IGameMove FindBestMove(IGameBoard b) {
            // TODO: call the private FindBestMove with appropriate values for the parameters.
            // mMaxDepth is what the depthLeft should start at.
            // You are maximizing iff the board's current player is 1.

            return b.CurrentPlayer == 1
                ? FindBestMove(b, _mMaxDepth, true, int.MinValue, int.MaxValue).Move
                : FindBestMove(b, _mMaxDepth, false, int.MinValue, int.MaxValue).Move;
        }

        private static MinimaxBestMove FindBestMove(IGameBoard b, int depthLeft, bool maximize, int alpha, int beta) {
            // Implement the minimax algorithm. 
            // Your first attempt will not use alpha-beta pruning. Once that works, 
            // implement the pruning as discussed in the project notes.
            if (depthLeft == 0 || b.IsFinished) {
                return new MinimaxBestMove {
                    Move = null,
                    Weight = b.Weight
                };
            }

            IGameMove bestMove = null;

            foreach (var possibleMove in b.GetPossibleMoves()) {
                b.ApplyMove(possibleMove);
                var nextBestMove = FindBestMove(b, depthLeft - 1, !maximize, alpha, beta);
                b.UndoLastMove();

                if (maximize) {
                    // i.e. alpha became > beta, so return this
                    if (nextBestMove.Weight >= beta) {
                        return new MinimaxBestMove {
                            Weight = beta,
                            Move = possibleMove
                        };
                    }

                    if (nextBestMove.Weight <= alpha) continue;

                    bestMove = possibleMove;
                    alpha = nextBestMove.Weight;
                } else {
                    // i.e. beta became < alpha, so return this
                    if (nextBestMove.Weight <= alpha) {
                        return new MinimaxBestMove {
                            Weight = alpha,
                            Move = possibleMove
                        };
                    }

                    if (nextBestMove.Weight >= beta) continue;

                    bestMove = possibleMove;
                    beta = nextBestMove.Weight;
                }
            }
            return new MinimaxBestMove {
                Move = bestMove,
                Weight = maximize ? alpha : beta
            };
        }
    }
}