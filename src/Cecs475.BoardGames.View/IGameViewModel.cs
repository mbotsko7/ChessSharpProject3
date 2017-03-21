using System;
using System.ComponentModel;

namespace Cecs475.BoardGames.View {
	public interface IGameViewModel : INotifyPropertyChanged {
		int BoardValue { get; }
		int CurrentPlayer { get; }
		bool CanUndo { get; }
		void UndoMove();
		event EventHandler GameFinished;
	}
}
