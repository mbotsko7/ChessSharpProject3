using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Cecs475.BoardGames.Chess.View {
    class ChessSquareColorConverter : IMultiValueConverter {
        private static readonly SolidColorBrush DarkBrush = new SolidColorBrush(Colors.DarkSeaGreen);
        private static readonly SolidColorBrush LightBrush = new SolidColorBrush(Colors.PeachPuff);
        private static readonly SolidColorBrush HoveredBrush = new SolidColorBrush(Colors.DarkOliveGreen);
        private static readonly SolidColorBrush SelectedBrush = new SolidColorBrush(Colors.Salmon);
        private static readonly SolidColorBrush CheckBrush = new SolidColorBrush(Colors.Yellow);

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            var pos = (BoardPosition) values[0];
            var isHovered = (bool) values[1];
            var isSelected = (bool) values[2];
            var isInCheck = (bool) values[3];
            var viewModel = (ChessViewModel) parameter;

            // If the king is in trouble, highlight it
            if (isInCheck)
                return CheckBrush;

            // If it's selected, highlight it
            if (isSelected) 
                return SelectedBrush;

            // If it's hovered, highlight it
            if (isHovered) 
                return viewModel.HasSelected ? SelectedBrush : HoveredBrush;
            
            // Alternate between a light and dark brush
            return (pos.Row + pos.Col) % 2 == 0 ? LightBrush : DarkBrush;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}