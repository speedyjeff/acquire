using System;

using Acquire.Engine;

namespace AcquireX
{
    // any updates to this require an addition to playerNames (in .ctor) and create an object (in StartGame)
    public enum Oppenents { Human = 0, Random = 1, Computer2 = 2, Computer3 = 3 };

    class WinStats
    {
        public string Name;
        public int Wins;
        public long NetWorth;
    }

    public partial class MainPage : ContentPage
    {
        Button[,] squares;
        AcquireGame game;
        Dictionary<int, IComputer> computers;
        IComputer lastComputer;
        AcquireGameStates lastState;
        List<string> cycleQueue;
        int oneThread;
        Dictionary<string, Oppenents> playerNames;
        bool repeatAgain;
        Dictionary<int, WinStats> lifetime;

        // UI handle names
        private const string Lshares = "_Shares";
        private const string Lprice = "_Price";
        private const string Lsize = "_Size";
        private const string Lbonus = "_Bonus";
        private const string Lcolor = "_Square";
        private const string Lcount = "_Count";

        private const string Lname = "PlayerName";
        private const string Lcash = "Cash";
        private const string Lnetworth = "NetWorth";

        private const string Lselected = "Selected";
        private const string Lselection = "Selection";

        private const string Lsharecost = "ShareCost";

        private string[] Lbuycorp = new string[] { "BuyCorp1_List", "BuyCorp2_List", "BuyCorp3_List" };

        private const string Lnewcorp = "NewCorp_List";

        private const string Lstartbase = "Player";
        private const string Lstartstatus = "StartStatus";
        private const string Lstartlist = "_List";
        private const string Lstarttext = "_Text";
        private const string Lstarterr = "StartErr_Message";

        private const string Lstatus = "Status";
        private const string Lstatustag = "_Message";
        private const string Laddmsg = "AdditionalMessage";

        private const string Ltradename = "TradeName";
        private const string Ltradecorp = "TradeCorp";
        private const string Ltradelist = "TradeNumber_List";
        private const string Ltraderes = "TradeResult";
        private const string Ltradebonuskind = "TradeBonusKind";
        private const string Ltradebonus = "TradeBonusAmount";

        private const string Lsellname = "SellName";
        private const string Lsellcorp = "SellCorp";
        private const string Lselllist = "SellNumber_List";
        private const string Lsellprice = "SellPrice";

        private const string Ldoneplayer = "Player";
        private const string Ldonename = "_Name";
        private const string Ldonenetworth = "_NetWorth";
        private const string Ldonecash = "_Cash";
        private const string Ldonesep = "_";
        private const string Ldonewinner = "WinnerName";

        private const string Lwinbase = "WinPlayer";
        private const string Lwinname = "_Name";
        private const string Lwinwin = "_Wins";
        private const string Lwinnetworth = "_NetWorth";

        public MainPage()
        {
            InitializeComponent();

            // track the squres
            squares = new Button[AcquireConstants.BoardHeight, AcquireConstants.BoardWidth];
            for (int dim0 = 0; dim0 < AcquireConstants.BoardHeight; dim0++)
            {
                for (int dim1 = 0; dim1 < AcquireConstants.BoardWidth; dim1++)
                {
                    squares[dim0, dim1] = (Button)this.FindByName( $"rectangle{((dim1 + 1) + (dim0 * 12))}");

                    if (null == squares[dim0, dim1]) throw new NullReferenceException("Square " + dim0 + " x " + dim1 + " is null!");
                }
            }

            // init data
            computers = new Dictionary<int, IComputer>();
            lastComputer = null;
            lastState = AcquireGameStates.Done;
            cycleQueue = new List<string>();
            oneThread = 0;
            playerNames = new Dictionary<string, Oppenents>();
            playerNames.Add("Human", Oppenents.Human);
            playerNames.Add("Computer Easy", Oppenents.Random);
            playerNames.Add("Computer Hard", Oppenents.Computer2);
            playerNames.Add("Computer Very Hard", Oppenents.Computer3);
            game = null;
            GetItem<Label>(Laddmsg).Text = "";  // fix up the bad looking red block
            repeatAgain = false;
            lifetime = new Dictionary<int, WinStats>();

            // initialize the startup screen
            for (int i = 1; i <= AcquireConstants.MaxPlayers; i++)
            {
                string basename = Lstartbase + i;
                GetItem<Picker>(basename, Lstartlist).Items.Clear();
                foreach (string name in playerNames.Keys)
                {
                    GetItem<Picker>(basename, Lstartlist).Items.Add(name);
                }

                // set the defaults
                if (i == 1) GetItem<Picker>(basename, Lstartlist).SelectedIndex = 0;
                if (i == 2) GetItem<Picker>(basename, Lstartlist).SelectedIndex = 1;
            }

            // start the AI thread
            /*
            Device.StartTimer(
                interval: new TimeSpan(0, 0, 0, 0, 100),
                () =>
                {
                    AdvanceAI_Callback();
                    // run again
                    return true;
                });
            */

            var timer = Dispatcher.CreateTimer();
            timer.Tick += AdvanceAI_Callback;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timer.Start();
        }

