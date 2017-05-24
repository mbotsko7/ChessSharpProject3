using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Cecs475.BoardGames.Chess {
    public class ChessBoard : IGameBoard {
        /// <summary>
        /// The number of rows and columns on the chess board.
        /// </summary>
        public const int BOARD_SIZE = 8;

        // Reminder: there are 3 different types of rooks
        private sbyte[,] mBoard = new sbyte[8, 8] {
            {-2, -4, -5, -6, -7, -5, -4, -3}, {-1, -1, -1, -1, -1, -1, -1, -1}, {0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0}, {0, 0, 0, 0, 0, 0, 0, 0}, {0, 0, 0, 0, 0, 0, 0, 0}, {1, 1, 1, 1, 1, 1, 1, 1},
            {2, 4, 5, 6, 7, 5, 4, 3}
        };

        public bool IsFinished => IsCheckmate || IsStalemate;

        public int Weight {
            get {
                int white = 0;
                int black = 0;

                if (IsFinished) {
                    return CurrentPlayer == 1 ? int.MaxValue : int.MinValue;
                }

                if (IsStalemate)
                    return 0;

                // Count spaces each pawn has moved forward
                foreach (var gameMove in MoveHistory) {
                    var move = gameMove as ChessMove;

                    switch (move?.Piece.PieceType) {
                        case ChessPieceType.Pawn:
                            if (move.Piece.Player == 1) {
                                white += Math.Abs(move.StartPosition.Row - move.EndPosition.Row);
                            } else {
                                black += Math.Abs(move.StartPosition.Row - move.EndPosition.Row);
                            }
                            break;
                    }
                }

                var white3PointPieces = new List<BoardPosition>();
                var black3PointPieces = new List<BoardPosition>();
               
                for (var row = 0; row < BOARD_SIZE; row++) {
                    for (var col = 0; col < BOARD_SIZE; col++) {
                        if (Math.Abs(mBoard[row, col]) == 3) {
                            if (mBoard[row, col] > 0) {
                                white3PointPieces.Add(new BoardPosition(row, col));
                            } else {
                                black3PointPieces.Add(new BoardPosition(row, col));
                            }
                        }
                    }
                }

                // Check each position and get the threatened spaces for knights and bishops
                for (var row = 0; row < BOARD_SIZE; row++) {
                    for (var col = 0; col < BOARD_SIZE; col++) {
                        if (mBoard[row, col] > 0) {
                            // Points for ownership
                            white += mBoard[row, col];

                            var threatened = GetPiecesThreatenedFrom(new BoardPosition(row, col)) as List<BoardPosition>;

                            // Points for each knight and bishop protected
                            white += threatened.Union(white3PointPieces).Count();

                            foreach (var boardPosition in threatened) {
                                if (mBoard[boardPosition.Row, boardPosition.Col] >= 0) continue;
                                
                                // Points for each type of enemy threatened
                                switch (GetPieceAtPosition(boardPosition).PieceType) {
                                    case ChessPieceType.RookQueen:
                                    case ChessPieceType.RookKing:
                                    case ChessPieceType.RookPawn:
                                        white += 2;
                                        break;
                                    case ChessPieceType.Knight:
                                    case ChessPieceType.Bishop:
                                        white += 1;
                                        break;
                                    case ChessPieceType.Queen:
                                        white += 5;
                                        break;
                                    case ChessPieceType.King:
                                        white += 4;
                                        break;
                                }
                            }
                        } else if (mBoard[row, col] < 0) {
                            // Points for ownership
                            black += mBoard[row, col] * -1;

                            var threatened = GetPiecesThreatenedFrom(new BoardPosition(row, col)) as List<BoardPosition>;

                            // Points for each knight and bishop protected
                            black += threatened.Union(black3PointPieces).Count();

                            foreach (var boardPosition in threatened) {
                                if (mBoard[boardPosition.Row, boardPosition.Col] <= 0) continue;
                                
                                // Points for each type of enemy threatened
                                switch (GetPieceAtPosition(boardPosition).PieceType) {
                                    case ChessPieceType.RookQueen:
                                    case ChessPieceType.RookKing:
                                    case ChessPieceType.RookPawn:
                                        black += 2;
                                        break;
                                    case ChessPieceType.Knight:
                                    case ChessPieceType.Bishop:
                                        black += 1;
                                        break;
                                    case ChessPieceType.Queen:
                                        black += 5;
                                        break;
                                    case ChessPieceType.King:
                                        black += 4;
                                        break;
                                }
                            }
                        }
                    }
                }

                return white - black;
            }
        }

        // Game states
        public bool NeedToPawnPromote;

        private int _blackKingFirstMove;
        private int _whiteKingFirstMove;
        private int _blackRookKingFirstMove;
        private int _whiteRookKingFirstMove;
        private int _blackRookQueenFirstMove;
        private int _whiteRookQueenFirstMove;

        /// <summary>
        /// Constructs a new chess board with the default starting arrangement.
        /// </summary>
        public ChessBoard() {
            MoveHistory = new List<IGameMove>();

            // Finish any other one-time setup.
            NeedToPawnPromote = false;
            _blackKingFirstMove = -1;
            _whiteKingFirstMove = -1;
            _whiteRookKingFirstMove = -1;
            _blackRookKingFirstMove = -1;
            _whiteRookQueenFirstMove = -1;
            _blackRookQueenFirstMove = -1;
        }

        /// <summary>
        /// Constructs a new chess board by only placing pieces as specified.
        /// </summary>
        /// <param name="startingPositions">a sequence of tuple pairs, where each pair specifies the starting
        /// position of a particular piece to place on the board</param>
        public ChessBoard(IEnumerable<Tuple<BoardPosition, ChessPiecePosition>> startingPositions)
            : this() {
            // NOTE THAT THIS CONSTRUCTOR CALLS YOUR DEFAULT CONSTRUCTOR FIRST
            foreach (int i in Enumerable.Range(0, 8)) {
                // another way of doing for player = 0 to < 8
                foreach (int j in Enumerable.Range(0, 8)) {
                    mBoard[i, j] = 0;
                }
            }
            foreach (var pos in startingPositions) {
                SetPosition(pos.Item1, pos.Item2);
            }

            // Check initial game state
            UpdateGameStateFlags();

            // Update other states
            _whiteRookKingFirstMove = GetPositionsOfPiece(ChessPieceType.RookKing, 1).Any() ? -1 : 1;
            _blackRookKingFirstMove = GetPositionsOfPiece(ChessPieceType.RookKing, 2).Any() ? -1 : 1;
            _whiteRookQueenFirstMove = GetPositionsOfPiece(ChessPieceType.RookQueen, 1).Any() ? -1 : 1;
            _blackRookQueenFirstMove = GetPositionsOfPiece(ChessPieceType.RookQueen, 2).Any() ? -1 : 1;
        }

        private void UpdateGameStateFlags() {
            IsCheck = !GetCheckingPiecePosition().Equals(new BoardPosition(-1, -1));

            if (GetPossibleMoves().Any()) {
                IsCheckmate = false;
                IsStalemate = false;
            } else {
                // No moves: if is check is true, other flags false and checkmate true
                if (IsCheck) {
                    IsCheckmate = true;
                    IsCheck = IsStalemate = false;
                } else {
                    IsStalemate = true;
                    IsCheck = IsCheckmate = false;
                }
            }
        }

        /// <summary>
        /// A difference in piece values for the pieces still controlled by white vs. black, where
        /// a pawn is value 1, a knight and bishop are value 3, a rook is value 5, and a queen is value 9.
        /// </summary>
        public int Value {
            get {
                int val = 0;
                foreach (var t in MoveHistory) {
                    var currentMove = (t as ChessMove);

                    if (currentMove == null) continue;
                    switch (currentMove.MoveType) {
                        case ChessMoveType.Normal:
                        case ChessMoveType.EnPassant:
                            // Check the captured piece and update
                            if (currentMove.Captured.PieceType != ChessPieceType.Empty) {
                                val += GetPieceValue(currentMove.Captured.PieceType) *
                                       (currentMove.Piece.Player == 1 ? 1 : -1);
                            }
                            break;
                        case ChessMoveType.CastleQueenSide:
                        case ChessMoveType.CastleKingSide:
                            break;
                        case ChessMoveType.PawnPromote:
                            // Remove a pawn and add the new piece for the current player
                            val -= GetPieceValue(ChessPieceType.Pawn) *
                                   (currentMove.Piece.Player == 1 ? 1 : -1);
                            val += GetPieceValue(currentMove.Piece.PieceType) *
                                   (currentMove.Piece.Player == 1 ? 1 : -1);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                return val;
            }
        }

        public int PawnPromotion => MoveHistory.Count(move => {
            // Sanity check
            var chessMove = move as ChessMove;
            return chessMove != null && chessMove.MoveType == ChessMoveType.PawnPromote;
        });

        public int Orientation => CurrentPlayer == 1 ? 1 : -1;

        public int CurrentPlayer => (MoveHistory.Count - PawnPromotion - (NeedToPawnPromote ? 1 : 0)) % 2 + 1;

        // An auto-property suffices here.
        public IList<IGameMove> MoveHistory { get; }

        public bool IsCheck { get; private set; }

        public bool IsCheckmate { get; private set; }

        public bool IsStalemate { get; private set; }

        /// <summary>
        /// Returns the piece and player at the given position on the board.
        /// </summary>
        public ChessPiecePosition GetPieceAtPosition(BoardPosition position) {
            var boardVal = mBoard[position.Row, position.Col];
            return new ChessPiecePosition((ChessPieceType) Math.Abs(mBoard[position.Row, position.Col]),
                boardVal > 0 ? 1 : boardVal < 0 ? 2 : 0);
        }

        public void ApplyMove(IGameMove move) {
            var aMove = move as ChessMove;
            if (aMove == null) throw new ArgumentNullException(nameof(aMove));

            // Set move members
            aMove.Piece = GetPieceAtPosition(aMove.StartPosition);

            // Remove the piece from its starting position
            mBoard[aMove.StartPosition.Row, aMove.StartPosition.Col] = 0;
            
            // Set it at the end position
            if (aMove.MoveType != ChessMoveType.PawnPromote) {
                // Record the captured piece, if any
                if (MoveHistory.Count >= 7) {
                    Debug.WriteLine(aMove);
                    Debug.WriteLine(MoveHistory[6]);
                }
                aMove.Captured = GetPieceAtPosition(aMove.EndPosition);
                
                mBoard[aMove.EndPosition.Row, aMove.EndPosition.Col] =
                    (sbyte) ((int) aMove.Piece.PieceType * Orientation);
            }

            // Do the move, this is different according to move type
            switch (aMove.MoveType) {
                case ChessMoveType.Normal:
                    // Check if the move will allow us to promote a pawn
                    switch (aMove.Piece.PieceType) {
                        case ChessPieceType.Empty:
                            break;
                        case ChessPieceType.Pawn:
                            if (aMove.EndPosition.Row == 0 || aMove.EndPosition.Row == 7) {
                                NeedToPawnPromote = true;
                            }
                            break;
                        case ChessPieceType.RookQueen:
                            if (CurrentPlayer == 1 && _blackRookQueenFirstMove == -1) {
                                _whiteRookQueenFirstMove = MoveHistory.Count + 1;
                            } else if (CurrentPlayer == 2 && _blackRookQueenFirstMove == -1) {
                                _blackRookQueenFirstMove = MoveHistory.Count + 1;
                            }
                            break;
                        case ChessPieceType.RookKing:
                            if (CurrentPlayer == 1 && _whiteRookKingFirstMove == -1) {
                                _whiteRookKingFirstMove = MoveHistory.Count + 1;
                            } else if (CurrentPlayer == 2 && _blackRookKingFirstMove == -1) {
                                _blackRookKingFirstMove = MoveHistory.Count + 1;
                            }
                            break;
                        case ChessPieceType.RookPawn:
                            break;
                        case ChessPieceType.Knight:
                            break;
                        case ChessPieceType.Bishop:
                            break;
                        case ChessPieceType.Queen:
                            break;
                        case ChessPieceType.King:
                            if (CurrentPlayer == 1 && _whiteKingFirstMove == -1) {
                                _whiteKingFirstMove = MoveHistory.Count + 1;
                            } else if (CurrentPlayer == 2 && _blackKingFirstMove == -1) {
                                _blackKingFirstMove = MoveHistory.Count + 1;
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                case ChessMoveType.CastleQueenSide:
                    // Move king two moves to the left and the rook to the right of it
                    mBoard[aMove.EndPosition.Row, aMove.EndPosition.Col + 1] =
                        (sbyte) ((int) ChessPieceType.RookQueen * Orientation);

                    // Clear the old rook position
                    mBoard[aMove.EndPosition.Row, aMove.EndPosition.Col - 2] = 0;

                    // Set flags
                    if (CurrentPlayer == 1) {
                        _whiteKingFirstMove = MoveHistory.Count + 1;
                        _whiteRookQueenFirstMove = MoveHistory.Count + 1;
                    } else {
                        _blackKingFirstMove = MoveHistory.Count + 1;
                        _blackRookQueenFirstMove = MoveHistory.Count + 1;
                    }
                    break;
                case ChessMoveType.CastleKingSide:
                    // Move king two moves to the right and the rook to the left of it
                    mBoard[aMove.EndPosition.Row, aMove.EndPosition.Col - 1] =
                        (sbyte) ((int) ChessPieceType.RookKing * Orientation);

                    // Clear the old rook position
                    mBoard[aMove.EndPosition.Row, aMove.EndPosition.Col + 1] = 0;
                    

                    // Set flags
                    if (aMove.Piece.Player == 1) {
                        _whiteKingFirstMove = MoveHistory.Count + 1;
                        _whiteRookKingFirstMove = MoveHistory.Count + 1;
                    } else {
                        _blackKingFirstMove = MoveHistory.Count + 1;
                        _blackRookKingFirstMove = MoveHistory.Count + 1;
                    }
                    break;
                case ChessMoveType.EnPassant:
                    // Set the captured piece to the pawn
                    aMove.Captured = GetPieceAtPosition(aMove.EndPosition.Translate(Orientation, 0));

                    // Remove the enemy pawn captured
                    mBoard[aMove.EndPosition.Row + Orientation, aMove.EndPosition.Col] = (sbyte) ChessPieceType.Empty;
                    break;
                case ChessMoveType.PawnPromote:
                    // Pawns can be promoted to the type matching its column
                    mBoard[aMove.StartPosition.Row, aMove.StartPosition.Col] =
                        (sbyte) (aMove.EndPosition.Col * Orientation);

                    // Reset flag
                    NeedToPawnPromote = false;

                    // Update what the piece is
                    aMove.Piece = GetPieceAtPosition(aMove.StartPosition);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Update our other members
            MoveHistory.Add(aMove);

            // Check if game state changes
            UpdateGameStateFlags();
        }

        public void UndoLastMove() {
            // Get the move from history and remove it
            var move = MoveHistory.Last() as ChessMove;

            // Sanity
            if (move == null) return;

            if (NeedToPawnPromote)
                NeedToPawnPromote = false;

            // Remove the pawn promotion and undo the move before it
            if (move.MoveType == ChessMoveType.PawnPromote) {
                // Remove the pawn promote
                MoveHistory.RemoveAt(MoveHistory.Count - 1);

                // Replace position with a pawn
                mBoard[move.StartPosition.Row, move.StartPosition.Col] =
                    (sbyte) ((int) (ChessPieceType.Pawn) * Orientation);

                // Set the promote flag again
                NeedToPawnPromote = true;

                // Check if game state changes
                UpdateGameStateFlags();
                return;
            }

            // Remove the piece from its ending position
            mBoard[move.EndPosition.Row, move.EndPosition.Col] = (sbyte) ChessPieceType.Empty;

            // Update our other members
            MoveHistory.RemoveAt(MoveHistory.Count - 1);

            // Replace the piece at the starting location
            mBoard[move.StartPosition.Row, move.StartPosition.Col] =
                (sbyte) ((int) move.Piece.PieceType * Orientation);

            // Undo the move, this is different according to move type
            switch (move.MoveType) {
                case ChessMoveType.Normal:
                    // In case the piece was captured, replace it
                    mBoard[move.EndPosition.Row, move.EndPosition.Col] =
                        (sbyte) ((int) (move.Captured.PieceType) * -Orientation);

                    // Reset flags
                    if (_whiteKingFirstMove == MoveHistory.Count + 1)
                        _whiteKingFirstMove = -1;
                    else if (_blackKingFirstMove == MoveHistory.Count + 1)
                        _blackKingFirstMove = -1;
                    else if (_whiteRookQueenFirstMove == MoveHistory.Count + 1)
                        _whiteRookQueenFirstMove = -1;
                    else if (_whiteRookKingFirstMove == MoveHistory.Count + 1)
                        _whiteRookKingFirstMove = -1;
                    else if (_blackRookQueenFirstMove == MoveHistory.Count + 1)
                        _blackRookQueenFirstMove = -1;
                    else if (_blackRookKingFirstMove == MoveHistory.Count + 1)
                        _blackRookKingFirstMove = -1;
                    break;
                case ChessMoveType.CastleQueenSide:
                    // Move king two moves to the right and the rook to the starting position
                    mBoard[move.EndPosition.Row, move.EndPosition.Col - 2] =
                        (sbyte) ((int) (ChessPieceType.RookQueen) * Orientation);

                    // Clear the old rook position
                    mBoard[move.EndPosition.Row, move.EndPosition.Col + 1] = (sbyte) ChessPieceType.Empty;

                    // Reset first move
                    if (_whiteRookQueenFirstMove == MoveHistory.Count + 1 &&
                        _whiteKingFirstMove == MoveHistory.Count + 1) {
                        _whiteRookQueenFirstMove = -1;
                        _whiteKingFirstMove = -1;
                    } else if (_blackRookQueenFirstMove == MoveHistory.Count + 1 &&
                               _blackKingFirstMove == MoveHistory.Count + 1) {
                        _blackRookQueenFirstMove = -1;
                        _blackKingFirstMove = -1;
                    }
                    break;
                case ChessMoveType.CastleKingSide:
                    // Move king to moves to the right and the rook to the left of it
                    mBoard[move.EndPosition.Row, move.EndPosition.Col + 1] =
                        (sbyte) ((int) (ChessPieceType.RookKing) * Orientation);

                    // Clear the old rook position
                    mBoard[move.EndPosition.Row, move.EndPosition.Col - 1] = (sbyte) ChessPieceType.Empty;

                    // Reset first move
                    if (_whiteRookKingFirstMove == MoveHistory.Count + 1 &&
                        _whiteKingFirstMove == MoveHistory.Count + 1) {
                        _whiteRookKingFirstMove = -1;
                        _whiteKingFirstMove = -1;
                    } else if (_blackRookKingFirstMove == MoveHistory.Count + 1 &&
                               _blackKingFirstMove == MoveHistory.Count + 1) {
                        _blackRookKingFirstMove = -1;
                        _blackKingFirstMove = -1;
                    }
                    break;
                case ChessMoveType.EnPassant:
                    // Replace the captured enemy pawn
                    mBoard[move.EndPosition.Row + Orientation, move.EndPosition.Col] =
                        (sbyte) ((int) (ChessPieceType.Pawn) * -Orientation);
                    break;
                case ChessMoveType.PawnPromote:
                    // Replace position with a pawn
                    mBoard[move.StartPosition.Row, move.StartPosition.Col] =
                        (sbyte) ((int) (ChessPieceType.Pawn) * Orientation);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Check if game state changes
            UpdateGameStateFlags();
        }

        public IEnumerable<IGameMove> GetPossibleMoves() {
            // Create our list
            if (IsCheckmate)
                return new List<ChessMove>();

            var possibleMoves = new List<ChessMove>();
            var blockMoves = new List<BoardPosition>();
            var lastMove = MoveHistory.LastOrDefault() as ChessMove;

            // Pawn promotion
            if (lastMove != null && NeedToPawnPromote) {
                possibleMoves.Add(new ChessMove(lastMove.EndPosition,
                    new BoardPosition(-1, (int) ChessPieceType.Bishop), ChessMoveType.PawnPromote));
                possibleMoves.Add(new ChessMove(lastMove.EndPosition,
                    new BoardPosition(-1, (int) ChessPieceType.Knight), ChessMoveType.PawnPromote));
                possibleMoves.Add(new ChessMove(lastMove.EndPosition,
                    new BoardPosition(-1, (int) ChessPieceType.Queen), ChessMoveType.PawnPromote));
                possibleMoves.Add(new ChessMove(lastMove.EndPosition,
                    new BoardPosition(-1, (int) ChessPieceType.RookPawn), ChessMoveType.PawnPromote));
                return possibleMoves;
            }

            // Get the king position of the current player
            var kingPosition = GetPositionsOfPiece(ChessPieceType.King, CurrentPlayer).FirstOrDefault();

            // Reference the threatened spaces by the enemy
            var threatenedSpaces = GetThreatenedPositions(CurrentPlayer + Orientation).ToList();

            // If we are in check we have get the moves we are allowed to make
            if (IsCheck) {
                blockMoves.AddRange(GetLimitedMoves(kingPosition));

                var attacker = GetCheckingPiecePosition();

                if (!attacker.Equals(new BoardPosition(-1, -1))) {
                    blockMoves.Add(attacker);
                }
            }

            // Check all possible spaces
            for (var row = 0; row < BOARD_SIZE; row++) {
                for (var col = 0; col < BOARD_SIZE; col++) {
                    // Save current position and player
                    var currentPosition = new BoardPosition(row, col);
                    var currentPiece = GetPieceAtPosition(currentPosition);
                    bool isPinned = false;


                    // Check if piece belongs to the current player
                    if (currentPiece.Player != CurrentPlayer)
                        continue;

                    if (!IsCheck && threatenedSpaces.Contains(currentPosition) &&
                        !currentPosition.Equals(kingPosition)) {
                        // Remove piece
                        mBoard[row, col] = (sbyte) ChessPieceType.Empty;

                        // Need to get positions again
                        isPinned = GetThreatenedPositions(CurrentPlayer + Orientation).Contains(kingPosition);
                        if (isPinned) {
                            blockMoves.AddRange(GetLimitedMoves(kingPosition));
                            var attacker = GetCheckingPiecePosition();

                            if (!attacker.Equals(new BoardPosition(-1, -1))) {
                                blockMoves.Add(attacker);
                            }
                        }

                        // Replace
                        mBoard[row, col] = (sbyte) ((int) currentPiece.PieceType * Orientation);
                    }

                    // If the king is in check or a piece is pinned, we have limited movement
                    if (isPinned) {
                        // Get our available moves
                        var checkMoves = GetPossibleMovesInCheck(currentPosition, blockMoves);

                        // Add them to possible moves and continue to next piece
                        possibleMoves.AddRange(
                            checkMoves.Select(blockPosition => new ChessMove(currentPosition, blockPosition)));
                        continue;
                    }

                    // Get the non-special available moves
                    var currentPositionThreats = GetPiecesThreatenedFrom(currentPosition).ToList();

                    // Otherwise, just add the available moves given by getThreatenedPositions
                    // and consider special rules for king / pawn
                    switch (GetPieceAtPosition(currentPosition).PieceType) {
                        case ChessPieceType.Empty:
                            // Don't do anything
                            break;
                        case ChessPieceType.Pawn:
                            // Adjust movement based on white or black
                            var dRow = -Orientation;
                            var pawnMoves = new List<ChessMove>();
                            if ((row != 7 && CurrentPlayer == 2) || (row != 0 && CurrentPlayer == 1)) {
                                // Normal
                                var forwardOne = currentPosition.Translate(dRow, 0);
                                var forwardTwo = currentPosition.Translate(dRow * 2, 0);
                                if (PositionIsEmpty(forwardOne))
                                    pawnMoves.Add(new ChessMove(currentPosition, currentPosition.Translate(dRow, 0)));

                                // If it's the pawn's first move it can move up two spaces
                                if (((CurrentPlayer == 1 && row == 6) || (CurrentPlayer == 2 && row == 1)) &&
                                    GetPieceAtPosition(forwardOne).PieceType == ChessPieceType.Empty &&
                                    GetPieceAtPosition(forwardTwo).PieceType == ChessPieceType.Empty)
                                    pawnMoves.Add(
                                        new ChessMove(currentPosition, currentPosition.Translate(dRow * 2, 0)));

                                // Left capture
                                if (col != 0 && PositionIsEnemy(currentPosition.Translate(dRow, -1), CurrentPlayer))
                                    pawnMoves.Add(
                                        new ChessMove(currentPosition, currentPosition.Translate(dRow, -1)));

                                // Right capture
                                if (col != 7 && PositionIsEnemy(currentPosition.Translate(dRow, 1), CurrentPlayer))
                                    pawnMoves.Add(
                                        new ChessMove(currentPosition, currentPosition.Translate(dRow, 1)));

                                // Enpassant
                                if ((CurrentPlayer == 1 && row == 3) || (CurrentPlayer == 2 && row == 4)) {
                                    // Get the left and right pawn movements that could allow this
                                    var passRight = new ChessMove(currentPosition.Translate(dRow * 2, 1),
                                        currentPosition.Translate(0, 1));
                                    var passLeft = new ChessMove(currentPosition.Translate(dRow * 2, -1),
                                        currentPosition.Translate(0, -1));

                                    // Check if the last move is the same
                                    if (lastMove != null) {
                                        if (lastMove.Equals(passRight)) {
                                            pawnMoves.Add(new ChessMove(currentPosition,
                                                currentPosition.Translate(dRow, 1), ChessMoveType.EnPassant));
                                        } else if (lastMove.Equals(passLeft)) {
                                            pawnMoves.Add(new ChessMove(currentPosition,
                                                currentPosition.Translate(dRow, -1), ChessMoveType.EnPassant));
                                        }
                                    }
                                }
                                if (IsCheck)
                                    blockMoves.Add(GetCheckingPiecePosition());

                                possibleMoves.AddRange(!IsCheck
                                    ? pawnMoves
                                    : pawnMoves.Where(move => blockMoves.Contains(move.EndPosition)));
                            }
                            break;
                        case
                        ChessPieceType.RookQueen:
                        case
                        ChessPieceType.RookKing:
                        case
                        ChessPieceType.RookPawn:
                        case
                        ChessPieceType.Knight:
                        case
                        ChessPieceType.Bishop:
                        case
                        ChessPieceType.Queen:
                            if (IsCheck) {
                                possibleMoves.AddRange(currentPositionThreats.Intersect(blockMoves)
                                    .Where(move => GetPieceAtPosition(move).Player != currentPiece.Player)
                                    .Select(move => new ChessMove(currentPosition, move)));
                                continue;
                            }

                            // Normal (includes capture)
                            possibleMoves.AddRange(currentPositionThreats
                                .Where(move => GetPieceAtPosition(move).Player != currentPiece.Player)
                                .Select(move => new ChessMove(currentPosition, move)));
                            break;
                        case
                        ChessPieceType.King:
                            if (IsCheck) {
                                mBoard[row, col] = (sbyte) ChessPieceType.Empty;
                                possibleMoves.AddRange(
                                    currentPositionThreats
                                        .Except(GetThreatenedPositions(CurrentPlayer + Orientation))
                                        .Where(pos => GetPieceAtPosition(pos).Player != CurrentPlayer)
                                        .Select(pos => new ChessMove(currentPosition, pos)));
                                mBoard[row, col] = (sbyte) ((int) ChessPieceType.King * Orientation);
                                continue;
                            }

                            // Normal (includes capture)
                            possibleMoves.AddRange(currentPositionThreats.Except(threatenedSpaces)
                                .Where(pos => GetPieceAtPosition(pos).Player != CurrentPlayer)
                                .Select(move => new ChessMove(currentPosition, move)));

                            // Castle: Check king passing through check, empty between and flags
                            if (!threatenedSpaces.Contains(currentPosition.Translate(0, -2)) &&
                                !threatenedSpaces.Contains(currentPosition.Translate(0, -1)) &&
                                IsEmptyBetween(currentPosition,
                                    GetPositionsOfPiece(ChessPieceType.RookQueen, CurrentPlayer)
                                        .FirstOrDefault())) {
                                // Queen side
                                if (CurrentPlayer == 1) {
                                    if (_whiteRookQueenFirstMove == -1 && _whiteKingFirstMove == -1) {
                                        possibleMoves.Add(new ChessMove(currentPosition,
                                            currentPosition.Translate(0, -2),
                                            ChessMoveType.CastleQueenSide));
                                    }
                                } else {
                                    if (_blackRookQueenFirstMove == -1 && _blackKingFirstMove == -1) {
                                        possibleMoves.Add(new ChessMove(currentPosition,
                                            currentPosition.Translate(0, -2),
                                            ChessMoveType.CastleQueenSide));
                                    }
                                }
                            }
                            if (!threatenedSpaces.Contains(currentPosition.Translate(0, 2)) &&
                                !threatenedSpaces.Contains(currentPosition.Translate(0, 1)) &&
                                IsEmptyBetween(currentPosition,
                                    GetPositionsOfPiece(ChessPieceType.RookKing, CurrentPlayer)
                                        .FirstOrDefault())) {
                                // King side
                                if (CurrentPlayer == 1) {
                                    if (_whiteRookKingFirstMove == -1 && _whiteKingFirstMove == -1) {
                                        possibleMoves.Add(new ChessMove(currentPosition,
                                            currentPosition.Translate(0, 2),
                                            ChessMoveType.CastleKingSide));
                                    }
                                } else {
                                    if (_blackRookKingFirstMove == -1 && _blackKingFirstMove == -1) {
                                        possibleMoves.Add(new ChessMove(currentPosition,
                                            currentPosition.Translate(0, 2),
                                            ChessMoveType.CastleKingSide));
                                    }
                                }
                            }
                            break;
                        default:
                            // Can't be but just in case
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            // Return the moves
            return possibleMoves;
        }

        private IEnumerable<BoardPosition> GetLimitedMoves(BoardPosition kingPosition) {
            var blockMoves = new List<BoardPosition>();
            var attackingPiece = GetCheckingPiecePosition();
            int dCol = attackingPiece.Col.CompareTo(kingPosition.Col);
            int dRow = attackingPiece.Row.CompareTo(kingPosition.Row);
            int spaces = 0;

            // Get the number of spaces between king and checkingPiece
            switch (GetPieceAtPosition(attackingPiece).PieceType) {
                case ChessPieceType.Empty:
                    // Can't be
                    break;
                case ChessPieceType.Pawn:
                    // No spaces between
                    break;
                case ChessPieceType.RookQueen:
                case ChessPieceType.RookKing:
                case ChessPieceType.RookPawn:
                    spaces = dRow == 0
                        ? Math.Abs(attackingPiece.Col - kingPosition.Col)
                        : Math.Abs(attackingPiece.Row - kingPosition.Row);
                    break;
                case ChessPieceType.Knight:
                    // No line of sight
                    break;
                case ChessPieceType.Bishop:
                    spaces = Math.Abs(attackingPiece.Row - kingPosition.Row);
                    break;
                case ChessPieceType.Queen:
                    if (dRow == 0 || dCol == 0)
                        goto case ChessPieceType.RookQueen;
                    else {
                        goto case ChessPieceType.Bishop;
                    }
                case ChessPieceType.King:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Add the spaces between
            for (int i = 1; i < spaces; i++) {
                blockMoves.Add(kingPosition.Translate(dRow * i, dCol * i));
            }

            return blockMoves;
        }

        private BoardPosition GetCheckingPiecePosition() {
            var kingPosition = GetPositionsOfPiece(ChessPieceType.King, CurrentPlayer).FirstOrDefault();
            for (int row = 0; row < BOARD_SIZE; row++) {
                for (int col = 0; col < BOARD_SIZE; col++) {
                    var currentPosition = new BoardPosition(row, col);
                    if (GetPlayerAtPosition(currentPosition) == (CurrentPlayer + Orientation) &&
                        GetPiecesThreatenedFrom(currentPosition).Contains(kingPosition)) {
                        return currentPosition;
                    }
                }
            }
            return new BoardPosition(-1, -1);
        }

        private IEnumerable<BoardPosition> GetPiecesThreatenedFrom(BoardPosition pos) {
            var currentPiece = GetPieceAtPosition(pos);

            switch (currentPiece.PieceType) {
                case ChessPieceType.Empty:
                    return new List<BoardPosition>();
                case ChessPieceType.Pawn:
                    return GetThreatenedPositions_Pawn(pos);
                case ChessPieceType.RookQueen:
                case ChessPieceType.RookKing:
                case ChessPieceType.RookPawn:
                    return GetThreatenedPositions_Rook(pos);
                case ChessPieceType.Knight:
                    return GetThreatenedPositions_Knight(pos);
                case ChessPieceType.Bishop:
                    return GetThreatenedPositions_Bishop(pos);
                case ChessPieceType.Queen:
                    return GetThreatenedPositions_Queen(pos);
                case ChessPieceType.King:
                    return GetThreatenedPositions_King(pos);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool IsEmptyBetween(BoardPosition kingPosition, BoardPosition rookPosition) {
            // Get the direction
            var dCol = rookPosition.Col.CompareTo(kingPosition.Col);

            // Loop through spaces between king / rook
            for (int i = 1; i < Math.Abs(rookPosition.Col - kingPosition.Col); i++) {
                if (!PositionIsEmpty(kingPosition.Translate(0, dCol * i))) {
                    return false;
                }
            }

            // If all spaces are empty
            return true;
        }

        private IEnumerable<BoardPosition> GetPossibleMovesInCheck(BoardPosition startPosition,
            List<BoardPosition> blockMoves) {
            // The attacking piece can be captured
            // Or block the line of sight between king and attacking piece
            switch (GetPieceAtPosition(startPosition).PieceType) {
                case ChessPieceType.Empty:
                    // Can't be this if we're in check
                    break;
                case ChessPieceType.Pawn:
                    var possibleBlocks =
                        GetThreatenedPositions_Pawn(startPosition).Intersect(blockMoves) as List<BoardPosition> ??
                        new List<BoardPosition>();

                    // Add forward move, if it blocks them
                    int dRow = -Orientation;
                    var forward1 = startPosition.Translate(dRow, 0);
                    var forward2 = startPosition.Translate(dRow * 2, 0);
                    if (blockMoves.Contains(forward1)) {
                        possibleBlocks?.Add(forward1);
                    } else if (blockMoves.Contains(forward2) && (CurrentPlayer == 1 && startPosition.Row == 6) ||
                               (CurrentPlayer == 2 && startPosition.Row == 1)) {
                        possibleBlocks?.Add(forward2);
                    }
                    return possibleBlocks;
                case ChessPieceType.RookQueen:
                case ChessPieceType.RookKing:
                case ChessPieceType.RookPawn:
                    return GetThreatenedPositions_Rook(startPosition).Intersect(blockMoves);
                case ChessPieceType.Knight:
                    return GetThreatenedPositions_Knight(startPosition).Intersect(blockMoves);
                case ChessPieceType.Bishop:
                    return GetThreatenedPositions_Bishop(startPosition).Intersect(blockMoves);
                case ChessPieceType.Queen:
                    return GetThreatenedPositions_Queen(startPosition).Intersect(blockMoves);
                case ChessPieceType.King:
                    return GetThreatenedPositions_King(startPosition).Except(blockMoves);
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return null;
        }

        /// <summary>
        /// Gets a sequence of all positions on the board that are threatened by the given player. A king
        /// may not move to a square threatened by the opponent.
        /// </summary>
        public IEnumerable<BoardPosition> GetThreatenedPositions(int byPlayer) {
            // List to append to
            var positions = new List<BoardPosition>();

            // Iterate over board games
            for (var row = 0; row < BOARD_SIZE; row++) {
                for (var col = 0; col < BOARD_SIZE; col++) {
                    // Save our current information
                    var currentPosition = new BoardPosition(row, col);
                    var currentPiece = GetPieceAtPosition(currentPosition);

                    // Skip if the piece is empty or belongs to a different player
                    if (currentPiece.Player != byPlayer)
                        continue;

                    // Add the pieces threatened by that position
                    positions.AddRange(GetPiecesThreatenedFrom(currentPosition));
                }
            }

            // Return the new list
            return positions;
        }

        /// <summary>
        /// Checks the threatened positions for the king by checking one space over in all directions
        /// </summary>
        /// <param name="startPosition">the location of the piece</param>
        /// <param name="byPlayer">the player making the move</param>
        private IEnumerable<BoardPosition> GetThreatenedPositions_King(BoardPosition startPosition) {
            var positions = new List<BoardPosition>();
            var movePosition = new BoardPosition(0, 0);

            for (int moves = 0; moves < 8; moves++) {
                // Simulate movement
                switch (moves) {
                    case 0:
                        movePosition = startPosition.Translate(1, 1);
                        break;
                    case 1:
                        movePosition = startPosition.Translate(1, 0);
                        break;
                    case 2:
                        movePosition = startPosition.Translate(1, -1);
                        break;
                    case 3:
                        movePosition = startPosition.Translate(0, -1);
                        break;
                    case 4:
                        movePosition = startPosition.Translate(-1, -1);
                        break;
                    case 5:
                        movePosition = startPosition.Translate(-1, 0);
                        break;
                    case 6:
                        movePosition = startPosition.Translate(-1, 1);
                        break;
                    case 7:
                        movePosition = startPosition.Translate(0, 1);
                        break;
                }

                AddThreatenedPosition(movePosition, ref positions);
            }
            return positions;
        }

        /// <summary>
        /// Checks the threatened positions for the queen by checking as many spaces over in all directions
        /// </summary>
        /// <param name="startPosition">the location of the piece</param>
        /// <param name="byPlayer">the player making the move</param>
        private IEnumerable<BoardPosition> GetThreatenedPositions_Queen(BoardPosition startPosition) {
            var positions = new List<BoardPosition>();

            positions.AddRange(GetThreatenedPositions_Bishop(startPosition));
            positions.AddRange(GetThreatenedPositions_Rook(startPosition));

            return positions;
        }

        /// <summary>
        /// Checks the threatened positions for the bishop by checking as many spaces over in diagonals
        /// </summary>
        /// <param name="startPosition">the location of the piece</param>
        /// <param name="byPlayer">the player making the move</param>
        private IEnumerable<BoardPosition> GetThreatenedPositions_Bishop(BoardPosition startPosition) {
            var positions = new List<BoardPosition>();

            for (int moves = 0; moves < 4; moves++) {
                for (int i = 1; i < BOARD_SIZE; i++) {
                    // Simulate movement
                    BoardPosition movePosition = new BoardPosition(0, 0);
                    switch (moves) {
                        case 0:
                            movePosition = startPosition.Translate(i, i);
                            break;
                        case 1:
                            movePosition = startPosition.Translate(-i, -i);
                            break;
                        case 2:
                            movePosition = startPosition.Translate(i, -i);
                            break;
                        case 3:
                            movePosition = startPosition.Translate(-i, i);
                            break;
                    }

                    if (AddThreatenedPosition(movePosition, ref positions)) break;
                }
            }

            return positions;
        }

        /// <summary>
        /// Checks the threatened positions for the knight by checking the eight possible move positions
        /// </summary>
        /// <param name="startPosition">the location of the piece</param>
        /// <param name="byPlayer">the player making the move</param>
        private IEnumerable<BoardPosition> GetThreatenedPositions_Knight(BoardPosition startPosition) {
            var positions = new List<BoardPosition>();

            for (int moves = 0; moves < 8; moves++) {
                // Simulate movement
                BoardPosition movePosition = new BoardPosition(0, 0);
                switch (moves) {
                    case 0:
                        movePosition = startPosition.Translate(2, 1);
                        break;
                    case 1:
                        movePosition = startPosition.Translate(2, -1);
                        break;
                    case 2:
                        movePosition = startPosition.Translate(-2, 1);
                        break;
                    case 3:
                        movePosition = startPosition.Translate(-2, -1);
                        break;
                    case 4:
                        movePosition = startPosition.Translate(1, 2);
                        break;
                    case 5:
                        movePosition = startPosition.Translate(1, -2);
                        break;
                    case 6:
                        movePosition = startPosition.Translate(-1, 2);
                        break;
                    case 7:
                        movePosition = startPosition.Translate(-1, -2);
                        break;
                }

                AddThreatenedPosition(movePosition, ref positions);
            }
            return positions;
        }

        /// <summary>
        /// Checks the threatened positions by the rook by checking as many spaces over vertically / horizontally
        /// </summary>
        /// <param name="startPosition">the location of the piece</param>
        /// <param name="byPlayer">the player making the move</param>
        private IEnumerable<BoardPosition> GetThreatenedPositions_Rook(BoardPosition startPosition) {
            var positions = new List<BoardPosition>();

            for (int moves = 0; moves < 4; moves++) {
                for (int i = 1; i < BOARD_SIZE; i++) {
                    // Simulate movement
                    BoardPosition movePosition = new BoardPosition(0, 0);
                    switch (moves) {
                        case 0:
                            movePosition = startPosition.Translate(i, 0);
                            break;
                        case 1:
                            movePosition = startPosition.Translate(-i, 0);
                            break;
                        case 2:
                            movePosition = startPosition.Translate(0, i);
                            break;
                        case 3:
                            movePosition = startPosition.Translate(0, -i);
                            break;
                    }

                    if (AddThreatenedPosition(movePosition, ref positions)) break;
                }
            }
            return positions;
        }

        /// <summary>
        /// Checks the threatened positions for the pawn by checking diagonals in the opposite the "sign" of the player
        /// </summary>
        /// <param name="startPosition">the location of the piece</param>
        /// <param name="byPlayer">the player making the move</param>
        private IEnumerable<BoardPosition> GetThreatenedPositions_Pawn(BoardPosition startPosition) {
            var positions = new List<BoardPosition>();

            // Get direction pawn is traveling
            var dRow = GetPlayerAtPosition(startPosition) == 1 ? -1 : 1;

            // First forward diagonals w/ respect to player
            AddThreatenedPosition(startPosition.Translate(dRow, -1), ref positions);
            AddThreatenedPosition(startPosition.Translate(dRow, 1), ref positions);

            return positions;
        }

        /// <summary>
        /// Adds the movePosition to positions if the location is threatened. Returns true to signal control to stop
        /// </summary>
        private bool AddThreatenedPosition(BoardPosition movePosition, ref List<BoardPosition> positions) {
            // Check if move in bounds
            if (!PositionInBounds(movePosition)) return true;

            // In bounds, check if move is on a blank, enemy or friendly space
            if (PositionIsEmpty(movePosition)) {
                // Empty: threatened
                positions.Add(movePosition);
            } else {
                // Enemy / Friendly
                positions.Add(movePosition);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if the given position on the board is empty.
        /// </summary>
        /// <remarks>returns false if the position is not in bounds</remarks>
        public bool PositionIsEmpty(BoardPosition pos) {
            return GetPieceAtPosition(pos).PieceType == ChessPieceType.Empty;
        }

        /// <summary>
        /// Returns true if the given position contains a piece that is the enemy of the given player.
        /// </summary>
        /// <remarks>returns false if the position is not in bounds</remarks>
        public bool PositionIsEnemy(BoardPosition pos, int player) {
            return GetPlayerAtPosition(pos) != player && GetPlayerAtPosition(pos) != 0;
        }

        /// <summary>
        /// Returns true if the given position is in the bounds of the board.
        /// </summary>
        public static bool PositionInBounds(BoardPosition pos) {
            return 0 <= pos.Row && pos.Row < 8 && 0 <= pos.Col && pos.Col < 8;
        }

        /// <summary>
        /// Returns which player has a piece at the given board position, or 0 if it is empty.
        /// </summary>
        public int GetPlayerAtPosition(BoardPosition pos) {
            return GetPieceAtPosition(pos).Player;
        }

        /// <summary>
        /// Gets the value weight for a piece of the given type.
        /// </summary>
        /*
         * VALUES:
         * Pawn: 1
         * Knight: 3
         * Bishop: 3
         * Rook: 5
         * Queen: 9
         * King: infinity (maximum integer value)
         */
        public int GetPieceValue(ChessPieceType pieceType) {
            switch (pieceType) {
                case ChessPieceType.Empty:
                    return 0;
                case ChessPieceType.Pawn:
                    return 1;
                case ChessPieceType.RookQueen:
                    return 5;
                case ChessPieceType.RookKing:
                    return 5;
                case ChessPieceType.RookPawn:
                    return 5;
                case ChessPieceType.Knight:
                    return 3;
                case ChessPieceType.Bishop:
                    return 3;
                case ChessPieceType.Queen:
                    return 9;
                case ChessPieceType.King:
                    return Int32.MaxValue;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pieceType), pieceType, null);
            }
        }

        /// <summary>
        /// Manually places the given piece at the given position.
        /// </summary>
        // This is used in the constructor
        private void SetPosition(BoardPosition position, ChessPiecePosition piece) {
            mBoard[position.Row, position.Col] = (sbyte) ((int) piece.PieceType *
                                                          (piece.Player == 2 ? -1 : piece.Player));
        }

        public IEnumerable<BoardPosition> GetPositionsOfPiece(ChessPieceType piece, int player) {
            var possible = new List<BoardPosition>();

            for (int row = 0; row < BOARD_SIZE; row++) {
                for (int col = 0; col < BOARD_SIZE; col++) {
                    var position = new BoardPosition(row, col);
                    if (GetPieceAtPosition(position).PieceType == piece &&
                        GetPieceAtPosition(position).Player == player)
                        possible.Add(position);
                }
            }

            return possible;
        }

        public override string ToString() {
            string output = "";
            string[] PIECES = new string[9];
            PIECES[(int) ChessPieceType.Empty] = ".";
            PIECES[(int) ChessPieceType.Bishop] = "B";
            PIECES[(int) ChessPieceType.King] = "K";
            PIECES[(int) ChessPieceType.Knight] = "N";
            PIECES[(int) ChessPieceType.Pawn] = "P";
            PIECES[(int) ChessPieceType.Queen] = "Q";
            PIECES[(int) ChessPieceType.RookKing] = "R";
            PIECES[(int) ChessPieceType.RookQueen] = "R";
            PIECES[(int) ChessPieceType.RookPawn] = "R";

            foreach (int i in Enumerable.Range(0, 8)) {
                output += $"{i} ";
                foreach (int j in Enumerable.Range(0, 8)) {
                    var piece = GetPieceAtPosition(new BoardPosition(i, j));
                    char letter = Convert.ToChar(PIECES[(int) piece.PieceType]);
                    output += piece.Player == 1 ? letter : Char.ToLower(letter);
                    output += " ";
                }
                output += "\n";
            }
            output += "\n";
            output += "  0 1 2 3 4 5 6 7";
            return output;
        }
    }
}