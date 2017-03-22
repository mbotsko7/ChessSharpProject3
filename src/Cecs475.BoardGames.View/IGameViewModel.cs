using System;
using System.ComponentModel;

namespace Cecs475.BoardGames.View {
	/// <summary>
	/// Represents a ViewModel for a game that can be played in a WPF application.
	/// </summary>
	public interface IGameViewModel : INotifyPropertyChanged {
		/// <summary>
		/// The value of the ViewModel's board.
		/// </summary>
		int BoardValue { get; }

		/// <summary>
		/// The current player of the ViewModel's board.
		/// </summary>
		int CurrentPlayer { get; }

		/// <summary>
		/// True if the board state can undo a move, false otherwise.
		/// </summary>
		bool CanUndo { get; }

		/// <summary>
		/// Undoes the most recent move applied to the game board.
		/// </summary>
		void UndoMove();

		/// <summary>
		/// Invoked when the game is finished.
		/// </summary>
		event EventHandler GameFinished;
	}
}