        // helpers
        private IComputer GetComputer(PlayerStats player)
        {
            // initialize the common properties
            computers[player.ID].Player = player;
            computers[player.ID].Board = game.RawBoard;
            computers[player.ID].Corporations = game.Corporations;
            computers[player.ID].ID = player.ID;
            computers[player.ID].NumberPlayers = game.Players.Count;
            return computers[player.ID];
        }

        private bool GetCoords(Button r, out int dim0, out int dim1)
        {
            dim0 = dim1 = -1;

            for (int td0 = 0; td0 < AcquireConstants.BoardHeight; td0++)
            {
                for (int td1 = 0; td1 < AcquireConstants.BoardWidth; td1++)
                {
                    if (r.Equals(squares[td0, td1]))
                    {
                        dim0 = td0;
                        dim1 = td1;

                        return true;
                    }
                }
            }

            return false;
        }

        private T GetItem<T>(string name)
        {
            return (T)this.FindByName(name);
        }

        private T GetItem<T>(CorpNames corp, string type)
        {
            return (T)this.FindByName(corp.ToString() + type);
        }

        private T GetItem<T>(string name, string type)
        {
            return (T)this.FindByName(name + type);
        }

        private CorpNames ToCorpNames(object value)
        {
            if (Enum.TryParse(typeof(CorpNames), value as string, out object result)) return (CorpNames)result;
            throw new Exception($"failed to parse CorpsName : {value}");
        }

