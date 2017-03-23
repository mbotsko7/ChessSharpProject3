using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Cecs475.BoardGames.Othello.View {
	class OthelloSquareColorConverter : IMultiValueConverter {
		private static SolidColorBrush RED_BRUSH = new SolidColorBrush(Colors.Red);
		private static SolidColorBrush CORNER_BRUSH = new SolidColorBrush(Colors.Green);
		private static SolidColorBrush SIDE_BRUSH = new SolidColorBrush(Colors.LightGreen);
		private static SolidColorBrush DANGER_BRUSH = new SolidColorBrush(Colors.PaleVioletRed);
		private static SolidColorBrush DEFAULT_BRUSH = new SolidColorBrush(Colors.LightBlue);

		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
			// This converter will receive two properties: the Position of the square, and whether it
			// is being hovered.
			BoardPosition pos = (BoardPosition)values[0];
			bool isHovered = (bool)values[1];

			// Hovered squares have a specific color.
			if (isHovered) {
				return RED_BRUSH;
			}
			// Corner squares are very good, and drawn green.
			if ((pos.Row == 0 || pos.Row == 7) && (pos.Col == 0 || pos.Col == 7)) {
				return CORNER_BRUSH;
			}
			// Squares next to corners are very bad, and drawn pale red.
			if ((pos.Row == 0 || pos.Row == 1 || pos.Row == 6 || pos.Row == 7) 
				&& (pos.Col == 0 || pos.Col == 1 || pos.Col == 6 || pos.Col == 7)) {
				return DANGER_BRUSH;
			}
			// Squares along the edge are good, and drawn light green.
			if (pos.Row == 0 || pos.Row == 7 || pos.Col == 0 || pos.Col == 7) {
				return SIDE_BRUSH;
			}
			// Inner squares are drawn light blue.
			return DEFAULT_BRUSH;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
