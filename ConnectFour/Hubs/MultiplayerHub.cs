
using Microsoft.AspNetCore.SignalR;

namespace ConnectFour.Hubs;

public sealed class MultiplayerGameSnapshot
{
    public string RoomCode { get; set; } = string.Empty;

    // 0 = empty, 1 = Player 1, 2 = Player 2.
    // The board is stored as six rows of seven columns.
    public int[] Board { get; set; } = new int[42];

    public int CurrentPlayer { get; set; } = 1;

    // 0 = no winner, 1 = Player 1, 2 = Player 2.
    public int Winner { get; set; }

    public bool IsDraw { get; set; }

    public bool Player1Connected { get; set; }

    public bool Player2Connected { get; set; }
}

public sealed class MultiplayerMoveResult
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;
}

public class MultiplayerHub : Hub
{
    private static readonly Dictionary<string, GameRoom> Rooms = new();

    private static readonly object RoomLock = new();

    private sealed class GameRoom
    {
        public string RoomCode { get; set; } = string.Empty;

        public string Player1ConnectionId { get; set; } = string.Empty;

        public string Player2ConnectionId { get; set; } = string.Empty;

        public int[] Board { get; set; } = new int[42];

        public int CurrentPlayer { get; set; } = 1;

        public int Winner { get; set; }

        public bool IsDraw { get; set; }

        public bool IsFull =>
            !string.IsNullOrEmpty(Player1ConnectionId) &&
            !string.IsNullOrEmpty(Player2ConnectionId);
    }

    public async Task<string> CreateRoom()
    {
        string roomCode;

        lock (RoomLock)
        {
            do
            {
                roomCode = Guid.NewGuid()
                    .ToString("N")
                    .Substring(0, 6)
                    .ToUpperInvariant();
            }
            while (Rooms.ContainsKey(roomCode));

            Rooms[roomCode] = new GameRoom
            {
                RoomCode = roomCode,
                Player1ConnectionId = Context.ConnectionId
            };
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);

        return roomCode;
    }

    public async Task<bool> JoinRoom(string roomCode)
    {
        roomCode = roomCode.Trim().ToUpperInvariant();

        lock (RoomLock)
        {
            if (!Rooms.TryGetValue(roomCode, out var room))
            {
                return false;
            }

            if (room.IsFull ||
                room.Player1ConnectionId == Context.ConnectionId)
            {
                return false;
            }

            room.Player2ConnectionId = Context.ConnectionId;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);

        MultiplayerGameSnapshot snapshot;

        lock (RoomLock)
        {
            if (!Rooms.TryGetValue(roomCode, out var room))
            {
                return false;
            }

            snapshot = CreateSnapshot(room);
        }

        await Clients.Group(roomCode).SendAsync(
            "PlayerJoined",
            snapshot);

        return true;
    }

    public MultiplayerGameSnapshot? GetGameState(string roomCode)
    {
        roomCode = roomCode.Trim().ToUpperInvariant();

        lock (RoomLock)
        {
            if (!Rooms.TryGetValue(roomCode, out var room))
            {
                return null;
            }

            if (room.Player1ConnectionId != Context.ConnectionId &&
                room.Player2ConnectionId != Context.ConnectionId)
            {
                return null;
            }

            return CreateSnapshot(room);
        }
    }