        // AI logic
        private void AdvanceAI()
        {
            IComputer computer = null;
            bool cycle = false;
            AcquireGameStates currentState;

            // early out
            if (game == null || game.State == AcquireGameStates.GameSetup) return;

            // get the computer implementation (if this is a computer player)
            if (game.State == AcquireGameStates.TradeShares)
            {
                if (game.SharesToTrade.Player.IsComputer) computer = GetComputer(game.SharesToTrade.Player);
            }
            else if (game.State == AcquireGameStates.SellShares)
            {
                if (game.SharesToSell.Player.IsComputer) computer = GetComputer(game.SharesToSell.Player);
            }
            else if (game.CurrentPlayer.IsComputer)
            {
                computer = GetComputer(game.CurrentPlayer);
            }

            // not a computer player, so exit early
            if (computer == null)
            {
                // nothing
                lastComputer = null;
                return;
            }

            // guard against re-entrancy
            if (System.Threading.Interlocked.CompareExchange(ref oneThread, 1, 0) != 0) return;

            // rudimentry cycle detection, does not allow the same computer to be called twice
            cycle = (lastComputer == computer && lastState == game.State);
            currentState = game.State; // need to cache since state will change below

            // for cycle detection trade what has happened in the past
            cycleQueue.Add(computer.ID + " " + currentState);

            // plan next move
            switch (game.State)
            {
                case AcquireGameStates.ExitGame:
                    // if there are only computers playing then Exit, else let the human player decide
                    if (computers.Count == game.Players.Count)
                    {
                        EndGame();
                    }
                    else
                    {
                        ContinueGame();
                    }
                    break;
                case AcquireGameStates.Done:
                    // if only the computers than repeat again
                    if (computers.Count == game.Players.Count && repeatAgain)
                    {
                        StartGame();
                    }
                    break;
                case AcquireGameStates.PlaceTile:
                    // have the computer choose a piece
                    if (cycle) ShuffleTiles();
                    else SelectSquare(computer.SelectSquare(game.CurrentPlayer.Tiles));
                    break;
                case AcquireGameStates.BuyShares:
                    // have the current player buy shares
                    if (cycle) BuyShares(new CorpNames[0]);
                    else BuyShares(computer.BuyShares(game.SharesToBuy));
                    break;
                case AcquireGameStates.ChooseCorp:
                    // have the current choose a corp
                    if (cycle) throw new ArgumentException("Cycle detected while choosing a corp");
                    else ChooseCorp(computer.ChooseCorp(game.AvailableCorporations));
                    break;
                case AcquireGameStates.ChooseParentCorp:
                    // have the current choose a parent corp
                    if (cycle) throw new ArgumentException("Cycle detected while choosing a parent corp");
                    else ChooseCorp(computer.ChooseParentCorp(game.ParentCorporations));
                    break;
                case AcquireGameStates.NextTurn:
                    // nothing
                    break;
                case AcquireGameStates.SellShares:
                    // have the 'sell' player choose how many shares to sell
                    cycleQueue.Add(" - " + game.SharesToSell.Corporation + " " + game.SharesToSell.Price + " " + game.SharesToSell.Player.Name + " " + game.SharesToSell.Player.IsComputer);
                    // TODO! cycle detection in selling will require saving the last trade and ensuring that it is not the same
                    /*if (cycle) throw new ArgumentException("Cycle detected while selling shares");
                    else */
                    SellShares(computer.SellShares(game.SharesToSell));
                    break;
                case AcquireGameStates.TradeShares:
                    // have the 'trade' player choose how many shares to sell
                    cycleQueue.Add(" - " + game.SharesToTrade.ParentCorp + "/" + game.SharesToTrade.MergedCorp + " " + game.SharesToTrade.Bonus + " " + game.SharesToTrade.Player.Name + " " + game.SharesToTrade.Player.IsComputer);
                    // TODO! cycle detection in trading will require saving the last trade and ensuring that it is not the same
                    /*if (cycle) throw new ArgumentException("Cycle detected while trading shares");
                    else*/
                    TradeShares(computer.TradeShares(game.SharesToTrade, game[game.SharesToTrade.ParentCorp].Shares));
                    break;
            }

            // cycle detection
            lastState = currentState; // need to cache the state since it will change above
            lastComputer = computer;

            if (System.Threading.Interlocked.CompareExchange(ref oneThread, 0, 1) != 1) throw new ArgumentException("Lock released more than once");
        }

