using Cecs475.BoardGames.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

namespace Cecs475.BoardGames.WpfApplication {
	/// <summary>
	/// Interaction logic for GameChoiceWindow.xaml
	/// </summary>
	public partial class GameChoiceWindow : Window {
		public GameChoiceWindow() {
			InitializeComponent();
            Type gameType = typeof(IGameType);
            //Assembly.LoadFrom("lib/Cecs475.BoardGames.Chess.Model.dll");
            //Assembly.LoadFrom("lib/Cecs475.BoardGames.Chess.View.dll");
            Assembly.LoadFrom("lib/Cecs475.BoardGames.Othello.Model.dll");
            Assembly.LoadFrom("lib/Cecs475.BoardGames.Othello.View.dll");
            //Assembly.LoadFrom("lib/Cecs475.BoardGames.TicTacToe.Model.dll");
            //Assembly.LoadFrom("lib/Cecs475.BoardGames.TicTacToe.View.dll");
            List<object> l = new List<object>();
            var boardTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => gameType.IsAssignableFrom(t) && t.IsClass);
            foreach(var val  in boardTypes)
            {
                l.Add(Activator.CreateInstance(val));
            }
            /*foreach(var assemble in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach(var t in assemble.GetTypes())
                {
                    if (gameType.IsAssignableFrom(t) && t != gameType)
                    {

                        //var construct = gameType.GetConstructors(BindingFlags.Public);
                        //Type x = t.ReflectedType;
                        var obj = Activator.CreateInstance(t);
                        //var obj = construct[0].Invoke(new object[] { });
                        l.Add(obj);
                    }
                }
            }*/
            Application.Current.Resources["ItemsSource"] = boardTypes;
		}

		private void Button_Click(object sender, RoutedEventArgs e) {
			Button b = sender as Button;
			IGameType gameType = b.DataContext as IGameType;
			var gameWindow = new MainWindow(gameType,
                mHumanBtn.IsChecked.Value ? NumberOfPlayers.Two : NumberOfPlayers.One) {
				Title = gameType.GameName
			};
			gameWindow.Closed += GameWindow_Closed;
			gameWindow.Show();

			this.Hide();
		}

		private void GameWindow_Closed(object sender, EventArgs e) {
			this.Show();
		}
	}
}
