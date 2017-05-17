using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Cecs475.BoardGames;
using Cecs475.BoardGames.View;
using System;
using Cecs475.BoardGames.ComputerOpponent;
using System.Threading.Tasks;

namespace Cecs475.BoardGames.Othello.View
{
    public class OthelloSquare : INotifyPropertyChanged
    {
        private int mPlayer;
        public int Player
        {
            get { return mPlayer; }
            set
            {
                if (value != mPlayer)
                {
                    mPlayer = value;
                    OnPropertyChanged(nameof(Player));
                }
            }
        }

        private bool mIsHovered;
        public bool IsHovered
        {
            get { return mIsHovered; }
            set
            {
                if (value != mIsHovered)
                {
                    mIsHovered = value;
                    OnPropertyChanged(nameof(IsHovered));
                }
            }
        }

        public BoardPosition Position
        {
            get; set;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class OthelloViewModel : INotifyPropertyChanged, IGameViewModel
    {
        private const int MAX_AI_DEPTH = 8;
        private OthelloBoard mBoard;
        private ObservableCollection<OthelloSquare> mSquares;
        private IGameAi mGameAi = new MinimaxAi(MAX_AI_DEPTH);

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler GameFinished;
        /// <summary>
        /// Invoked after applying a move, if the new current player must pass their turn.
        /// </summary>
        public event EventHandler CurrentPlayerMustPass;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public OthelloViewModel()
        {
            mBoard = new OthelloBoard();
            mSquares = new ObservableCollection<OthelloSquare>(
                from pos in (
                    from r in Enumerable.Range(0, 8)
                    from c in Enumerable.Range(0, 8)
                    select new BoardPosition(r, c)
                )
                select new OthelloSquare()
                {
                    Position = pos,
                    Player = mBoard.GetPieceAtPosition(pos)
                }
            );

            PossibleMoves = new HashSet<BoardPosition>(
                from OthelloMove m in mBoard.GetPossibleMoves()
                select m.Position
            );
        }

        public void UndoMove()
        {
            mBoard.UndoLastMove();
            // In one-player mode, Undo has to remove an additional move to return to the
            // human player's turn.
            if (Players == NumberOfPlayers.One)
            {
                mBoard.UndoLastMove();
            }
            PossibleMoves = new HashSet<BoardPosition>(
                from OthelloMove m in mBoard.GetPossibleMoves()
                select m.Position
            );
            var newSquares =
                from r in Enumerable.Range(0, 8)
                from c in Enumerable.Range(0, 8)
                select new BoardPosition(r, c);
            int i = 0;
            foreach (var pos in newSquares)
            {
                mSquares[i].Player = mBoard.GetPieceAtPosition(pos);
                i++;
            }

            OnPropertyChanged(nameof(BoardValue));
            OnPropertyChanged(nameof(CurrentPlayer));
            OnPropertyChanged(nameof(CanUndo));
        }

        public async Task ApplyMove(BoardPosition position)
        {
            var possMoves = mBoard.GetPossibleMoves() as IEnumerable<OthelloMove>;
            foreach (var move in possMoves)
            {
                if (move.Position.Equals(position))
                {
                    mBoard.ApplyMove(move);
                    break;
                }
            }
            RebindState();
            if (Players == NumberOfPlayers.One && !mBoard.IsFinished)
            {

                var bestMove = await Task.Run(() => mGameAi.FindBestMove(mBoard));
                if (bestMove != null)
                {
                    mBoard.ApplyMove(bestMove);
                    RebindState();
                }
            }

            if (mBoard.PassCount == 2)
            {
                GameFinished?.Invoke(this, new EventArgs());
            }

            if (PossibleMoves.Count == 1 && PossibleMoves.First().Row == -1)
            {
                CurrentPlayerMustPass?.Invoke(this, new EventArgs());
            }

            if (PossibleMoves.Count == 0 || mBoard.IsFinished)
            {
                GameFinished?.Invoke(this, new EventArgs());
            }
        }

        private void RebindState()
        {
            PossibleMoves = new HashSet<BoardPosition>(
                from OthelloMove m in mBoard.GetPossibleMoves()
                select m.Position
            );
            var newSquares =
                from r in Enumerable.Range(0, 8)
                from c in Enumerable.Range(0, 8)
                select new BoardPosition(r, c);
            int i = 0;
            foreach (var pos in newSquares)
            {
                mSquares[i].Player = mBoard.GetPieceAtPosition(pos);
                i++;
            }

            OnPropertyChanged(nameof(BoardValue));
            OnPropertyChanged(nameof(CurrentPlayer));
            OnPropertyChanged(nameof(CanUndo));
        }

        public ObservableCollection<OthelloSquare> Squares
        {
            get { return mSquares; }
        }

        public HashSet<BoardPosition> PossibleMoves
        {
            get; private set;
        }

        public int BoardValue { get { return mBoard.Value; } }

        public int CurrentPlayer { get { return mBoard.CurrentPlayer; } }

        public NumberOfPlayers Players { get; set; }

        public bool CanUndo
        {
            get
            {
                return mBoard.MoveHistory.Count > 0;
            }
        }


    }
}