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
		public MainWindow(IGameType gameType, NumberOfPlayers players) {
			var viewAndViewModel = gameType.CreateViewAndViewModel(players);
			this.Resources.Add("GameView", viewAndViewModel.Item1);
			this.Resources.Add("ViewModel", viewAndViewModel.Item2);

			InitializeComponent();
			
			// Bind the Score label to the BoardValue of the ViewModel.
			mScoreLabel.SetBinding(Label.ContentProperty,
				new Binding() {
					Path = new PropertyPath("BoardValue"),
					Converter = gameType.CreateBoardValueConverter()
				}
			);

			// Bind the Player label to the CurrentPlayer of the ViewModel.
			mPlayerLabel.SetBinding(Label.ContentProperty,
				new Binding() {
					Path = new PropertyPath("CurrentPlayer"),
					Converter = gameType.CreateCurrentPlayerConverter()
				}
			);

			// 
			viewAndViewModel.Item2.GameFinished += ViewModel_GameFinished;
		}

		private void ViewModel_GameFinished(object sender, EventArgs e) {
			if (MessageBox.Show("Game over! Play a new game?", "Game over", 
				MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) ==
				MessageBoxResult.Yes) {
				this.Close();
			}
			else {
				Environment.Exit(0);
			}
		}

		private void UndoButton_Click(object sender, RoutedEventArgs e) {
			(FindResource("ViewModel") as IGameViewModel).UndoMove();
		}
	}
}
