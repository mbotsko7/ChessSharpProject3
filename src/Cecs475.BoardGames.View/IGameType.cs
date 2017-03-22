using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace Cecs475.BoardGames.View {
	/// <summary>
	/// Represents a game that can be played in a WPF application. Creates controls and 
	/// converters for plugging into a multi-game application.
	/// </summary>
	public interface IGameType {
		/// <summary>
		/// The name of the game.
		/// </summary>
		string GameName { get; }

		/// <summary>
		/// Constructs and returns a View control and ViewModel for the game type.
		/// </summary>
		Tuple<Control, IGameViewModel> CreateViewAndViewModel();

		/// <summary>
		/// Creates and returns a converter for displaying the current board's value
		/// in a UI label.
		/// </summary>
		IValueConverter CreateBoardValueConverter();

		/// <summary>
		/// Creates and returns a converter for displaying the current board's
		/// current player in a UI label.
		/// </summary>
		IValueConverter CreateCurrentPlayerConverter();

		
	}
}
