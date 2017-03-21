using Cecs475.BoardGames.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Cecs475.BoardGames.WpfApplication {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		public MainWindow(IGameType gameType) {
			var viewAndViewModel = gameType.CreateViewAndViewModel();
			this.Resources.Add("GameView", viewAndViewModel.Item1);
			this.Resources.Add("ViewModel", viewAndViewModel.Item2);

			InitializeComponent();

			mScoreLabel.SetBinding(Label.ContentProperty,
				new Binding() {
					Path = new PropertyPath("BoardValue"),
					Converter = gameType.CreateBoardValueConverter()
				}
			);

			mPlayerLabel.SetBinding(Label.ContentProperty,
				new Binding() {
					Path = new PropertyPath("CurrentPlayer"),
					Converter = gameType.CreateCurrentPlayerConverter()
				}
			);
		}

		private void UndoButton_Click(object sender, RoutedEventArgs e) {
			(FindResource("ViewModel") as IGameViewModel).UndoMove();
		}
	}
}
