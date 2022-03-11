using System;
using System.Collections.Generic;

namespace Acquire.Engine
{
    enum TileType { NewCorp, MergeCorp, StandAlone, Illegal, Expansion };

    class TileOption
    {
        public TileType TType;
        public List<CorpNames> Merger;
        public CorpNames ExpanedCorp;
        public Square Tile;
        public int Weight;  // 0 touching, 1 tile away, 2 all else

        public TileOption()
        {
            Merger = new List<CorpNames>();
        }
    }

    public class Computer2 : IComputer
    {
        public PlayerStats Player { get; set; }
        public CorpStats[] Corporations { get; set; }
        public CorpNames[,] Board { get; set; }
        public int NumberPlayers { get; set; }
        public int ID { get; set; }

        private bool lastSkippedMerger = false;
        private bool lastSkippedExpansion = false;
        private List<string> decsions = new List<string>();

        private List<CorpNames> nextBuyShares = new List<CorpNames>();
        private CorpNames newCorporation = CorpNames.NA;
        private CorpNames mergedCorporation = CorpNames.NA;

        private TileOption[] CategorizeTiles(Square[] tiles, out bool hasNewCorp, out bool hasMerger, out bool hasCorpExpand)
        {
            TileOption[] types;

            hasNewCorp = hasMerger = hasCorpExpand = false;
            types = new TileOption[tiles.Length];

            // play out a what if for each tile
            for (int i = 0; i < tiles.Length; i++)
            {
                List<CorpNames> neighbors = new List<CorpNames>();
                List<CorpNames> uniqueCorps = new List<CorpNames>();
                bool avaliable;
                int corpsAboveMax;

                // initialize
                types[i] = new TileOption();
                types[i].Tile = tiles[i];

                // grab neighbors
                if (tiles[i].Dim0 > 0 && Board[tiles[i].Dim0 - 1, tiles[i].Dim1] != CorpNames.NA) neighbors.Add(Board[tiles[i].Dim0 - 1, tiles[i].Dim1]);
                if ((tiles[i].Dim0 + 1) < Board.GetLength(0) && Board[tiles[i].Dim0 + 1, tiles[i].Dim1] != CorpNames.NA) neighbors.Add(Board[tiles[i].Dim0 + 1, tiles[i].Dim1]);
                if (tiles[i].Dim1 > 0 && Board[tiles[i].Dim0, tiles[i].Dim1 - 1] != CorpNames.NA) neighbors.Add(Board[tiles[i].Dim0, tiles[i].Dim1 - 1]);
                if ((tiles[i].Dim1 + 1) < Board.GetLength(1) && Board[tiles[i].Dim0, tiles[i].Dim1 + 1] != CorpNames.NA) neighbors.Add(Board[tiles[i].Dim0, tiles[i].Dim1 + 1]);

                // calculate weight
                if (neighbors.Count > 0) types[i].Weight = 0;
                else
                {
                    // check the other tiles as neighbors
                    for (int j = 0; j < tiles.Length; j++)
                    {
                        // two of the tiles are next to each other
                        if (i == j)
                        {
                            // nothing
                        }
                        else if ( 
                            (tiles[j].Dim0 == tiles[i].Dim0 && 1 == Math.Abs(tiles[i].Dim1 - tiles[j].Dim1)) ||
                            (tiles[j].Dim1 == tiles[i].Dim1 && 1 == Math.Abs(tiles[i].Dim0 - tiles[j].Dim0)) 
                            )
                        {
                            types[i].Weight = 0;
                        }

                        // two tiles are within 1
                        else if (
                            (tiles[j].Dim0 == tiles[i].Dim0 && 2 == Math.Abs(tiles[j].Dim1 - tiles[i].Dim1)) ||
                            (tiles[j].Dim1 == tiles[i].Dim1 && 2 == Math.Abs(tiles[j].Dim0 - tiles[i].Dim0)) ||
                            (1 == Math.Abs(tiles[j].Dim0 - tiles[i].Dim0) && 1 == Math.Abs(tiles[j].Dim1 - tiles[i].Dim1))
                            )
                        {
                            // (Dim0, Dim1)
                            //
                            //                (-2, 0) 
                            //         (-1,-1)(x, x) (-1,1)
                            // (0, -2) (x, x) (home) (x, x) (0, 2)
                            //         (1,-1) (x, x) (1,1)
                            //                (2, 0)
                            types[i].Weight = 1;
                        }

                        // further away
                        else
                        {
                            types[i].Weight = 2;
                        }
                    }
                }

                // preparse the neighbors
                foreach (CorpNames corp in neighbors)
                {
                    if (!uniqueCorps.Contains(corp)) uniqueCorps.Add(corp);
                    if (!types[i].Merger.Contains(corp) && corp != CorpNames.UnClaimed) types[i].Merger.Add(corp);
                }

                // early exit if no neighbors
                if (neighbors.Count == 0)
                {
                    types[i].TType = TileType.StandAlone;
                }
                
                // check for new corp
                else if (uniqueCorps.Count == 1 && uniqueCorps[0] == CorpNames.UnClaimed)
                {
                    // check for invalid - search for 0 size corps
                    avaliable = false;
                    foreach (CorpStats corp in Corporations)
                    {
                        if (corp.Size == 0) avaliable = true;
                    }

                    if (avaliable)
                    {
                        types[i].TType = TileType.NewCorp;
                        hasNewCorp = true;
                    }
                    else types[i].TType = TileType.Illegal;
                }

                // check for expansion
                else if (uniqueCorps.Count == 1 || (uniqueCorps.Count == 2 && (uniqueCorps[0] == CorpNames.UnClaimed || uniqueCorps[1] == CorpNames.UnClaimed)))
                {          
                    // this is expansion
                    types[i].ExpanedCorp = uniqueCorps[0] == CorpNames.UnClaimed ? uniqueCorps[1] : uniqueCorps[0];
                    types[i].TType = TileType.Expansion;
                    hasCorpExpand = true;
                }

                // merge case
                else if (types[i].Merger.Count > 1)
                {
                    // check for an invalid merge
                    corpsAboveMax = 0;
                    foreach (CorpNames corp in types[i].Merger)
                    {
                        if (Corporations[(int)corp].Size > AcquireConstants.NonMergableCorp) corpsAboveMax++;
                    }

                    // invalid merge
                    if (corpsAboveMax > 1) types[i].TType = TileType.Illegal;
                    else
                    {
                        types[i].TType = TileType.MergeCorp;
                        hasMerger = true;
                    }
                }
                else
                {
                    throw new ArgumentException("Should not get here!");
                }
            }

            return types;
        }