        // UI and game state
        private void AdvanceUI()
        {
            Picker corpList;

            // plan next move
            switch (game.State)
            {
                case AcquireGameStates.ExitGame:
                    // show UI
                    if (!game.CurrentPlayer.IsComputer)
                    {
                        ExitGameGrid.IsVisible = true; 
                    }
                    break;
                case AcquireGameStates.Done:
                    // show the UI
                    ExitGameGrid.IsVisible = false;
                    FinalDisplayGrid.IsVisible = true;

                    // display the final results
                    DisplayFinalResults(game.Players, game.Corporations);
                    break;
                case AcquireGameStates.PlaceTile:
                    // show UI
                    ExitGameGrid.IsVisible = false;
                    break;
                case AcquireGameStates.BuyShares:
                    // populate the pull downs for the shares to purchase

                    // hide UI
                    SellGrid.IsVisible = false;
                    ChooseCorpGrid.IsVisible = false;

                    if (!game.CurrentPlayer.IsComputer)
                    {
                        Picker buylist;
                        List<CorpNames>[] corps;

                        // show the UI
                        BuyGrid.IsVisible = true;

                        // populate the information
                        corps = game.SharesToBuy;
                        for (int i = 0; i < corps.Length; i++)
                        {
                            buylist = GetItem<Picker>(Lbuycorp[i]);
                            buylist.Items.Clear();
                            buylist.Items.Add(CorpNames.NA.ToString());
                            buylist.SelectedIndex = 0;
                            foreach (CorpNames corp in corps[i])
                            {
                                buylist.Items.Add(corp.ToString());
                            }
                        }
                    }
                    break;
                case AcquireGameStates.ChooseCorp:
                    // populate the pull down with avaliable corporations
                    if (!game.CurrentPlayer.IsComputer)
                    {
                        // show UI
                        ChooseCorpGrid.IsVisible = true;

                        // display information
                        corpList = GetItem<Picker>(Lnewcorp);
                        corpList.Items.Clear();
                        foreach (CorpNames corp in game.AvailableCorporations)
                        {
                            corpList.Items.Add(corp.ToString());
                        }
                        corpList.SelectedIndex = 0;
                    }
                    break;
                case AcquireGameStates.ChooseParentCorp:
                    // populate the pull down with avaliable corporations for the merger
                    if (!game.CurrentPlayer.IsComputer)
                    {
                        // show UI
                        ChooseCorpGrid.IsVisible = true;

                        // display information
                        corpList = GetItem<Picker>(Lnewcorp);
                        corpList.Items.Clear();
                        foreach (CorpNames corp in game.ParentCorporations)
                        {
                            corpList.Items.Add(corp.ToString());
                        }
                        corpList.SelectedIndex = 0;
                    }
                    break;
                case AcquireGameStates.NextTurn:
                    // turn off UI
                     BuyGrid.IsVisible = false;

                    // end turn
                    game.EndTurn();

                    if (game.State == AcquireGameStates.ExitGame && !game.CurrentPlayer.IsComputer)
                    {
                         ExitGameGrid.IsVisible = true;
                    }
                    break;
                case AcquireGameStates.SellShares:
                    // NOTE! This section is reintrant... it is called multiple times to get answers from everyone

                    // hide UI
                    TradeGrid.IsVisible = false;

                    if (!game.SharesToSell.Player.IsComputer)
                    {
                        // show UI
                        SellGrid.IsVisible = true;

                        // display user information
                        GetItem<Label>(Lsellname).Text = game.SharesToSell.Player.Name;
                        GetItem<Label>(Lsellcorp).Text = $"{game.SharesToSell.Corporation}";
                        GetItem<Label>(Lsellprice).Text = "$" + game.SharesToSell.Price;
                        GetItem<Picker>(Lselllist).Items.Clear();
                        for (int shares = 0; shares <= game.SharesToSell.Player.Shares(game.SharesToSell.Corporation); shares++)
                        {
                            GetItem<Picker>(Lselllist).Items.Add($"{shares}");
                        }
                        GetItem<Picker>(Lselllist).SelectedIndex = 0;
                    }

                    Refresh(game.SharesToSell.Player);
                    break;
                case AcquireGameStates.TradeShares:
                    // NOTE! This section is reintrant... it is called multiple times to get answers from everyone

                    // hide UI
                    ChooseCorpGrid.IsVisible = false;

                    if (!game.SharesToTrade.Player.IsComputer)
                    {
                        // show UI
                        TradeGrid.IsVisible = true;

                        // display the user information
                        GetItem<Label>(Ltradename).Text = game.SharesToTrade.Player.Name;
                        GetItem<Label>(Ltradecorp).Text = game.SharesToTrade.MergedCorp + "/" + game.SharesToTrade.ParentCorp;
                        GetItem<Label>(Ltraderes).Text = "";
                        GetItem<Label>(Ltradebonuskind).Text = $"{game.SharesToTrade.BonusKind}";
                        GetItem<Label>(Ltradebonus).Text = "$" + game.SharesToTrade.Bonus;
                        GetItem<Picker>(Ltradelist).Items.Clear();
                        for (int shares = 0; shares <= game.SharesToTrade.Player.Shares(game.SharesToTrade.MergedCorp) && (shares / 2) <= game[game.SharesToTrade.ParentCorp].Shares; shares += 2)
                        {
                            GetItem<Picker>(Ltradelist).Items.Add($"{shares}");
                        }
                        GetItem<Picker>(Ltradelist).SelectedIndex = 0;
                    }

                    Refresh(game.SharesToTrade.Player);
                    break;
            }

            if (game.State != AcquireGameStates.SellShares && game.State != AcquireGameStates.TradeShares)
            {
                // update the board
                Refresh(game.CurrentPlayer);
            }
        }

