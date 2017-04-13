using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Cecs475.BoardGames.Chess.View.Annotations;

namespace Cecs475.BoardGames.Chess.View {
    /// <summary>
    /// Interaction logic for PawnPromotionDialog.xaml
    /// </summary>
    public partial class PawnPromotionDialog {
        private readonly ChessViewModel _model;
        private static readonly SolidColorBrush WhiteBrush = new SolidColorBrush(Colors.AntiqueWhite);
        private static readonly SolidColorBrush BlackBrush = new SolidColorBrush(Colors.Black);
        private static readonly SolidColorBrush DefaultBrush = new SolidColorBrush(Colors.DimGray);
        public PawnPromotionDialog(ChessViewModel model) {
            InitializeComponent();
            DataContext = this;
            _model = model;
        }

        public string Piece { get; set; }
        
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e) {
            var button = sender as Button;

            // What was clicked?
            if (button == null)
                return;

            Piece = button.Content as string;
            Close();
        }

        private void Button_OnEnter(object sender, MouseEventArgs e) {
            var button = (Button) sender;

            button.Background = _model.CurrentPlayer == 1 ? WhiteBrush : BlackBrush;
            button.Foreground = _model.CurrentPlayer == 1 ? BlackBrush : WhiteBrush;
        }

        private void Button_OnLeave(object sender, MouseEventArgs e) {
            var button = (Button) sender;

            button.Background = DefaultBrush;
            button.Foreground = BlackBrush;
        }
    }
}