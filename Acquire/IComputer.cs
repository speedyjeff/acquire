using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Acquire
{
    public interface IComputer
    {
        PlayerStats Player {get; set;}
        CorpStats[] Corporations {get; set;}
        CorpNames[,] Board {get; set;}
        int NumberPlayers { get; set; }
        int ID { get; set; }

        // choose a tile
        Square SelectSquare(Square[] tiles);

        // buy shares
        CorpNames[] BuyShares(List<CorpNames>[] corps); 

        // choose a coropration
        CorpNames ChooseCorp(List<CorpNames> corps);

        // choose a parent corporation
        CorpNames ChooseParentCorp(List<CorpNames> corps);

        // sell shares
        int SellShares(SaleTransaction sale);

        // trade shares
        int TradeShares(TradeTransaction trade, int parentCorpShares);
    }
}
