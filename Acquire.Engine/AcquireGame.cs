using System;
using System.Collections.Generic;

namespace Acquire.Engine
{
    public enum CorpNames { Apple=0, Berry=1, Canary=2, Drip=3, Easy=4, Frozen=5, Grape=6, NA=7, UnClaimed=8 };
    public enum AcquireGameStates { GameSetup, PlaceTile, SellShares, ChooseCorp, BuyShares, ChooseParentCorp, TradeShares, NextTurn, ExitGame, Done };
    public enum BoardStates { InvalidAlways, InvalidNow, StandAlone, ExpandCorp, NewCorp, MergeCorp };
    public enum BonusType {Majoriy, SharedMajority, Minority, SharedMinority};
    public enum AdditionalMessage { Blank, NoAvaliableCorps, InvalidMerger, InsufficientFunds };

    // this is the price matrix
    //  A      C      F    Stock  Majority  Minority
    //  B      D      G    Value  Bonus     Bonus
    //         E
    //-------------------  -----  --------  --------
    //  2      -      -    $ 200  $  600    $ 300
    //  3      2      -    $ 300  $ 1200    $ 600
    //  4      3      2    $ 400  $ 2400    $1200
    //  5      4      3    $ 600  $ 3600    $1800
    //6-11     5      4    $ 800  $ 4800    $2400
    //12-20  6-11     5    $1000  $ 6000    $3000
    //21-40  12-20  6-11   $1200  $ 7200    $3600
    //41..   21-40  12-20  $1400  $ 8400    $4200
    //  -    41..   21-40  $1600  $10600    $5300
    //  -      -    41..   $1800  $12000    $6000

    public class CorpStats
    {
        public CorpStats(CorpNames name, int size)
        {
            Name = name;
            Size = size;
            Shares = AcquireConstants.MaxShares; // default starting value
        }

        public CorpNames Name { get; set; }
        public int Shares { get; set; }
        public int Size { get; set; }

        public int Price
        {
            get
            {
                if (Size == 0) return 0;

                if (Name.Equals(CorpNames.Apple) || Name.Equals(CorpNames.Berry))
                {
                    if (Size == 2) return 200;
                    else if (Size == 3) return 300;
                    else if (Size == 4) return 400;
                    else if (Size == 5) return 600;
                    else if (Size >=6 && Size <=11) return 800;
                    else if (Size >= 12 && Size <= 20) return 1000;
                    else if (Size >= 21 && Size <= 40) return 1200;
                    else if (Size >= 41) return 1400;
                }
                else if (Name.Equals(CorpNames.Canary) || Name.Equals(CorpNames.Drip) || Name.Equals(CorpNames.Easy))
                {
                    if (Size == 2) return 300;
                    else if (Size == 3) return 400;
                    else if (Size == 4) return 600;
                    else if (Size == 5) return 800;
                    else if (Size >= 6 && Size <= 11) return 1000;
                    else if (Size >= 12 && Size <= 20) return 1200;
                    else if (Size >= 21 && Size <= 40) return 1400;
                    else if (Size >= 41) return 1600;
                }
                else if (Name.Equals(CorpNames.Frozen) || Name.Equals(CorpNames.Grape))
                {
                    if (Size == 2) return 400;
                    else if (Size == 3) return 600;
                    else if (Size == 4) return 800;
                    else if (Size == 5) return 1000;
                    else if (Size >= 6 && Size <= 11) return 1200;
                    else if (Size >= 12 && Size <= 20) return 1400;
                    else if (Size >= 21 && Size <= 40) return 1600;
                    else if (Size >= 41) return 1800;
                }

                throw new ArgumentException("Price failed for " + Name + " with + " + Size);
            }
        }

        public int BonusMax
        {
            get
            {
                if (Size == 0) return 0;

                if (Name.Equals(CorpNames.Apple) || Name.Equals(CorpNames.Berry))
                {
                    if (Size == 2) return 600;
                    else if (Size == 3) return 1200;
                    else if (Size == 4) return 2400;
                    else if (Size == 5) return 3600;
                    else if (Size >= 6 && Size <= 11) return 4800;
                    else if (Size >= 12 && Size <= 20) return 6000;
                    else if (Size >= 21 && Size <= 40) return 7200;
                    else if (Size >= 41) return 8400;
                }
                else if (Name.Equals(CorpNames.Canary) || Name.Equals(CorpNames.Drip) || Name.Equals(CorpNames.Easy))
                {
                    if (Size == 2) return 1200;
                    else if (Size == 3) return 2400;
                    else if (Size == 4) return 3600;
                    else if (Size == 5) return 4800;
                    else if (Size >= 6 && Size <= 11) return 6000;
                    else if (Size >= 12 && Size <= 20) return 7200;
                    else if (Size >= 21 && Size <= 40) return 8400;
                    else if (Size >= 41) return 10600;
                }
                else if (Name.Equals(CorpNames.Frozen) || Name.Equals(CorpNames.Grape))
                {
                    if (Size == 2) return 2400;
                    else if (Size == 3) return 3600;
                    else if (Size == 4) return 4800;
                    else if (Size == 5) return 6000;
                    else if (Size >= 6 && Size <= 11) return 7200;
                    else if (Size >= 12 && Size <= 20) return 8400;
                    else if (Size >= 21 && Size <= 40) return 10600;
                    else if (Size >= 41) return 12000;
                }

                throw new ArgumentException("BonusMax failed for " + Name + " with + " + Size);
            }
        }

