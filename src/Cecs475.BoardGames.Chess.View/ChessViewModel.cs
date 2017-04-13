using Cecs475.BoardGames.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Cecs475.BoardGames.Chess.View {
    public class ChessViewModel : IGameViewModel {
        private readonly ChessBoard _board;

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler GameFinished;

        private void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ChessViewModel() {
            _board = new ChessBoard();

            Squares = new ObservableCollection<ChessSquare>(
                from pos in (
                    from r in Enumerable.Range(0, 8)
                    from c in Enumerable.Range(0, 8)
                    select new BoardPosition(r, c)
                )
                select new ChessSquare {
                    Position = pos,
                    Piece = _board.GetPieceAtPosition(pos)
                }
            );
        }

        public int BoardValue => _board.Value;

        public int CurrentPlayer => _board.CurrentPlayer;

        public bool HasSelected => Squares.Any(s => s.IsSelected);
        
        public ObservableCollection<ChessSquare> Squares { get; }

        public HashSet<BoardPosition> PossibleMoves => new HashSet<BoardPosition>(
            from ChessMove move in _board.GetPossibleMoves()
            where move.StartPosition.Equals(Squares.FirstOrDefault(s => s.IsSelected)?.Position)
            select move.EndPosition
        );

        public bool CanUndo => _board.MoveHistory.Count > 0;

        public void UndoMove() {
            if (!CanUndo) return;

            _board.UndoLastMove();
            if (_board.NeedToPawnPromote)
                _board.UndoLastMove();

            foreach (var chessSquare in Squares) {
                chessSquare.Piece = _board.GetPieceAtPosition(chessSquare.Position);
            }

            OnPropertyChanged(nameof(BoardValue));
            OnPropertyChanged(nameof(Squares));
            OnPropertyChanged(nameof(CurrentPlayer));
            OnPropertyChanged(nameof(CanUndo));
        }

        public void ApplyMove(BoardPosition squarePosition) {
            var possibleMoves = _board.GetPossibleMoves() as IEnumerable<ChessMove>;
            var selectedPiece = Squares.FirstOrDefault(s => s.IsSelected);

            if (possibleMoves == null || selectedPiece == null)
                return;

            foreach (var move in possibleMoves) {
                // Since pawns are special
                if (!move.StartPosition.Equals(selectedPiece.Position) ||
                    !move.EndPosition.Equals(squarePosition))
                    continue;

                // Make a move
                _board.ApplyMove(move);
                break;
            }

            if (_board.NeedToPawnPromote) {
                var dialog = new PawnPromotionDialog(this);
                dialog.ShowDialog();
                possibleMoves = _board.GetPossibleMoves() as IEnumerable<ChessMove>;
                _board.ApplyMove(possibleMoves?.FirstOrDefault(move => move.ToString().Contains(dialog.Piece)));
            }
            
            foreach (var chessSquare in Squares) {
                chessSquare.Piece = _board.GetPieceAtPosition(chessSquare.Position);
                if (_board.IsCheck && chessSquare.Piece.Player == CurrentPlayer &&
                    chessSquare.Piece.PieceType == ChessPieceType.King)
                    chessSquare.IsInCheck = true;
            }

            OnPropertyChanged(nameof(BoardValue));
            OnPropertyChanged(nameof(Squares));
            OnPropertyChanged(nameof(CurrentPlayer));
            OnPropertyChanged(nameof(CanUndo));

            if (!_board.GetPossibleMoves().Any())
                GameFinished?.Invoke(this, new EventArgs());
        }
    }

    public class ChessSquare : INotifyPropertyChanged {
        private ChessPiecePosition _piece;
        private bool _isHovered;
        private bool _isSelected;
        private bool _isInCheck;

        public event PropertyChangedEventHandler PropertyChanged;

        public ChessPiecePosition Piece {
            get => _piece;
            set {
                if (value.Equals(_piece)) return;

                _piece = value;
                OnPropertyChanged(nameof(Piece));
            }
        }

        public bool IsHovered {
            get => _isHovered;
            set {
                if (value == _isHovered) return;
                _isHovered = value;
                OnPropertyChanged(nameof(IsHovered));
            }
        }

        public bool IsSelected {
            get => _isSelected;
            set {
                if (value == _isSelected) return;
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
                OnPropertyChanged(nameof(ChessViewModel.PossibleMoves));
            }
        }

        public bool IsInCheck {
            get => _isInCheck;
            set {
                if (value == _isInCheck)
                    return;
                _isInCheck = value;
                OnPropertyChanged(nameof(IsInCheck));
            }
        }

        public BoardPosition Position { get; set; }

        private void OnPropertyChanged(string name) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class ChessSquareImageConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null) return null;

            var chessPiecePosition = (ChessPiecePosition) value;

            // Check if we have an empty square
            if (chessPiecePosition.PieceType == ChessPieceType.Empty) return null;

            // If not empty, we can return an image
            var color = chessPiecePosition.Player == 1 ? "white" : "black";
            var piece = chessPiecePosition.PieceType.ToString().ToLower();

            // We only care whether the image is a rook, so fix it in the event we need to
            if (piece.Contains("rook"))
                piece = "rook";

            // Return a new image control
            return new Image {
                Source = new BitmapImage(new Uri(
                    "pack://application:,,,/Cecs475.BoardGames.Chess.View;component/Resources/" +
                    color + piece + ".png"))
            };
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}