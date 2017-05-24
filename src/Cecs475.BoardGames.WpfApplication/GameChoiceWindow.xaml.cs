﻿using Cecs475.BoardGames.View;
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
using System.IO;

namespace Cecs475.BoardGames.WpfApplication {
	/// <summary>
	/// Interaction logic for GameChoiceWindow.xaml
	/// </summary>
	public partial class GameChoiceWindow : Window {
		public GameChoiceWindow() {
            InitializeComponent();
            
            Type gameType = typeof(IGameType);
            var files = Directory.GetFiles("lib");
            foreach(var f in Directory.GetFiles("lib"))
            {
                Console.WriteLine(f);
                string file = f.Substring(4);
                file = file.Substring(0, file.Length - 4);
                Assembly.Load($"{file}, Version=1.0.0.0, Culture=neutral, PublicKeyToken=68e71c13048d452a");
                //Assembly.Load($"{f.Substring(0,f.Length-4)}, Version=1.0.0.0, Culture=neutral, PublicKeyToken=68e71c13048d452a");
            }
            List<object> l = new List<object>();
            var boardTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => gameType.IsAssignableFrom(t) && t.IsClass);
            foreach(var val  in boardTypes)
            {
                //Console.WriteLine("1");
                IGameType v = (IGameType)val.GetConstructor(Type.EmptyTypes).Invoke(null);
                l.Add(v);
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
            Application.Current.Resources["GameTypes"] = l;
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

        public async Task DownloadGames()
        {
            var loadwindow = new Loading();
            loadwindow.Close();
        }
	}
}
