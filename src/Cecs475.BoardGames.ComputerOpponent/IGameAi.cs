using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cecs475.BoardGames.ComputerOpponent
{
    public interface IGameAi
    {
        IGameMove FindBestMove(IGameBoard b);
    }
}