        public int BonusMin
        {
            get
            {
                if (Size == 0) return 0;

                if (Name.Equals(CorpNames.Apple) || Name.Equals(CorpNames.Berry))
                {
                    if (Size == 2) return 300;
                    else if (Size == 3) return 600;
                    else if (Size == 4) return 1200;
                    else if (Size == 5) return 1800;
                    else if (Size >= 6 && Size <= 11) return 2400;
                    else if (Size >= 12 && Size <= 20) return 3000;
                    else if (Size >= 21 && Size <= 40) return 3600;
                    else if (Size >= 41) return 4200;
                }
                else if (Name.Equals(CorpNames.Canary) || Name.Equals(CorpNames.Drip) || Name.Equals(CorpNames.Easy))
                {
                    if (Size == 2) return 600;
                    else if (Size == 3) return 1200;
                    else if (Size == 4) return 1800;
                    else if (Size == 5) return 2400;
                    else if (Size >= 6 && Size <= 11) return 3000;
                    else if (Size >= 12 && Size <= 20) return 3600;
                    else if (Size >= 21 && Size <= 40) return 4200;
                    else if (Size >= 41) return 5300;
                }
                else if (Name.Equals(CorpNames.Frozen) || Name.Equals(CorpNames.Grape))
                {
                    if (Size == 2) return 1200;
                    else if (Size == 3) return 1800;
                    else if (Size == 4) return 2400;
                    else if (Size == 5) return 3000;
                    else if (Size >= 6 && Size <= 11) return 3600;
                    else if (Size >= 12 && Size <= 20) return 4200;
                    else if (Size >= 21 && Size <= 40) return 5300;
                    else if (Size >= 41) return 6000;
                }

                throw new ArgumentException("BonusMin failed for " + Name + " with + " + Size);
            }
        }
    }

    public class PlayerStats
    {
        private int[] shares;
        private List<Square> tiles;

        public PlayerStats(string name, bool computer, int id)
        {
            // initialize the details
            Cash = AcquireConstants.InitialCash;
            Name = name;
            IsComputer = computer;
            tiles = new List<Square>();
            ID = id;

            shares = new int[AcquireConstants.CorpCount];
            for(int c=0; c<AcquireConstants.CorpCount; c++) shares[c] = 0;
        }

        public int ID { get; set; }
        public bool IsComputer { get; set; }
        public string Name { get; set;  }
        public int Cash { get; set; }

        public Square[] Tiles
        {
            get
            {
                return tiles.ToArray();
            }
        }

        public bool AddTile(Square tile)
        {
            if (tiles.Count > AcquireConstants.MaxTiles) throw new ArgumentException("Added to many tiles to '" + Name + "'s tile collection");
            tiles.Add(tile);
            return true;
        }

        public bool RemoveTile(Square tile)
        {
            tiles.Remove(tile);
            return true;
        }

        public int Shares(CorpNames corp)
        {
            return shares[(int)corp];
        }

        public void AddShares(CorpNames corp, int count)
        {
            shares[(int)corp] += count;
        }

        public void RemoveShares(CorpNames corp, int count)
        {
            shares[(int)corp] -= count;
        }
    }

    public struct Square
    {
        public int Dim0;
        public int Dim1;
    }

    class TileCollection
    {
        private Square[] tiles;
        private int currentTile;
        private Random rand;

        public TileCollection(int seed = 0)
        {
            currentTile = -1;

            tiles = new Square[AcquireConstants.BoardWidth * AcquireConstants.BoardHeight];
            for(int Dim0=0; Dim0<AcquireConstants.BoardHeight; Dim0++)
            {
                for (int Dim1=0; Dim1<AcquireConstants.BoardWidth; Dim1++)
                {
                    // initialize the set of tiles
                    tiles[Dim1 + (Dim0*AcquireConstants.BoardHeight)].Dim0 = Dim0;
                    tiles[Dim1 + (Dim0*AcquireConstants.BoardHeight)].Dim1 = Dim1;
                }
            }

            // randomize the tile collection
            rand = (seed > 0) ? new Random(seed) : new Random();
            for (int i=0; i<500; i++)
            {
                int i1 = rand.Next() % (AcquireConstants.BoardHeight*AcquireConstants.BoardWidth);
                int i2 = rand.Next() % (AcquireConstants.BoardHeight*AcquireConstants.BoardWidth);
                Square stmp = tiles[i1];
                tiles[i1] = tiles[i2];
                tiles[i2] = stmp;
            }

        }

        public int Count
        {
            get
            {
                lock (this)
                {
                    return ((tiles.Length - 1) - currentTile);
                }
            }
        }


