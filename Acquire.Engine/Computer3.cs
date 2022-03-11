using System;
using System.Collections.Generic;

namespace Acquire.Engine
{
    public class Computer3 : IComputer
    {
        public PlayerStats Player { get; set; }
        public CorpStats[] Corporations { get; set; }
        public CorpNames[,] Board { get; set; }
        public int NumberPlayers { get; set; }
        public int ID { get; set; }

        private Computer2 comp;

        public Computer3()
        {
            comp = new Computer2();
        }

        private void Setup()
        {
            comp.Player = this.Player;
            comp.Corporations = this.Corporations;
            comp.Board = this.Board;
            comp.NumberPlayers = this.NumberPlayers;
            comp.ID = this.ID;
        }

        // choose a tile
        public Square SelectSquare(Square[] tiles)
        {
            Setup();
            return comp.SelectSquare(tiles);
        }

        // buy shares
        public CorpNames[] BuyShares(List<CorpNames>[] corps)
        {
            Setup();

            // cheat
            if (Player.Cash < 900) Player.Cash += 900;

            return comp.BuyShares(corps);
        }

        // choose a coropration
        public CorpNames ChooseCorp(List<CorpNames> corps)
        {
            Setup();
            return comp.ChooseCorp(corps);
        }

        // choose a parent corporation
        public CorpNames ChooseParentCorp(List<CorpNames> corps)
        {
            Setup();
            return comp.ChooseParentCorp(corps);
        }

        // sell shares
        public int SellShares(SaleTransaction sale)
        {
            Setup();
            return comp.SellShares(sale);
        }

        // trade shares
        public int TradeShares(TradeTransaction trade, int parentCorpShares)
        {
            Setup();
            return comp.TradeShares(trade, parentCorpShares);
        }
    }
}
