using Cecs475.BoardGames.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace Cecs475.BoardGames.Othello.View {
	public class OthelloGameType : IGameType {
		public string GameName {
			get {
				return "Othello";
			}
		}

		public IValueConverter CreateBoardValueConverter() {
			return new OthelloValueConverter();
		}

		public IValueConverter CreateCurrentPlayerConverter() {
			return new OthelloCurrentPlayerConverter();
		}

		public Tuple<Control, IGameViewModel> CreateViewAndViewModel() {
			var view = new OthelloView();
			var model = view.Model;
			return new Tuple<Control, IGameViewModel>(view, model);
		}
	}
}