        public Square Next()
        {
            lock (this)
            {
                if (Count <= 0) throw new ArgumentException("All tiles have been played");

                currentTile++;
                return tiles[currentTile];
            }
        }

        public void Push(Square square)
        {
            lock (this)
            {
                int ntmp;

                // push the tile back on the queue
                if (currentTile < 0) throw new ArgumentException("Not able to push tile onto collection");

                // find an index later in the collection
                ntmp = (rand.Next() % Count) + currentTile;

                // slide in the new tile
                tiles[currentTile] = tiles[ntmp];
                tiles[ntmp] = square;

                // back up the current pointer
                currentTile--;
            }
        }
    }

    class Board
    {
        private CorpNames[,] board;

        public Board()
        {
            board = new CorpNames[AcquireConstants.BoardHeight, AcquireConstants.BoardWidth];
            for (int Dim0 = 0; Dim0 < AcquireConstants.BoardHeight; Dim0++)
            {
                for (int Dim1 = 0; Dim1 < AcquireConstants.BoardWidth; Dim1++)
                {
                    // initialize the board
                    board[Dim0,Dim1] = CorpNames.NA;
                }
            }
        }

        public CorpNames[,] RawBoard
        {
            get
            {
                return board;
            }
        }

        // MUST NOT manipulate any state... this must remain as non-side effect code!
        public BoardStates TryPlace(Square tile, CorpStats[] corps, out List<CorpNames> merger, out CorpNames expandedCorp)
        {
            // inspect the 4 neighbors
            //  Could be either..
            //    BoardStates.StandAlone - no neighbors
            //    BoardStates.ExpandCorp - all neighbors same corp, or unclaimed
            //    BoardStates.NewCorp    - all neighbors unclaimed
            //    BoardStates.MergedCorp - neighbors 2 or more different corps
            //  Special
            //    BoardStates.InvalidNow - New corp, but none avaliable
            //    BoardStates.InvalidAlways - Merge corp, but 2 or more are greater than AcquireConstants.NonMergableCorp

            List<CorpNames> neighbors;
            List<CorpNames> uniqueCorps;
            bool avaliable;
            int corpsAboveMax;
            
            // init
            merger = new List<CorpNames>();
            expandedCorp = CorpNames.NA;
            neighbors = new List<CorpNames>();
            uniqueCorps = new List<CorpNames>();

            // grab neighbors
            if (tile.Dim0 > 0 && board[tile.Dim0 - 1, tile.Dim1] != CorpNames.NA) neighbors.Add(board[tile.Dim0 - 1, tile.Dim1]);
            if ((tile.Dim0+1) < board.GetLength(0) && board[tile.Dim0 + 1, tile.Dim1] != CorpNames.NA) neighbors.Add(board[tile.Dim0 + 1, tile.Dim1]);
            if (tile.Dim1 > 0 && board[tile.Dim0, tile.Dim1 - 1] != CorpNames.NA) neighbors.Add(board[tile.Dim0, tile.Dim1 - 1]);
            if ((tile.Dim1+1) < board.GetLength(1) && board[tile.Dim0, tile.Dim1 + 1] != CorpNames.NA) neighbors.Add(board[tile.Dim0, tile.Dim1 + 1]);

            // early exit if no neighbors
            if (neighbors.Count == 0) return BoardStates.StandAlone;

            // preparse the neighbors
            foreach (CorpNames corp in neighbors)
            {
                if (!uniqueCorps.Contains(corp)) uniqueCorps.Add(corp);
                if (!merger.Contains(corp) && corp != CorpNames.UnClaimed) merger.Add(corp);
            }

            // check for new corp
            if (uniqueCorps.Count == 1 && uniqueCorps[0] == CorpNames.UnClaimed)
            {
                // check for invalid - search for 0 size corps
                avaliable = false;
                foreach (CorpStats corp in corps)
                {
                    if (corp.Size == 0) avaliable = true;
                }

                if (avaliable) return BoardStates.NewCorp;
                else return BoardStates.InvalidNow;
            }

            // check for expansion
            if (uniqueCorps.Count == 1 || (uniqueCorps.Count == 2 && (uniqueCorps[0] == CorpNames.UnClaimed || uniqueCorps[1] == CorpNames.UnClaimed)))
            {
                // this is expansion
                expandedCorp = uniqueCorps[0] == CorpNames.UnClaimed ? uniqueCorps[1] : uniqueCorps[0];
                return BoardStates.ExpandCorp;
            }

            // merge case
            if (merger.Count > 1)
            {
                // check for an invalid merge
                corpsAboveMax = 0;
                foreach (CorpNames corp in merger)
                {
                    if (corps[(int)corp].Size > AcquireConstants.NonMergableCorp) corpsAboveMax++;
                }

                // invalid merge
                if (corpsAboveMax > 1) return BoardStates.InvalidAlways;

                return BoardStates.MergeCorp;
            }

            throw new ArgumentException("Fell through while trying to validate a piece");
        }