        // decision tree
        //
        //  hasMerger - YES ->  |
        //     |                |
        //    NO                |
        //     |               \|/
        //     |           Buy 3 shares? - NO -> MERGE_LOGIC (pick corp with largest shares)
        //     |                |
        //     |               YES *(Note with share to buy, DONT TAKE THIS BRANCH NEXT TIME)
        //     |               \|/
        //     |----------> hasNewCorp - YES -> NEWCORP_LOGIC (pick cheapest)
        //                      |
        //                     NO || none avaliable
        //                     \|/
        //                   0 Weight tiles (!new && !merge && !expand) -YES-> CHOOSE_0_WEIGHT_LOGIC 
        //                      |
        //                     NO
        //                     \|/
        //                hasCorpExpand - YES -> money - Buy stock? - NO -> EXPAND_LOGIC (pick corp with largest shares)
        //                      |                           |
        //                     NO                          YES *(Note which share to buy, DONT TAKE THIS BRANCH NEXT TIME)
        //                     \|/                         \|/
        //                    CHOOSETILE_LOGIC            CHOOSETILE_LOGIC (don't merge, don't create corp, don't expand, find tile by others, or by largest corp)

        // choose a tile
        public Square SelectSquare(Square[] tiles)
        {
            TileOption[] options;
            bool hasNewCorp;
            bool hasMerger;
            bool hasCorpExpand;

            decsions.Add("SelectSquare");

            // init
            nextBuyShares.Clear();
            newCorporation = CorpNames.NA;
            mergedCorporation = CorpNames.NA;
                
            // preparse tiles
            options = CategorizeTiles(tiles, out hasNewCorp, out hasMerger, out hasCorpExpand);

            // try merger
            if (hasMerger)
            {
                CorpNames largestCorp = CorpNames.NA;
                Square largestCorpTile = tiles[0];

                // find the largest corp
                int maxShares = -1;
                foreach (TileOption option in options)
                {
                    if (option.TType == TileType.MergeCorp)
                    {
                        // find the corp with the largest number of shares
                        foreach (CorpNames corp in option.Merger)
                        {
                            if (Player.Shares(corp) > maxShares)
                            {
                                maxShares = Player.Shares(corp);
                                largestCorp = corp;
                                largestCorpTile = option.Tile;
                            }
                        }
                    }
                }

                // set the corp that we want to be merged
                mergedCorporation = largestCorp;

                // try to merge first
                if (Corporations[(int)largestCorp].Shares == 0 
                    || Player.Cash < ((Corporations[(int)largestCorp].Shares > AcquireConstants.MaxSharePurchase)? (Corporations[(int)largestCorp].Price * AcquireConstants.MaxSharePurchase) : (Corporations[(int)largestCorp].Price * Corporations[(int)largestCorp].Shares))
                    || lastSkippedMerger)
                {
                    decsions.Add(" - skip(" + lastSkippedMerger + ") merge " + largestCorp + " now");

                    lastSkippedMerger = false;
                    // make the merger
                    return largestCorpTile;
                }
                else
                {
                    decsions.Add(" - merge " + largestCorp + " later");

                    // buy shares then merge
                    nextBuyShares.Add(largestCorp);
                    lastSkippedMerger = true;
                }
            }
            
            // try new corp
            if (hasNewCorp)
            {
                Square selectedTile = tiles[0];

                // find the tile which creates a corp
                foreach (TileOption option in options)
                {
                    if (option.TType == TileType.NewCorp)
                    {
                        selectedTile = option.Tile;
                        break;
                    }
                }

                // pick the corp with the highest price that we can buy 3 shares in
                // find if any corps are avaliable
                foreach (CorpStats corp in Corporations)
                {
                    if (corp.Size == 0)
                    {
                        decsions.Add(" - New most expensive affordable " + corp.Name);

                        // choose the most expensive affordable corp
                        nextBuyShares.Add(corp.Name);

                        newCorporation = corp.Name;
                        return selectedTile;
                    }
                }

                throw new ArgumentException("A new corporation should have been choosen");
            }

            // find 0 weight items that are not a new corp, or merger, or expansion
            foreach (TileOption option in options)
            {
                if (option.TType == TileType.StandAlone && option.Weight == 0)
                {
                    decsions.Add(" - zero weight tile");

                    return option.Tile;
                }
            }

            // expansion
            if (hasCorpExpand)
            {
                CorpNames largestCorp = CorpNames.NA;
                Square largestCorpTile = tiles[0];

                // find the largest corp
                int maxShares = -1;
                foreach (TileOption option in options)
                {
                    if (option.TType == TileType.Expansion)
                    {
                       if (Player.Shares(option.ExpanedCorp) > maxShares)
                       {
                           maxShares = Player.Shares(option.ExpanedCorp);
                           largestCorp = option.ExpanedCorp;
                           largestCorpTile = option.Tile;
                       }
                    }
                }

                // see if we can buy shares
                if (Corporations[(int)largestCorp].Shares == 0
                    || Player.Cash < ((Corporations[(int)largestCorp].Shares > AcquireConstants.MaxSharePurchase) ? (Corporations[(int)largestCorp].Price * AcquireConstants.MaxSharePurchase) : (Corporations[(int)largestCorp].Price * Corporations[(int)largestCorp].Shares))
                    || lastSkippedExpansion)
                {
                    decsions.Add(" - skip(" + lastSkippedExpansion + ") expand " + largestCorp + " now");

                    lastSkippedExpansion = false;
                    // make the merger
                    return largestCorpTile;
                }
                else
                {
                    decsions.Add(" - Expand " + largestCorp + " later");

                    // buy shares then merge
                    nextBuyShares.Add(largestCorp);
                    lastSkippedExpansion = true;
                }
            }

            // find the tile which is standalone which are close
            foreach (TileOption option in options)
            {
                if (option.TType == TileType.StandAlone && option.Weight == 1)
                {
                    decsions.Add(" - Return 1 weight tile");

                    return option.Tile;
                }
            }

            // find the tile which is standalone and not necessarily close
            foreach (TileOption option in options)
            {
                if (option.TType == TileType.StandAlone && option.Weight > 1)
                {
                    decsions.Add(" - Return 2+ weight tile");
                    return option.Tile;
                }
            }

            // hmm... that means that we can't find a perferred tile
            foreach (TileOption option in options)
            {
                if (option.TType == TileType.MergeCorp)
                {
                    decsions.Add(" - Return merger tile");
                    return option.Tile;
                }
            }
            foreach (TileOption option in options)
            {
                if (option.TType == TileType.Expansion)
                {
                    decsions.Add(" - Return expansion tile");
                    return option.Tile;
                }
            }

            decsions.Add(" - Return invalid");

            // all else just return the first, it will be illegal
            return tiles[0];
        }