        private void DisplayLifetime()
        {
            int cnt;

            // hide UI
            FinalDisplayGrid.IsVisible = false;
            WinGrid.IsVisible = true;

            // clear them all out
            for (int i = 1; i <= AcquireConstants.MaxPlayers; i++)
            {
                string basename = Lwinbase + i;

                GetItem<Label>(basename, Lwinname).Text = "";
                GetItem<Label>(basename, Lwinwin).Text = "0";
                GetItem<Label>(basename, Lwinnetworth).Text = "$0";
            }

            if (lifetime.Count > 0)
            {
                cnt = 1;
                foreach (PlayerStats player in game.Players)
                {
                    int id = player.ID;
                    string basename = Lwinbase + cnt;

                    GetItem<Label>(basename, Lwinname).Text = lifetime[id].Name;
                    GetItem<Label>(basename, Lwinwin).Text = $"{lifetime[id].Wins}";
                    GetItem<Label>(basename, Lwinnetworth).Text = "$" + lifetime[id].NetWorth;

                    cnt++;
                }
            }
        }

        private void DisplayFinalResults(List<PlayerStats> players, CorpStats[] corps)
        {
            PlayerStats winner = null;
            int maxNetworth;
            int networth;
            int cnt;

            // hide UI
            WinGrid.IsVisible = false;

            // clear them all out
            for (int i = 1; i <= AcquireConstants.MaxPlayers; i++)
            {
                string basename = Ldoneplayer + i;

                GetItem<Label>(basename, Ldonename).Text = "";
                GetItem<Label>(basename, Ldonecash).Text = "$";

                foreach (CorpStats corp in corps)
                {
                    GetItem<Label>(basename, Ldonesep + corp.Name).Text = "0";
                }
                GetItem<Label>(basename, Ldonenetworth).Text = "$";
            }

            // calculate networth and display
            maxNetworth = -1;
            cnt = 1;
            foreach (PlayerStats player in players)
            {
                string basename = Ldoneplayer + cnt;

                if (!lifetime.ContainsKey(player.ID))
                {
                    WinStats win = new WinStats();
                    win.Name = player.Name;
                    win.Wins = 0;
                    win.NetWorth = 0;
                    lifetime.Add(player.ID, win);
                }

                // display the player information
                GetItem<Label>(basename, Ldonename).Text = player.Name;
                GetItem<Label>(basename, Ldonecash).Text = "$" + player.Cash;

                networth = 0;
                foreach (CorpStats corp in corps)
                {
                    GetItem<Label>(basename, Ldonesep + corp.Name).Text = $"{player.Shares(corp.Name)}";
                    networth += player.Shares(corp.Name) * corp.Price;
                }
                networth += player.Cash;
                GetItem<Label>(basename, Ldonenetworth).Text = "$" + networth;
                lifetime[player.ID].NetWorth += networth;

                if (networth > maxNetworth)
                {
                    winner = player;
                    maxNetworth = networth;
                }

                cnt++;
            }

            GetItem<Label>(Ldonewinner).Text = winner.Name;
            lifetime[winner.ID].Wins++;
        }