        // size of the corrporation
        public int PlaceTile(Square tile)
        {
            board[tile.Dim0, tile.Dim1] = CorpNames.UnClaimed;

            return 0;
        }

        // delta of the increase in size of the corporation
        public int ExpandCorp(Square tile, CorpNames corporation)
        {
            return CreateCorp(tile, corporation);
        }

        // size to the corrporation
        public int CreateCorp(Square tile, CorpNames corporation)
        {
            int corpSize = 0;

            // set ownership of the corporation
            board[tile.Dim0, tile.Dim1] = corporation;
            corpSize++;

            // find surrounding pieces to incorporate
            if (tile.Dim0 > 0 && board[tile.Dim0 - 1, tile.Dim1] == CorpNames.UnClaimed) { board[tile.Dim0 - 1, tile.Dim1] = corporation; corpSize++; }
            if ((tile.Dim0+1) < board.GetLength(0) && board[tile.Dim0 + 1, tile.Dim1] == CorpNames.UnClaimed) { board[tile.Dim0 + 1, tile.Dim1] = corporation;  corpSize++; }
            if (tile.Dim1 > 0 && board[tile.Dim0, tile.Dim1 - 1] == CorpNames.UnClaimed) { board[tile.Dim0, tile.Dim1 - 1] = corporation; corpSize++; }
            if ((tile.Dim1+1) < board.GetLength(1) && board[tile.Dim0, tile.Dim1 + 1] == CorpNames.UnClaimed) { board[tile.Dim0, tile.Dim1 + 1] = corporation; corpSize++; }

            return corpSize;
        }

        // size of the corrporation
        public int MergeCorps(Square tile, CorpNames corporation, CorpNames[] merger)
        {
            int corpSize = 0;

            // claim ownership of the new tile (ensure that unclaimed tiles are encorporated as well)
            ExpandCorp(tile, corporation);
            // don't count these since it will be counted below

            // seek out tiles of the merged corps and convert them
            for (int Dim0 = 0; Dim0 < board.GetLength(0); Dim0++)
            {
                for (int Dim1 = 0; Dim1 < board.GetLength(1); Dim1++)
                {
                    if (board[Dim0, Dim1] == corporation) corpSize++;

                    foreach (CorpNames corp in merger)
                    {
                        if (corp == board[Dim0, Dim1])
                        {
                            board[Dim0, Dim1] = corporation;
                            corpSize++;
                        }
                    }
                }
            }

            return corpSize;
        }
    }

    public struct TradeTransaction
    {
        public PlayerStats Player;
        public BonusType BonusKind;
        public int Bonus;
        public CorpNames ParentCorp;
        public CorpNames MergedCorp;
    }

    public struct SaleTransaction
    {
        public PlayerStats Player;
        public CorpNames Corporation;
        public int Price;
    }

    public static class AcquireConstants
    {
        public const int InitialCash = 6000;
        public const int CorpCount = 7;
        public const int BoardWidth = 12;
        public const int BoardHeight = 12;
        public const int MaxTiles = 5;
        public const int MaxSharePurchase = 3;
        public const int NonMergableCorp = 11;
        public const int MaxShares = 25;
        public const int LargestCorporation = 41;
        public const int MaxPlayers = 6;
    }

    // The game is a state machine
    //                                                                                                     |-rep->|           |---each player---|
    //  |GameSetup| --- 2 to 6 players---|  |PlaceTile| --- causes a merger ---> |ChooseParentCorp| ---> |TradeShares| ---> |SellShares| <------|
    //                                   |   /|\ |                                                                               |
    //                                   |    |  | --- creates a corp ---> |ChooseCorp| ---|                                     |
    //                                   |    |  |                                         |                                     |
    //                                   |    |  | ---> |BuyShares| <----------------------|-------------------------------------|
    //                                   |    |                 |
    //                                   |    |--- |NextTurn|<--|
    //                                   |    |       /|\ |     
    //                                   |-------------|  |     
    //                                        |          \|/    
    //                                        |------|ExitGame|
    //                                                 |
    //                                                \|/
    //                                              |Done|

    public class AcquireGame
    {
        private CorpStats[] corporations;
        private List<PlayerStats> players;
        private AcquireGameStates state;
        private int currentPlayer;
        private Board board;
        private TileCollection tiles;
        private AdditionalMessage message;
        private int gid;

        // state dependent variables
        private List<CorpNames> parentCorps;
        private List<CorpNames> mergedCorps;
        private List<TradeTransaction> tradeShares;
        private List<SaleTransaction> sellShares;
        private List<CorpNames> availableCorps;
        private Square currentTile;
        private CorpNames parentCorporation;

        public AcquireGame(int seed = 0)
        {
            // initialize content
            corporations = new CorpStats[AcquireConstants.CorpCount];
            for (int c = 0; c < AcquireConstants.CorpCount; c++) corporations[c] = new CorpStats((CorpNames)c, 0);

            players = new List<PlayerStats>();
            state = AcquireGameStates.GameSetup;
            currentPlayer = -1;
            tiles = new TileCollection(seed);
            board = new Board();
            parentCorps = null;
            tradeShares = null;
            mergedCorps = null;
            sellShares = null;
            availableCorps = null;
            parentCorporation = CorpNames.NA;
            message = AdditionalMessage.Blank;
            gid = 0;
        }

