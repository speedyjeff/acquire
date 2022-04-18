using System;
using System.Collections.Generic;

namespace Acquire.Engine
{
    public class Computer1 : IComputer
    {
        private Random rand;

        public PlayerStats Player { get; set; }
        public CorpStats[] Corporations { get; set; }
        public CorpNames[,] Board { get; set; }
        public int NumberPlayers { get; set; }
        public int ID { get; set; }

        public Computer1(int seed = 0)
        {
            rand = (seed > 0) ? new Random(seed) : new Random();
        }

        // choose a tile
        public Square SelectSquare(Square[] tiles)
        {
            return tiles[rand.Next() % tiles.Length];
        }

        // buy shares
        public CorpNames[] BuyShares(List<CorpNames>[] corps)
        {
            CorpNames[] purchase;
            int cnt;
            
            purchase = new CorpNames[corps.Length];
            cnt = 0;
            foreach (List<CorpNames> choice in corps)
            {
                if (choice.Count == 0) purchase[cnt] = CorpNames.NA;
                else purchase[cnt] = choice[ rand.Next() % choice.Count ];
                cnt++;
            }

            return purchase;
        }

        // choose a coropration
        public CorpNames ChooseCorp(List<CorpNames> corps)
        {
            return corps[ rand.Next() % corps.Count ];
        }

        // choose a parent corporation
        public CorpNames ChooseParentCorp(List<CorpNames> corps)
        {
            return corps[ rand.Next() % corps.Count ];
        }

        // sell shares
        public int SellShares(SaleTransaction sale)
        {
            // sell them all
            return sale.Player.Shares(sale.Corporation);
        }

        // trade shares
        public int TradeShares(TradeTransaction trade, int parentCorpShares)
        {
            int maxShares;
            int shares;

            maxShares = trade.Player.Shares(trade.MergedCorp);
            if (parentCorpShares < trade.Player.Shares(trade.MergedCorp)) maxShares = parentCorpShares;

            if (maxShares == 0 || maxShares == 1) return 0;

            shares = rand.Next() % maxShares;

            // only return an even number
            return (shares % 2) == 0? shares: shares - 1;
        }
    }
}
