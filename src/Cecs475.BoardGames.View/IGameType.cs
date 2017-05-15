using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace Cecs475.BoardGames.View
{
    public enum NumberOfPlayers
    {
        One = 1,
        Two
    }
    public interface IGameType
    {
        string GameName { get; }
        Tuple<Control, IGameViewModel> CreateViewAndViewModel(NumberOfPlayers players);
        IValueConverter CreateBoardValueConverter();
        IValueConverter CreateCurrentPlayerConverter();
    }
}