        // information
        public CorpStats this[CorpNames index]
        {
            get
            {
                return corporations[(int)index];
            }
        }

        public CorpStats[] Corporations
        {
            get
            {
                return corporations;
            }
        }

        public List<PlayerStats> Players
        {
            get
            {
                return players;
            }
        }

        public PlayerStats CurrentPlayer
        {
            get
            {
                return players[currentPlayer];
            }
        }

        public AcquireGameStates State
        {
            get
            {
                return state;
            }
        }

        public CorpNames[,] RawBoard
        {
            get
            {
                return board.RawBoard;
            }
        }

        public AdditionalMessage Message
        {
            get
            {
                return message;
            }
        }

        // state appropriate opporations
        public Square CurrentTile
        {
            get
            {
                return currentTile;
            }
        }

        public List<CorpNames> ParentCorporations
        {
            get
            {
                if (AcquireGameStates.ChooseParentCorp != state) throw new ArgumentException("AcquireGame not in the right stete: " + state);
                return parentCorps;
            }
        }

        public CorpNames ParentCorporation
        {
            get
            {
                if (AcquireGameStates.TradeShares != state 
                    && AcquireGameStates.SellShares != state) throw new ArgumentException("AcquireGame not in the right stete: " + state);
                return parentCorporation;
            }
        }

        public List<CorpNames> AvailableCorporations
        {
            get
            {
                if (AcquireGameStates.ChooseCorp != state) throw new ArgumentException("AcquireGame not in the right stete: " + state);
                return availableCorps;
            }
        }

        public TradeTransaction SharesToTrade
        {
            get
            {
                TradeTransaction? trade;

                if (AcquireGameStates.TradeShares != state) throw new ArgumentException("AcquireGame not in the right stete: " + state);

                if (tradeShares == null || tradeShares.Count == 0) throw new ArgumentException("There are no more TradeTransactions remianing");

                // return the highest ranked one, and shrink the list
                trade = null;
                foreach (TradeTransaction t in tradeShares) if (t.BonusKind == BonusType.Majoriy && trade == null) trade = t;
                foreach (TradeTransaction t in tradeShares) if (t.BonusKind == BonusType.SharedMajority && trade == null) trade = t;
                foreach (TradeTransaction t in tradeShares) if (t.BonusKind == BonusType.Minority && trade == null) trade = t;
                foreach (TradeTransaction t in tradeShares) if (t.BonusKind == BonusType.SharedMinority && trade == null) trade = t;

                if (trade == null) throw new ArgumentException("Failed to find the next trade transation");

                return trade.Value;
            }
        }

        public SaleTransaction SharesToSell
        {
            get
            {
                if (AcquireGameStates.SellShares != state) throw new ArgumentException("AcquireGame not in the right stete: " + state);

                if (sellShares == null || sellShares.Count == 0) throw new ArgumentException("There are no more SaleTransactions remianing");

                return sellShares[0];
            }
        }

        public List<CorpNames>[] SharesToBuy
        {
            get
            {
                List<CorpNames>[] corps;

                if (AcquireGameStates.BuyShares != state) throw new ArgumentException("AcquireGame not in the right stete: " + state);

                // init
                corps = new List<CorpNames>[AcquireConstants.MaxSharePurchase];
                for (int i = 0; i < AcquireConstants.MaxSharePurchase; i++) corps[i] = new List<CorpNames>();

                // find all Corps with MaxSharePurchase+/.../2+/1+ shares and Size > 0
                foreach (CorpStats corp in corporations)
                {
                    for (int i = AcquireConstants.MaxSharePurchase; i > 0; i--)
                        if (corp.Size > 0 && corp.Shares >= i) corps[i-1].Add(corp.Name);
                }

                return corps;
            }
        }

        ///////////////////////////
        // AcquireGameStates.GameSetup
        public int AddPlayer(string name, bool computer)
        {
            PlayerStats player;

            if (AcquireGameStates.GameSetup != state) throw new ArgumentException("AcquireGame not in the right stete: " + state);

            if (players.Count >= AcquireConstants.MaxPlayers) throw new ArgumentException("Cannot exceed " + AcquireConstants.MaxPlayers + " players");

            player = new PlayerStats(name, computer, gid++);
            for (int i = 0; i < AcquireConstants.MaxTiles; i++) player.AddTile(tiles.Next());
            players.Add(player);

            return player.ID;
        }

        public bool StartGame()
        {
            if (AcquireGameStates.GameSetup != state) throw new ArgumentException("AcquireGame not in the right stete: " + state);

            if (players.Count < 2) throw new ArgumentException("Must have at least 2 players to play");

            state = AcquireGameStates.NextTurn;
            AdvancePlayer();

            return true;
        }