        private void Refresh(PlayerStats player)
        {
            CorpNames[,] board;
            int networth;

            // debug only
            //DisplayFinalResults(game.Players, game.Corporations);

            // update status
            GetItem<Label>(Lstatus).Text = GetItem<Label>(game.State + Lstatustag).Text;
            if (game.Message == Acquire.Engine.AdditionalMessage.Blank)
            {
                GetItem<Label>(Laddmsg).Text = "";
            }
            else
            {
                var label = GetItem<Label>(Laddmsg);
                label.Text = GetItem<Label>(game.Message + Lstatustag).Text;
            }

            // update corp stats
            for (int c = 0; c < AcquireConstants.CorpCount; c++)
            {
                CorpNames corp = (CorpNames)c;
                CorpStats stats = game[corp];

                GetItem<Label>(corp, Lshares).Text = $"{stats.Shares}";
                GetItem<Label>(corp, Lprice).Text = "$" + stats.Price;
                GetItem<Label>(corp, Lsize).Text = $"{stats.Size}";
                GetItem<Label>(corp, Lbonus).Text = "$" + stats.BonusMax + "/$" + stats.BonusMin;
            }

            // update player stats
            PlayerStats pstats = player;
            GetItem<Label>(Lname).Text = pstats.Name;
            GetItem<Label>(Lcash).Text = pstats.IsComputer ? "$???" : "$" + pstats.Cash;
            for (int c = 0; c < AcquireConstants.CorpCount; c++)
            {
                CorpNames corp = (CorpNames)c;

                GetItem<Label>(corp, Lcount).Text = pstats.IsComputer ? "?" : pstats.Shares(corp).ToString();
            }

            // calculate the networth
            networth = 0;
            foreach (CorpStats corp in game.Corporations)
            {
                networth += player.Shares(corp.Name) * corp.Price;
            }
            networth += player.Cash;
            GetItem<Label>(Lnetworth).Text = pstats.IsComputer ? "$???" : "$" + networth;

            // paint the board
            board = game.RawBoard;
            for (int dim0 = 0; dim0 < board.GetLength(0); dim0++)
            {
                for (int dim1 = 0; dim1 < board.GetLength(1); dim1++)
                {
                    squares[dim0, dim1].BackgroundColor = GetItem<Button>(board[dim0, dim1], Lcolor).BackgroundColor;
                }
            }

            // display the possible squares
            if (!game.CurrentPlayer.IsComputer)
            {
                foreach (Square tile in game.CurrentPlayer.Tiles)
                {
                    squares[tile.Dim0, tile.Dim1].BackgroundColor = GetItem<Button>(Lselection, Lcolor).BackgroundColor;
                }
            }

            // display the selected tile
            if (game.State != AcquireGameStates.PlaceTile)
            {
                squares[game.CurrentTile.Dim0, game.CurrentTile.Dim1].BackgroundColor = GetItem<Button>(Lselected, Lcolor).BackgroundColor;
            }
        }

        // manipulation
        private void StartGame()
        {
            int id;
            List<string[]> players;

            // grab the player names and algorithms for each player
            players = new List<string[]>();

            for (int i = 1; i <= AcquireConstants.MaxPlayers; i++)
            {
                string basename = Lstartbase + i;
                if (null != GetItem<Picker>(basename, Lstartlist).SelectedItem)
                {
                    string[] npair = new string[2];
                    npair[0] = (string)GetItem<Picker>(basename, Lstartlist).SelectedItem;
                    npair[1] = (string)GetItem<Entry>(basename, Lstarttext).Text;
                    players.Add(npair);
                }
            }

            if (players.Count < 2)
            {
                GetItem<Label>(Lstartstatus).Text = (string)GetItem<Label>(Lstarterr).Text;
                return;
            }

#if PREDICTABLE_EXECUTION
            var RandomSeed = 123456;
#else
            var RandomSeed = 0;
#endif

            // start Acquire game engine
            game = new AcquireGame(RandomSeed);

            // init game state
            computers.Clear();
            cycleQueue.Clear();

            // add the players
            foreach (string[] npairs in players)
            {
                if (npairs.Length != 2) throw new ArgumentException("Wrong size of the internal data structure");
                if (!playerNames.ContainsKey(npairs[0])) throw new ArgumentException("Missing a player implementation");

                id = game.AddPlayer(npairs[1], playerNames[npairs[0]] != Oppenents.Human);

                switch (playerNames[npairs[0]])
                {
                    case Oppenents.Random:
                        computers.Add(id, new Computer1(RandomSeed));
                        break;
                    case Oppenents.Computer2:
                        computers.Add(id, new Computer2());
                        break;
                    case Oppenents.Computer3:
                        computers.Add(id, new Computer3());
                        break;
                }
            }

            // hide/show the UI
            StartGrid.IsVisible = false;
            FinalDisplayGrid.IsVisible = false;
            GameGrid.IsVisible = true;
            if (lifetime.Count > 0 && (computers.Count == game.Players.Count))
            {
                // only display if there are computer players
                // TODO - not sure when this is supposed to show
                //DisplayLifetime();
            }

            // start the game
            game.StartGame();

            // update the UI
            Refresh(game.CurrentPlayer);
        }

