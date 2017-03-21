using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace Cecs475.BoardGames.View {
	public interface IGameType {
		string GameName { get; }
		Tuple<Control, IGameViewModel> CreateViewAndViewModel();
		IValueConverter CreateBoardValueConverter();
		IValueConverter CreateCurrentPlayerConverter();
	}
}