        ///////////////////////////
        // AcquireGameStates.ExitGame
        public bool EndGame()
        {
            if (AcquireGameStates.ExitGame != state) throw new ArgumentException("AcquireGame not in the right stete: " + state);

            state = AcquireGameStates.Done;

            return true;
        }

        public bool ContinueGame()
        {
            if (AcquireGameStates.ExitGame != state) throw new ArgumentException("AcquireGame not in the right stete: " + state);

            // move ahead
            AdvancePlayer();

            return true;
        }

        private void AdvancePlayer()
        {
            List<CorpNames> m;
            CorpNames e;
            List<Square> removals;

            if (AcquireGameStates.ExitGame != state && AcquireGameStates.NextTurn != state) throw new ArgumentException("AcquireGame not in the right stete: " + state);

            currentPlayer = (currentPlayer + 1) % players.Count;

            // ensure the player has 5 valid tiles
            removals = new List<Square>();
            while (CurrentPlayer.Tiles.Length < 5 && tiles.Count > 0)
            {
                removals.Clear();
                // remove invalid tiles
                foreach (Square tile in CurrentPlayer.Tiles)
                {
                    if (board.TryPlace(tile, corporations, out m, out e) == BoardStates.InvalidAlways)
                    {
                        removals.Add(tile);
                    }
                }
                foreach (Square tile in removals) CurrentPlayer.RemoveTile(tile);

                // add tiles
                if (tiles.Count > 0) CurrentPlayer.AddTile(tiles.Next());
            }

            state = AcquireGameStates.PlaceTile;
        }

        ///////////////////////////
        // AcquireGameStates.PlaceTile
        public bool ShuffleTiles()
        {
            if (AcquireGameStates.PlaceTile != state) throw new ArgumentException("AcquireGame not in the right stete: " + state);

            // reset the message
            message = AdditionalMessage.Blank;

            // the user is in a place where they do not like their current set of tiles
            foreach (Square tile in CurrentPlayer.Tiles)
            {
                CurrentPlayer.RemoveTile(tile);
                tiles.Push(tile);
            }

            state = AcquireGameStates.BuyShares;

            return true;
        }

        ///////////////////////////
        // AcquireGameStates.NextTurn
        public bool EndTurn()
        {
            int totalShares;
            int totalSize;

            if (AcquireGameStates.NextTurn != state) throw new ArgumentException("AcquireGame not in the right stete: " + state);

            // reset the message
            message = AdditionalMessage.Blank;

            // sanity check the state of the data
            // ensure that the number of shares is a static number
            totalShares = 0;
            totalSize = 0;
            foreach (CorpStats corp in corporations)
            {
                if (corp.Shares < 0 || corp.Shares > AcquireConstants.MaxShares) throw new ArgumentException("Corporation shares is out of range '" + corp.Shares + "'");
                if (corp.Size < 0) throw new ArgumentException("Corporation size is out of range '" + corp.Size + "'");

                totalShares += corp.Shares;
                totalSize += corp.Size;

                foreach (PlayerStats player in players)
                {
                    if (player.Shares(corp.Name) < 0) throw new ArgumentException("'" + player.Name + "' shares of '" + corp.Name + "' are out of range '" + player.Shares(corp.Name) + "'");
                    totalShares += player.Shares(corp.Name);
                }
            }
            if (totalShares != (AcquireConstants.MaxShares * AcquireConstants.CorpCount)) throw new ArgumentException("Totalshares is inconsistent '" + totalShares + "' should be '" + (AcquireConstants.MaxShares * AcquireConstants.CorpCount) + "'");
            // count the corps in the board
            for (int dim0 = 0; dim0 < RawBoard.GetLength(0); dim0++)
                for (int dim1 = 0; dim1 < RawBoard.GetLength(1); dim1++)
                    if (RawBoard[dim0, dim1] != CorpNames.NA && RawBoard[dim0, dim1] != CorpNames.UnClaimed)
                        totalSize--;
            if (totalSize != 0) throw new ArgumentException("The sum of the corporation sizes does not match the board.  The difference is '" + totalSize + "'");

            if (GameOver())
            {
                // change states
                state = AcquireGameStates.ExitGame;
            }
            else
            {
                // move ahead
                AdvancePlayer();
            }

            return true;
        }

        private bool GameOver()
        {
            // There are a few conditions which may warrant the end of the game
            //   Any corporation is over LargestCorporation
            //   The tile collection is empty

            if (AcquireGameStates.NextTurn != state) throw new ArgumentException("AcquireGame not in the right stete: " + state);

            if (tiles.Count == 0) return true;

            // search for large corporations
            foreach (CorpStats corp in corporations)
            {
                if (corp.Size >= AcquireConstants.LargestCorporation) return true;
            }

            return false;
        }