    public async Task<MultiplayerMoveResult> PlayMove(
        string roomCode,
        int column)
    {
        roomCode = roomCode.Trim().ToUpperInvariant();

        MultiplayerGameSnapshot? snapshot = null;

        lock (RoomLock)
        {
            if (!Rooms.TryGetValue(roomCode, out var room))
            {
                return new MultiplayerMoveResult
                {
                    Success = false,
                    Message = "Game room not found."
                };
            }

            int player;

            if (room.Player1ConnectionId == Context.ConnectionId)
            {
                player = 1;
            }
            else if (room.Player2ConnectionId == Context.ConnectionId)
            {
                player = 2;
            }
            else
            {
                return new MultiplayerMoveResult
                {
                    Success = false,
                    Message = "You are not a player in this room."
                };
            }

            if (!room.IsFull)
            {
                return new MultiplayerMoveResult
                {
                    Success = false,
                    Message = "Waiting for the second player to join."
                };
            }

            if (room.Winner != 0 || room.IsDraw)
            {
                return new MultiplayerMoveResult
                {
                    Success = false,
                    Message = "This game has already finished."
                };
            }

            if (column < 0 || column > 6)
            {
                return new MultiplayerMoveResult
                {
                    Success = false,
                    Message = "Invalid column selected."
                };
            }

            if (player != room.CurrentPlayer)
            {
                return new MultiplayerMoveResult
                {
                    Success = false,
                    Message = "It is not your turn."
                };
            }

            // Find the lowest available row in the selected column.
            int landingRow = -1;

            for (int row = 5; row >= 0; row--)
            {
                int index = row * 7 + column;

                if (room.Board[index] == 0)
                {
                    landingRow = row;
                    break;
                }
            }

            if (landingRow == -1)
            {
                return new MultiplayerMoveResult
                {
                    Success = false,
                    Message = "That column is full. Choose another column."
                };
            }

            // Place the piece on the server-controlled board.
            room.Board[landingRow * 7 + column] = player;

            if (CheckForWin(room.Board, player))
            {
                room.Winner = player;
            }
            else if (room.Board.All(piece => piece != 0))
            {
                room.IsDraw = true;
            }
            else
            {
                room.CurrentPlayer = player == 1 ? 2 : 1;
            }

            snapshot = CreateSnapshot(room);
        }

        // Broadcast the accepted move to both players.
        await Clients.Group(roomCode).SendAsync(
            "MovePlayed",
            snapshot);

        return new MultiplayerMoveResult
        {
            Success = true,
            Message = "Move accepted."
        };
    }

    private static MultiplayerGameSnapshot CreateSnapshot(GameRoom room)
    {
        return new MultiplayerGameSnapshot
        {
            RoomCode = room.RoomCode,
            Board = room.Board.ToArray(),
            CurrentPlayer = room.CurrentPlayer,
            Winner = room.Winner,
            IsDraw = room.IsDraw,
            Player1Connected = !string.IsNullOrEmpty(
                room.Player1ConnectionId),
            Player2Connected = !string.IsNullOrEmpty(
                room.Player2ConnectionId)
        };
    }

    private static bool CheckForWin(int[] board, int player)
    {
        int[,] directions =
        {
            { 0, 1 },  // Horizontal
            { 1, 0 },  // Vertical
            { 1, 1 },  // Diagonal down-right
            { 1, -1 }  // Diagonal down-left
        };

        for (int row = 0; row < 6; row++)
        {
            for (int col = 0; col < 7; col++)
            {
                if (board[row * 7 + col] != player)
                {
                    continue;
                }

                for (int direction = 0; direction < 4; direction++)
                {
                    int rowDirection = directions[direction, 0];
                    int colDirection = directions[direction, 1];

                    int count = 1;

                    for (int step = 1; step < 4; step++)
                    {
                        int nextRow = row + rowDirection * step;
                        int nextCol = col + colDirection * step;

                        if (nextRow < 0 || nextRow >= 6 ||
                            nextCol < 0 || nextCol >= 7)
                        {
                            break;
                        }

                        if (board[nextRow * 7 + nextCol] != player)
                        {
                            break;
                        }

                        count++;
                    }

                    if (count >= 4)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        string? roomToNotify = null;

        lock (RoomLock)
        {
            foreach (var room in Rooms.Values.ToList())
            {
                if (room.Player1ConnectionId == Context.ConnectionId ||
                    room.Player2ConnectionId == Context.ConnectionId)
                {
                    roomToNotify = room.RoomCode;

                    Rooms.Remove(room.RoomCode);

                    break;
                }
            }
        }

        if (roomToNotify != null)
        {
            await Clients.Group(roomToNotify).SendAsync(
                "PlayerDisconnected");
        }

        await base.OnDisconnectedAsync(exception);
    }
}