        // decision tree
        //
        // nextBuyShares.Count - 1+ -> BUY_SPECIFIC_LOGIC (round robin on list and buy as much as you can)
        //       |
        //      ==0
        //      \|/
        //    corpSize - 11+ -> LONGTERM_LOGIC (ensure that we are ahead in large corps [shares > (boughtShares/NumberPlayers)])
        //       |
        //     all <11
        //      \|/
        //     SHORTTERM_LOGIC (buy stock in corps in the middle)

        // buy shares
        public CorpNames[] BuyShares(List<CorpNames>[] corps)
        {
            CorpNames[] shares = new CorpNames[corps.Length];
            int[] remaining = new int[AcquireConstants.CorpCount]; // track how many of each corp remain
            int cash;
            int index;

            decsions.Add("BuyShares");

            // init
            for (int i = 0; i < shares.Length; i++) shares[i] = CorpNames.NA;
            cash = Player.Cash;
            index = shares.Length - 1;
            for (int i = 0; i < remaining.Length; i++) remaining[i] = Corporations[i].Shares;

            // buy what we were told to buy
            if (nextBuyShares.Count > 0)
            {
                // round robin through the names and buy as much as you can
                for (int i = 0; i < shares.Length; i++)
                {
                    CorpNames corp = nextBuyShares[i % nextBuyShares.Count];

                    shares[i] = CorpNames.NA;

                    if (cash > (Corporations[(int)corp].Price) && remaining[(int)corp] > 0)
                    {
                        decsions.Add(" - Buy predetermined " + corp);
                        cash -= Corporations[(int)corp].Price;
                        shares[i] = corp;
                        remaining[(int)corp]--;
                    }
                }

                return shares;
            }
            else
            {
                for (int tries = 0; tries < corps.Length; tries++)
                {
                    // check if there is a large corp, ensure we are the majority
                    foreach (CorpStats corp in Corporations)
                    {
                        if (corp.Size > AcquireConstants.NonMergableCorp)
                        {
                            // check that we have a reasonable chance of being the majority
                            if (index >= 0
                                && remaining[(int)corp.Name] > 0
                                && Player.Shares(corp.Name) < ((AcquireConstants.MaxShares - corp.Shares) / NumberPlayers)
                                && cash >= corp.Price)
                            {
                                decsions.Add(" - Buy " + corp.Name + " to remain majority");
                                shares[index--] = corp.Name;
                                cash -= corp.Price;
                                remaining[(int)corp.Name]--;
                            }
                        }
                    }

                    //   0 1 2 3 4 5 6 7 8 9 10 11
                    //  0
                    //  1
                    //  2
                    //  3      x x x x x x
                    //  4      x x x x x x
                    //  5      x x x x x x
                    //  6      x x x x x x
                    //  7      x x x x x x
                    //  8      x x x x x x 
                    //  9
                    // 10
                    // 11

                    // find coporations that are near the middle and buy stock
                    for (int dim0 = AcquireConstants.BoardHeight/4; dim0 < (AcquireConstants.BoardHeight*3)/4 && index >= 0; dim0++)
                    {
                        for (int dim1 = AcquireConstants.BoardWidth/4; dim1 < (AcquireConstants.BoardWidth*3)/4 && index >= 0; dim1++)
                        {
                            CorpNames corp = Board[dim0, dim1];

                            if (index >= 0
                                && corp != CorpNames.NA
                                && corp != CorpNames.UnClaimed
                                && remaining[(int)corp] > 0
                                && Corporations[(int)corp].Size > 0
                                && Corporations[(int)corp].Shares >= 1
                                && cash >= Corporations[(int)corp].Price)
                            {
                                decsions.Add(" - Buy " + corp + " since it is in the middle");
                                shares[index--] = corp;
                                cash -= Corporations[(int)corp].Price;
                                remaining[(int)corp]--;
                            }
                        }
                    }

                    // buy stock in the smaller corporations
                    // TODO: this is a brain dead algorithm... it has a lot of room for improvement
                    for (int size = 2; size <= AcquireConstants.NonMergableCorp && index >= 0; size++)
                    {
                        foreach (CorpStats corp in Corporations)
                        {
                            if (index >= 0
                                && remaining[(int)corp.Name] > 0
                                && corp.Size == size
                                && corp.Shares >= 1
                                && cash >= corp.Price)
                            {
                                decsions.Add(" - Buy " + corp.Name + " since it is small");
                                shares[index--] = corp.Name;
                                cash -= corp.Price;
                                remaining[(int)corp.Name]--;
                            }
                        }
                    }
                }

                decsions.Add(" - Bought " + (shares.Length - 1 - index) + " shares");

                return shares;
            }
        }