        ///////////////////////////
        // AcquireGameStates.PlaceTile
        public bool StartTurn(Square tile)
        {
            bool tileExists;
            BoardStates tileState;
            List<CorpNames> merged;
            CorpNames expandedCorp;
            int maxSize;

            // set class state
            parentCorps = null;
            tradeShares = null;
            mergedCorps = null;
            sellShares = null;
            availableCorps = new List<CorpNames>();
            currentTile = tile;
            parentCorporation = CorpNames.NA;
            message = AdditionalMessage.Blank;

            if (AcquireGameStates.PlaceTile != state) throw new ArgumentException("AcquireGame not in the right stete: " + state);

            // ensure that the player has this tile to play
            tileExists = false;
            foreach (Square t in players[currentPlayer].Tiles)
            {
                if (tile.Dim0 == t.Dim0 && tile.Dim1 == t.Dim1) tileExists = true;
            }
            if (!tileExists) throw new ArgumentException("This tile is not part of the users tiles");

            // start the move
            tileState = board.TryPlace(tile, corporations, out merged, out expandedCorp);

            // remove the tile, unless it will be used later
            if (tileState != BoardStates.InvalidNow) CurrentPlayer.RemoveTile(tile);

            switch (tileState)
            {
                case BoardStates.InvalidNow:
                    message = AdditionalMessage.NoAvaliableCorps;
                    return false;
                case BoardStates.InvalidAlways:
                    message = AdditionalMessage.InvalidMerger;
                    return false;
                case BoardStates.StandAlone:
                    board.PlaceTile(tile);
                    state = AcquireGameStates.BuyShares;
                    return true;
                case BoardStates.ExpandCorp:
                    int deltaSize = board.ExpandCorp(tile, expandedCorp);
                    corporations[(int)expandedCorp].Size += deltaSize;
                    state = AcquireGameStates.BuyShares;
                    return true;
                case BoardStates.MergeCorp:
                    parentCorps = new List<CorpNames>();
                    mergedCorps = new List<CorpNames>();

                    // find the corp with the largest size and make a collection of possible parents
                    maxSize = -1;
                    foreach (CorpNames corp in merged) if (corporations[(int)corp].Size > maxSize) maxSize = corporations[(int)corp].Size;
                    foreach (CorpNames corp in merged)
                        if (corporations[(int)corp].Size >= maxSize)
                            parentCorps.Add(corp);
                        else
                            mergedCorps.Add(corp);

                    state = AcquireGameStates.ChooseParentCorp;
                    return true;
                case BoardStates.NewCorp:
                     // make a list of the available corporations
                    foreach(CorpStats corp in corporations)
                    {
                        if (corp.Size == 0) availableCorps.Add(corp.Name);
                    }
                    state = AcquireGameStates.ChooseCorp;
                    return true;
            }

            throw new ArgumentException("Failed to take appropriate action in PlayTile");
        }

        ///////////////////////////
        // AcquireGameStates.ChooseParentCorp
        public bool ChooseParentCorp(CorpNames parentCorp)
        {
            int max1, max2;
            int cnt1, cnt2;

            if (AcquireGameStates.ChooseParentCorp != state) throw new ArgumentException("AcquireGame not in the right stete: " + state);

            tradeShares = new List<TradeTransaction>();
            sellShares = new List<SaleTransaction>();
            parentCorporation = parentCorp;

            // build the true merged corporation list
            if (parentCorps.Count > 1)
            {
                foreach (CorpNames corp in parentCorps)
                {
                    if (corp != parentCorp) mergedCorps.Add(corp);
                }
            }

            // award the majority/minority bonus'
            foreach (CorpNames corp in mergedCorps)
            {
                max1 = max2 = -1;
                cnt1 = cnt2 = 0;

                // find the highest and second highest share count
                foreach (PlayerStats player in players)
                {
                    // populate the sell shares list
                    SaleTransaction s;
                    s.Player = player;
                    s.Corporation = corp;
                    s.Price = corporations[(int)corp].Price;
                    sellShares.Add(s);

                    // find the maxes
                    if (player.Shares(corp) > max1) { max2 = max1; max1 = player.Shares(corp); }
                    else if (player.Shares(corp) == max1) { } // nothing
                    else if (player.Shares(corp) > max2) max2 = player.Shares(corp);
                }

                if (max1 == max2) throw new ArgumentException("Majority and minority were identified as the same");

                // count how many people should split the bonus
                foreach (PlayerStats player in players)
                {
                    if (player.Shares(corp) == max1) cnt1++;
                    else if (player.Shares(corp) == max2) cnt2++;
                }

                // give the money
                TradeTransaction t;
                t.MergedCorp = corp;
                t.ParentCorp = parentCorp;
                foreach (PlayerStats player in players)
                {
                    // set player reference
                    t.Player = player;

                    if (player.Shares(corp) == max1)
                    {
                        t.Bonus = corporations[(int)corp].BonusMax / cnt1;
                        t.BonusKind = cnt1 == 1 ? BonusType.Majoriy : BonusType.SharedMajority;
                        player.Cash += corporations[(int)corp].BonusMax / cnt1;
                        tradeShares.Add(t);
                    }
                    else if (player.Shares(corp) == max2)
                    {
                        t.Bonus = corporations[(int)corp].BonusMin / cnt2;
                        t.BonusKind = (cnt2 == 1) ? BonusType.Minority : BonusType.SharedMinority;
                        player.Cash += corporations[(int)corp].BonusMin / cnt2;
                        tradeShares.Add(t);
                    }
                }
            }

            // change state
            state = AcquireGameStates.TradeShares;

            return true;
        }