        private void SelectSquare(Square square)
        {
            SelectSquare(square.Dim0, square.Dim1);
        }

        private void SelectSquare(int dim0, int dim1)
        {
            if (game.State != AcquireGameStates.PlaceTile) return;

            // check if this is a square which the player has already
            foreach (Square tile in game.CurrentPlayer.Tiles)
            {
                if (tile.Dim0 == dim0 && tile.Dim1 == dim1)
                {
                    game.StartTurn(tile);
                    break;
                }
            }

            // move ahead
            AdvanceUI();
        }

        private void BuyShares(CorpNames[] corps)
        {
            if (game.State != AcquireGameStates.BuyShares) return;

            // place the order
            game.BuyShares(corps);

            // move ahead
            AdvanceUI();
        }

        private void ChooseCorp(CorpNames corp)
        {
            if (game.State == AcquireGameStates.ChooseCorp)
            {
                // choose a corporation
                game.ChooseCorp(corp);
            }
            else if (game.State == AcquireGameStates.ChooseParentCorp)
            {
                // choose a corporation
                game.ChooseParentCorp(corp);
            }
            else
            {
                return;
            }

            // move ahead
            AdvanceUI();
        }

        private void ShuffleTiles()
        {
            if (game.State != AcquireGameStates.PlaceTile) return;

            game.ShuffleTiles();

            // move ahead
            AdvanceUI();
        }

        private void EndTurn()
        {
            if (game.State != AcquireGameStates.NextTurn) return;

            game.EndTurn();

            // move ahead
            AdvanceUI();
        }

        private void TradeShares(int count)
        {
            if (game.State != AcquireGameStates.TradeShares) return;

            // make the trade
            game.TradeShares(count);

            // move ahead
            AdvanceUI();
        }

        private void SellShares(int count)
        {
            if (game.State != AcquireGameStates.SellShares) return;

            // make the sale
            game.SellShares(count);

            // move ahead
            AdvanceUI();
        }

        private void ContinueGame()
        {
            if (game.State != AcquireGameStates.ExitGame) return;

            game.ContinueGame();

            // move ahead
            AdvanceUI();
        }

        private void EndGame()
        {
            if (game.State != AcquireGameStates.ExitGame) return;

            game.EndGame();

            // move ahead
            AdvanceUI();
        }

        // handlers
        private void AdvanceAI_Callback(object sender, EventArgs e)
        {
            AdvanceAI();
        }

        private void Square_MouseLeftButtonDown(object sender, EventArgs e)
        {
            Button r = (Button)sender;
            int dim0, dim1;

            if (GetCoords(r, out dim0, out dim1))
            {
                SelectSquare(dim0, dim1);
            }
        }

        private void Square_MouseEnter(object sender, FocusEventArgs e)
        {
        }

        private void Square_MouseLeave(object sender, FocusEventArgs e)
        {
        }

