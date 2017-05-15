using System;
using System.Windows.Controls;
using Cecs475.BoardGames.View;
using System.Windows.Data;

namespace Cecs475.BoardGames.TicTacToe.View
{
    public class TicTacToeGameType : IGameType
    {
        public string GameName
        {
            get
            {
                return "TicTacToe";
            }
        }


        public IValueConverter CreateBoardValueConverter()
        {
            return new TicTacToeValueConverter();
        }

        public IValueConverter CreateCurrentPlayerConverter()
        {
            return new TicTacToePlayerConverter();
        }

        public Tuple<Control, IGameViewModel> CreateViewAndViewModel(NumberOfPlayers players)
        {
            var view = new TicTacToeView();
            var model = view.Model;
            model.Players = players;
            return new Tuple<Control, IGameViewModel>(view, model);
        }
    }
}