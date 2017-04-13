using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cecs475.BoardGames.Chess {
    /// <summary>
    /// Represents the type of move a particular ChessMove represents
    /// </summary>
    public enum ChessMoveType {
        /// <summary>
        /// Moving one piece using its normal move rules, possibly capturing another piece
        /// </summary>
        Normal,

        /// <summary>
        /// Castling to the queen side
        /// </summary>
        CastleQueenSide,

        /// <summary>
        /// Castling to the king side
        /// </summary>
        CastleKingSide,

        /// <summary>
        /// Performing an en passant
        /// </summary>
        EnPassant,

        /// <summary>
        /// Promoting a pawn that has reached the final rank
        /// </summary>
        PawnPromote
    }

    /// <summary>
    /// Represents a move to be applied to a ChessBoard object.
    /// </summary>
    public class ChessMove : IGameMove {
        /// <summary>
        /// Constructs a ChessMove that moves a piece from one position to another
        /// </summary>
        /// <param name="start">the starting position of the piece to move</param>
        /// <param name="end">the position where the piece will end up</param>
        public ChessMove(BoardPosition start, BoardPosition end) {
            StartPosition = start;
            EndPosition = end;
            MoveType = ChessMoveType.Normal;
        }

        /// <summary>
        /// Constructs a ChessMove that performs a "special" move from one position to another.
        /// </summary>
        /// <param name="start">the starting position of the piece to move</param>
        /// <param name="end">the position where the piece will end up</param>
        /// <param name="type">the special chess move type to perform</param>
        public ChessMove(BoardPosition start, BoardPosition end, ChessMoveType type) :
            this(start, end) {
            MoveType = type;
        }

        public override string ToString() {
            if (MoveType == ChessMoveType.PawnPromote) {
                string str = "(" + (char) ('a' + StartPosition.Col) + (8 - StartPosition.Row) + ", ";
                if (EndPosition.Col == (int) ChessPieceType.Bishop)
                    return str + "Bishop)";
                else if (EndPosition.Col == (int) ChessPieceType.Knight)
                    return str + "Knight)";
                else if (EndPosition.Col == (int) ChessPieceType.Queen)
                    return str + "Queen)";
                else
                    return str + "Rook)";
            } else {
                return "(" + (char) ('a' + StartPosition.Col) + (8 - StartPosition.Row) + ", " +
                       (char) ('a' + EndPosition.Col) + (8 - EndPosition.Row) + ")";
            }
        }

        public bool Equals(IGameMove m) {
            ChessMove other = m as ChessMove;
            return StartPosition.Equals(other.StartPosition) && EndPosition.Equals(other.EndPosition);
        }

        public bool Equals(ChessMove other) {
            return StartPosition.Equals(other.StartPosition) && EndPosition.Equals(other.EndPosition);
        }

        public override bool Equals(object obj) {
            return Equals(obj as ChessMove);
        }

        /// <summary>
        /// The starting position of the move.
        /// </summary>
        public BoardPosition StartPosition { get; set; }

        /// <summary>
        /// The ending position of the move.
        /// </summary>
        public BoardPosition EndPosition { get; set; }

        /// <summary>
        /// The chess piece that was moved.
        /// </summary>
        public ChessPiecePosition Piece { get; set; }

        /// <summary>
        /// Whatever piece was captured when this move was applied, if any.
        /// </summary>
        public ChessPiecePosition Captured { get; set; }

        /// <summary>
        /// The type of move being applied.
        /// </summary>
        public ChessMoveType MoveType { get; set; }
    }
}