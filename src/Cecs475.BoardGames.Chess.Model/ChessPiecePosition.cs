using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cecs475.BoardGames.Chess {
	/// <summary>
	/// Represents each type of piece that can be placed on a chess board.
	/// </summary>
	public enum ChessPieceType : sbyte {
		/// <summary>
		/// An empty square
		/// </summary>
		Empty = 0,
		Pawn = 1,
		/// <summary>
		/// The queen-side rook.
		/// </summary>
		RookQueen = 2,
		/// <summary>
		/// The king-side rook.
		/// </summary>
		RookKing = 3,
		/// <summary>
		/// A rook from a promoted pawn.
		/// </summary>
		RookPawn = 8,
		Knight = 4,
		Bishop = 5,
		Queen = 6,
		King = 7
	}

	/// <summary>
	/// Represents the owner and type of a piece at a particular position on the 
	/// chess board.
	/// </summary>
	public struct ChessPiecePosition {
		public ChessPiecePosition(ChessPieceType type, int player) {
			PieceType = type;
			Player = player;
		}

		/// <summary>
		/// The type of piece found at the given position.
		/// </summary>
		public ChessPieceType PieceType { get; private set; }
		/// <summary>
		/// The player controlling the piece at the given position, or 0 if the position was empty.
		/// </summary>
		public int Player { get; private set; }
	}

}
