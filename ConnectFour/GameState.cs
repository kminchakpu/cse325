
namespace ConnectFour;

public class GameState
{
    static GameState()
    {
        CalculateWinningPlaces();
    }

    public enum WinState
    {
        No_Winner = 0,
        Player1_Wins = 1,
        Player2_Wins = 2,
        Tie = 3
    }

    public class GameRecord
    {
        public int GameNumber { get; set; }
        public string Result { get; set; } = string.Empty;
        public DateTime CompletedAt { get; set; }
    }

    // Current game information
    public int PlayerTurn => TheBoard.Count(x => x != 0) % 2 + 1;

    public int CurrentTurn => TheBoard.Count(x => x != 0);

    // Game scores
    public int Player1Wins { get; private set; }

    public int Player2Wins { get; private set; }

    public int Draws { get; private set; }

    public int TotalGames => Player1Wins + Player2Wins + Draws;

    // Game history
    private readonly List<GameRecord> _gameHistory = new();

    public IReadOnlyList<GameRecord> GameHistory => _gameHistory;

    // Game appearance settings
    public string BoardColor { get; set; } = "#000000";

    public string Player1Color { get; set; } = "#008000";

    public string Player2Color { get; set; } = "#800080";

    // Winning combinations
    public static readonly List<int[]> WinningPlaces = new();

    public static void CalculateWinningPlaces()
    {
        // Horizontal rows
        for (byte row = 0; row < 6; row++)
        {
            byte rowCol1 = (byte)(row * 7);
            byte rowColEnd = (byte)((row + 1) * 7 - 1);
            byte checkCol = rowCol1;

            while (checkCol <= rowColEnd - 3)
            {
                WinningPlaces.Add(new int[]
                {
                    checkCol,
                    (byte)(checkCol + 1),
                    (byte)(checkCol + 2),
                    (byte)(checkCol + 3)
                });

                checkCol++;
            }
        }

        // Vertical columns
        for (byte col = 0; col < 7; col++)
        {
            byte checkRow = col;

            while (checkRow <= 14 + col)
            {
                WinningPlaces.Add(new int[]
                {
                    checkRow,
                    (byte)(checkRow + 7),
                    (byte)(checkRow + 14),
                    (byte)(checkRow + 21)
                });

                checkRow += 7;
            }
        }

        // Forward slash diagonal "/"
        for (byte col = 0; col < 4; col++)
        {
            byte colRow1 = (byte)(21 + col);
            byte colRowEnd = (byte)(35 + col);
            byte checkPos = colRow1;

            while (checkPos <= colRowEnd)
            {
                WinningPlaces.Add(new int[]
                {
                    checkPos,
                    (byte)(checkPos - 6),
                    (byte)(checkPos - 12),
                    (byte)(checkPos - 18)
                });

                checkPos += 7;
            }
        }

        // Backslash diagonal "\"
        for (byte col = 0; col < 4; col++)
        {
            byte colRow1 = col;
            byte colRowEnd = (byte)(14 + col);
            byte checkPos = colRow1;

            while (checkPos <= colRowEnd)
            {
                WinningPlaces.Add(new int[]
                {
                    checkPos,
                    (byte)(checkPos + 8),
                    (byte)(checkPos + 16),
                    (byte)(checkPos + 24)
                });

                checkPos += 7;
            }
        }
    }

    // Check whether a player has won or the game is a tie
    public WinState CheckForWin()
    {
        if (CurrentTurn < 4)
        {
            return WinState.No_Winner;
        }

        foreach (var scenario in WinningPlaces)
        {
            if (TheBoard[scenario[0]] == 0)
            {
                continue;
            }

            if (TheBoard[scenario[0]] ==
                TheBoard[scenario[1]] &&
                TheBoard[scenario[1]] ==
                TheBoard[scenario[2]] &&
                TheBoard[scenario[2]] ==
                TheBoard[scenario[3]])
            {
                return (WinState)TheBoard[scenario[0]];
            }
        }

        if (CurrentTurn == 42)
        {
            return WinState.Tie;
        }

        return WinState.No_Winner;
    }

    // Record completed game results
    public void RecordResult(WinState result)
    {
        string gameResult;

        switch (result)
        {
            case WinState.Player1_Wins:
                Player1Wins++;
                gameResult = "Player 1 Wins";
                break;

            case WinState.Player2_Wins:
                Player2Wins++;
                gameResult = "Player 2 Wins";
                break;

            case WinState.Tie:
                Draws++;
                gameResult = "Draw";
                break;

            default:
                return;
        }

        _gameHistory.Add(new GameRecord
        {
            GameNumber = _gameHistory.Count + 1,
            Result = gameResult,
            CompletedAt = DateTime.Now
        });
    }

    // Play a piece in the selected column
    public byte PlayPiece(int column)
    {
        if (CheckForWin() != WinState.No_Winner)
        {
            throw new ArgumentException("Game is over");
        }

        if (column < 0 || column >= 7)
        {
            throw new ArgumentException("Invalid column");
        }

        if (TheBoard[column] != 0)
        {
            throw new ArgumentException("Column is full");
        }

        var landingSpot = column;

        for (var i = column; i < 42; i += 7)
        {
            if (i + 7 >= 42 || TheBoard[i + 7] != 0)
            {
                break;
            }

            landingSpot = i + 7;
        }

        TheBoard[landingSpot] = PlayerTurn;

        return ConvertLandingSpotToRow(landingSpot);
    }

    // Connect Four board: 7 columns × 6 rows
    public List<int> TheBoard { get; private set; } =
        new List<int>(new int[42]);

    // Reset the current board
    public void ResetBoard()
    {
        TheBoard = new List<int>(new int[42]);
    }

    // Convert a board position into its row number
    private byte ConvertLandingSpotToRow(int landingSpot)
    {
        return (byte)(Math.Floor(landingSpot / (decimal)7) + 1);
    }
}