        ///////////////////////////
        // AcquireGameStates.TradeShares
        public bool TradeShares(int count)
        {
            TradeTransaction trade;

            if (AcquireGameStates.TradeShares != state) throw new ArgumentException("AcquireGame not in the right stete: " + state);

            // grab the trade transaction
            trade = SharesToTrade;

            // sanity check the input
            if ((count % 2) != 0) throw new ArgumentException("Shares to trade must be even");
            if (corporations[(int)trade.ParentCorp].Shares - (count / 2) < 0) throw new ArgumentException("Cannot trade for more shares than exist");

            // make the trade
            // parent corporation
            corporations[(int)trade.ParentCorp].Shares -= (count / 2);
            trade.Player.AddShares(trade.ParentCorp, (count / 2));

            // merged corporation
            corporations[(int)trade.MergedCorp].Shares += count;
            trade.Player.RemoveShares(trade.MergedCorp, count);

            // remove it from the set
            if (!tradeShares.Remove(trade)) throw new ArgumentException("Failed to remove the trade");

            if (tradeShares.Count == 0) state = AcquireGameStates.SellShares;

            return true;
        }

        ///////////////////////////
        // AcquireGameStates.SellShares
        public bool SellShares(int count)
        {
            int corpSize;
            SaleTransaction sale;

            if (AcquireGameStates.SellShares != state) throw new ArgumentException("AcquireGame not in the right stete: " + state);

            // grab the sale transaction
            sale = SharesToSell;

            // sanity check the data
            if (sale.Player.Shares(sale.Corporation) < count) throw new ArgumentException("Cannot sell more shares than '" + sale.Player.Name + "' has");

            // make the sale
            if (count != 0)
            {
                corporations[(int)sale.Corporation].Shares += count;
                sale.Player.RemoveShares(sale.Corporation, count);
                sale.Player.Cash += count * sale.Price;
            }

            // remove this from the set
            if (!sellShares.Remove(sale)) throw new ArgumentException("Failed to remove the sale");

            // change state once everyone is done
            if (sellShares.Count == 0)
            {
                // merge the corporations
                corpSize = board.MergeCorps(CurrentTile, ParentCorporation, mergedCorps.ToArray());
                corporations[(int)ParentCorporation].Size = corpSize;

                // clear out any merger information
                // Reset the sizes for the merged corporations
                foreach (CorpNames c in mergedCorps)
                {
                    corporations[(int)c].Size = 0;
                }

                // advance the state
                state = AcquireGameStates.BuyShares;
            }

            return true;
        }

        ///////////////////////////
        // AcquireGameStates.ChooseCorp
        public bool ChooseCorp(CorpNames corp)
        {
            int corpSize;

            if (AcquireGameStates.ChooseCorp != state) throw new ArgumentException("AcquireGame not in the right stete: " + state);

            corpSize = board.CreateCorp(CurrentTile, corp);
            corporations[(int)corp].Size = corpSize;

            // add a share to the creator of the new corporation
            if (corporations[(int)corp].Shares > 0)
            {
                CurrentPlayer.AddShares(corp, 1);
                corporations[(int)corp].Shares -= 1;
            }

            state = AcquireGameStates.BuyShares;

            return true;
        }

        ///////////////////////////
        // AcquireGameStates.BuyShares
        public bool BuyShares(CorpNames[] corps)
        {
            int cost = 0;

            if (AcquireGameStates.BuyShares != state) throw new ArgumentException("AcquireGame not in the right stete: " + state);

            // reset the message
            message = AdditionalMessage.Blank;

            // do some sanity checks
            if (corps.Length > AcquireConstants.MaxSharePurchase) throw new ArgumentException("Must pass in " + AcquireConstants.MaxSharePurchase + " shares to buy");
            for (int i = 0; i < corps.Length; i++)
            {
                if (corps[i] != CorpNames.NA && corps[i] != CorpNames.UnClaimed)
                {
                    cost += corporations[(int)corps[i]].Price;
                    if (corporations[(int)corps[i]].Shares < 1) throw new ArgumentException("Insufficent shares in " + corps[i]);
                }
            }
            if (cost > CurrentPlayer.Cash)
            {
                message = AdditionalMessage.InsufficientFunds;
                return false;
            }
            // TODO! ensure that there are enough shares if more than 1 corp is passed in
            
            // make the transaction
            for (int i = 0; i < corps.Length; i++)
            {
                if (corps[i] != CorpNames.NA && corps[i] != CorpNames.UnClaimed)
                {
                    // subtract the cash
                    CurrentPlayer.Cash -= corporations[(int)corps[i]].Price;
                    // remove the shares from the pool
                    corporations[(int)corps[i]].Shares--;
                    // add to the players shares
                    CurrentPlayer.AddShares(corps[i], 1);
                }
            }

            state = AcquireGameStates.NextTurn;

            return true;
        }
    }

}