        private void BuyButton_Click(object sender, EventArgs e)
        {
            CorpNames[] corps = new CorpNames[AcquireConstants.MaxSharePurchase];

            // collect up purchaes
            for (int i = 0; i < AcquireConstants.MaxSharePurchase; i++)
            {
                if (null == GetItem<Picker>(Lbuycorp[i]).SelectedItem) corps[i] = CorpNames.NA;
                else corps[i] = ToCorpNames(GetItem<Picker>(Lbuycorp[i]).SelectedItem);
            }

            BuyShares(corps);
        }

        private void NewCorpButton_Click(object sender, EventArgs e)
        {
            if (null == GetItem<Picker>(Lnewcorp).SelectedItem) return;

            ChooseCorp(ToCorpNames(GetItem<Picker>(Lnewcorp).SelectedItem));
        }

        private void BuyCorp_List_SelectionChanged(object sender, EventArgs e)
        {
            int cost = 0;

            if (game.State != AcquireGameStates.BuyShares) return;

            // add up the cost
            for (int i = 0; i < AcquireConstants.MaxSharePurchase; i++)
            {
                if (null != GetItem<Picker>(Lbuycorp[i]).SelectedItem && CorpNames.NA != ToCorpNames(GetItem<Picker>(Lbuycorp[i]).SelectedItem))
                    cost += game[ToCorpNames(GetItem<Picker>(Lbuycorp[i]).SelectedItem)].Price;
            }

            // post the cost
            GetItem<Label>(Lsharecost).Text = "$" + cost;
        }

        private void TradeNumber_List_SelectionChanged(object sender, EventArgs e)
        {
            if (game.State != AcquireGameStates.TradeShares) return;

            if (null != GetItem<Picker>(Ltradelist).SelectedItem)
            {
                if (Int32.TryParse(GetItem<Picker>(Ltradelist).SelectedItem as string, out int val))
                {
                    GetItem<Label>(Ltraderes).Text = $"{(val / 2)}";
                }
                else throw new Exception("not able to convert trade to int");
            }
        }

        private void SellNumber_List_SelectionChanged(object sender, EventArgs e)
        {
            if (game.State != AcquireGameStates.SellShares) return;

            if (null != GetItem<Picker>(Lselllist).SelectedItem)
            {
                if (Int32.TryParse(GetItem<Picker>(Lselllist).SelectedItem as string, out int val))
                {
                    GetItem<Label>(Lsellprice).Text = "$" + (val * game.SharesToSell.Price);
                }
                else throw new Exception("not able to convert sell to int");
            }
        }

        private void TradeButton_Click(object sender, EventArgs e)
        {
            if (GetItem<Picker>(Ltradelist).SelectedItem == null)
            {
                TradeShares(0);
            }
            else if (Int32.TryParse(GetItem<Picker>(Ltradelist).SelectedItem as string, out int val))
            {
                TradeShares(val);
            }
            else throw new Exception("not able to convert trade to int");
        }

        private void SellButton_Click(object sender, EventArgs e)
        {
            if (GetItem<Picker>(Lselllist).SelectedItem == null)
            {
                SellShares(0);
            }
            else if (Int32.TryParse(GetItem<Picker>(Lselllist).SelectedItem as string, out int val))
            {
                SellShares(val);
            }
            else throw new Exception("not able to convert sell to int");
        }

        private void NoButton_Click(object sender, EventArgs e)
        {
            ContinueGame();
        }

        private void YesButton_Click(object sender, EventArgs e)
        {
            EndGame();
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            // reset the pull downs
            AdvanceUI();
        }

        private void ShuffleTilesButton_Click(object sender, EventArgs e)
        {
            ShuffleTiles();
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            // hide the win grid
            FinalDisplayGrid.IsVisible = false;
            WinGrid.IsVisible = false;

            // set repeat
            repeatAgain = false;

            // start game
            StartGame();
        }

        private void StartOverButton_Click(object sender, EventArgs e)
        {
            // show UI
            FinalDisplayGrid.IsVisible = false;
            StartGrid.IsVisible = true;
            WinGrid.IsVisible = false;
        }

        private void RepeatButton_Click(object sender, EventArgs e)
        {
            // set repeat
            repeatAgain = true;

            // start game
            StartGame();
        }
    }
}