        // decision tree
        //
        // newCorporation (or fail)

        // choose a coropration
        public CorpNames ChooseCorp(List<CorpNames> corps)
        {
            if (newCorporation == CorpNames.NA || !corps.Contains(newCorporation)) throw new ArgumentException("NewCorporation should have been set before calling this " + newCorporation);

            return newCorporation;
        }

        // decision tree
        //
        // choose a corp other than mergeCorporatoin (or fail)

        // choose a parent corporation
        public CorpNames ChooseParentCorp(List<CorpNames> corps)
        {
            if (mergedCorporation == CorpNames.NA) throw new ArgumentException("MergeCorporation should have been set before calling this " + mergedCorporation);

            decsions.Add("ChooseParentCorp");

            foreach (CorpNames corp in corps)
            {
                if (corp != mergedCorporation)
                {
                    decsions.Add(" - Merge " + corp);
                    return corp;
                }
            }

            // ugh... this is not the way that I had it planned
            if (corps.Contains(mergedCorporation))
            {
                decsions.Add(" - [things changed] mergedCorportation " + corps);
                return mergedCorporation;
            }
            else
            {
                decsions.Add(" - [things really changed] return first " + corps[0]);
                return corps[0];
            }
        }

        // decision tree
        //
        // Sell them all

        // sell shares
        public int SellShares(SaleTransaction sale)
        {
            // always sell them all
            return Player.Shares( sale.Corporation );
        }

        // decision tree
        //
        // Cash - buy 0 shares -> TRADE_HALF
        //  |
        //  buy 1 share
        // \|/
        // TRADE_ALL

        // trade shares
        public int TradeShares(TradeTransaction trade, int parentCorpShares)
        {
            int lowestprice = Int32.MaxValue;
            int shares = 0;

            // find the cheapest share price
            foreach (CorpStats corp in Corporations)
            {
                if (corp.Size > 0 && corp.Shares > 0 && corp.Price < lowestprice) lowestprice = corp.Price;
            }

            // calculate the max number of shares which can be traded
            shares = (Player.Shares(trade.MergedCorp) > (parentCorpShares*2)) ? (parentCorpShares*2) : Player.Shares(trade.MergedCorp);
            // ensure they are even
            if (shares % 2 != 0) shares--;

            if (Player.Cash > lowestprice)
            {
                // trade max
                return shares;
            }
            else
            {
                // trade half
                shares = shares / 2;
                if (shares % 2 != 0) shares--;
                return shares;
            }
        }
    }
}
