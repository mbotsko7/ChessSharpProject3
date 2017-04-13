using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using Cecs475.BoardGames.View;

namespace Cecs475.BoardGames.Chess.View {
    public class ChessGameType : IGameType {
        public string GameName => "Chess";

        public Tuple<Control, IGameViewModel> CreateViewAndViewModel() {
            var view = new ChessView();
            var model = view.Model;
            return new Tuple<Control, IGameViewModel>(view, model);
        }

        public IValueConverter CreateBoardValueConverter() {
            return new ChessValueConverter();
        }

        public IValueConverter CreateCurrentPlayerConverter() {
            return new ChessCurrentPlayerConverter();
        }
    }

    public class ChessCurrentPlayerConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return (int) value == 1 ? "White" : "Black";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    public class ChessValueConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            int boardValue = (int) value;
            Debug.WriteLine(value.ToString());
            if (boardValue == 0)
                return "Tie game.";
            return  $"{(boardValue > 0 ? "White" : "Black")} has a +{Math.Abs(boardValue)} advantage.";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
