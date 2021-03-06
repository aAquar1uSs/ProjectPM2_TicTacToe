using TicTacToe.Server.DTO;
using TicTacToe.Server.Enums;
using TicTacToe.Server.Models;
using TicTacToe.Server.Tools;
using TicTacToe.Server.Tools.Impl;

namespace TicTacToe.Server.Services.Impl
{
    public class StatisticService : IStatisticService
    {
        private const string RoomsPath = "roomInfo.json";

        private const string UsersPath = "usersStorage.json";

        private const int MinRoundsForLeader = 10;

        private readonly HashSet<string> _leaderPlayers;

        private List<Room> _roomStorage;

        private readonly JsonHelper<Room> _roomsJsonHelper;

        private readonly JsonHelper<UserAccountDto> _usersJsonHelper;

        private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

        public StatisticService()
        {
            _leaderPlayers = new HashSet<string>();
            _roomStorage = new List<Room>();
            _roomsJsonHelper = new JsonHelper<Room>(RoomsPath);
            _usersJsonHelper = new JsonHelper<UserAccountDto>(UsersPath);
        }

        public async Task<PrivateStatisticDto> GetPrivateStatisticAsync(string login,
            DateTime startDate, DateTime endDate)
        {
            await _semaphoreSlim.WaitAsync();
            try
            {
                await UpdateRoomStorageFromFileAsync();

                var (winCount, lostCount) = GetWinLostCount(login, startDate, endDate);

                var allMoves = GetAllMovesFromRoomStorage(login, startDate, endDate);
                var (allTime, allRoomsCount) = GetAllTimeAndCountsOfRooms(login, startDate, endDate);
                var allMovesCount = allMoves.Count;
                var allPosition = allMoves.Select(x => x.IndexOfCell + 1).ToList();
                var allNumbers = allMoves.Select(x => x.Number).ToList();

                var (topPosition, countOfPositionUse) = GetTopPropertyWithCount(allPosition);
                var (topNumbers, countOfNumbersUse) = GetTopPropertyWithCount(allNumbers);

                return new PrivateStatisticDto(
                    winCount,
                    lostCount,
                    allRoomsCount,
                    allMovesCount,
                    topNumbers,
                    countOfNumbersUse,
                    topPosition,
                    countOfPositionUse,
                    allTime);
            }
            finally
            {
                _ = _semaphoreSlim.Release();
            }
        }

        public async Task<List<LeaderStatisticDto>> GetLeadersAsync(SortingType type)
        {
            await _semaphoreSlim.WaitAsync();
            try
            {
                await UpdateRoomStorageFromFileAsync();
                await UpdateLeaderPlayers();

                var resultLeaders = GetResultLeaders();

                switch (type)
                {
                    case SortingType.Winnings:
                        resultLeaders = resultLeaders.OrderByDescending(leader => leader.Winnings).ToList();
                        break;

                    case SortingType.Losses:
                        resultLeaders = resultLeaders.OrderByDescending(leader => leader.Losses).ToList();
                        break;

                    case SortingType.WinRate:
                        resultLeaders = resultLeaders.OrderByDescending(leader => leader.WinRate).ToList();
                        break;

                    case SortingType.Rooms:
                        resultLeaders = resultLeaders.OrderByDescending(leader => leader.RoomsNumber).ToList();
                        break;

                    case SortingType.Time:
                        resultLeaders = resultLeaders.OrderByDescending(leader => leader.Time).ToList();
                        break;

                    default:
                        break;
                }

                return resultLeaders;
            }
            finally
            {
                _ = _semaphoreSlim.Release();
            }
        }
        private async Task UpdateRoomStorageFromFileAsync()
        {
            _roomStorage = await _roomsJsonHelper.DeserializeAsync();
        }

        private async Task UpdateLeaderPlayers()
        {
            var usersStorage = await _usersJsonHelper.DeserializeAsync();

            usersStorage.ForEach(user =>
            {
                var (winCount, lostCount) = GetWinLostCount(user.Login,
                    DateTime.MinValue, DateTime.MaxValue);
                var roundsCount = winCount + lostCount;

                if (roundsCount > MinRoundsForLeader)
                    _ = _leaderPlayers.Add(user.Login);
            });
        }

        private List<MoveDto> GetAllMovesFromRoomStorage(string login, DateTime startDate, DateTime endDate)
        {
            var result = new List<MoveDto>();
            _roomStorage.ForEach(room =>
            {
                if (room.Times.CreationRoomDate >= startDate && room.Times.FinishRoomDate <= endDate)
                {
                    if (login.Equals(room.FirstPlayer.Login, StringComparison.Ordinal))
                    {
                        foreach (var moves in room.Rounds)
                        {
                            result.AddRange(moves.FirstPlayerMoves);
                        }
                    }
                    else if (login.Equals(room.SecondPlayer.Login, StringComparison.Ordinal))
                    {
                        foreach (var moves in room.Rounds)
                        {
                            result.AddRange(moves.SecondPlayerMoves);
                        }
                    }
                }
            });

            return result;
        }

        private List<LeaderStatisticDto> GetResultLeaders()
        {
            var resultLeaders = new List<LeaderStatisticDto>();

            foreach (var player in _leaderPlayers)
            {
                var (winCount, lostCount) = GetWinLostCount(player,
                    DateTime.MinValue, DateTime.MaxValue);
                var (time, roomsCount) = GetAllTimeAndCountsOfRooms(player,
                    DateTime.MinValue, DateTime.MaxValue);

                var leaderStatistic = new LeaderStatisticDto(
                    player, winCount, lostCount, roomsCount, time);
                resultLeaders.Add(leaderStatistic);
            }

            return resultLeaders;
        }

        private (int, int) GetWinLostCount(string login, DateTime startDate, DateTime endDate)
        {
            var winCount = 0;
            var lostCount = 0;
            _roomStorage.ForEach(room =>
            {
                if (room.Times.CreationRoomDate >= startDate && room.Times.FinishRoomDate <= endDate)
                {
                    if (login.Equals(room.FirstPlayer.Login, StringComparison.Ordinal))
                    {
                        winCount += room.FirstPlayer.Wins;
                        lostCount += room.SecondPlayer.Wins;
                    }
                    else if (login.Equals(room.SecondPlayer.Login, StringComparison.Ordinal))
                    {
                        winCount += room.SecondPlayer.Wins;
                        lostCount += room.FirstPlayer.Wins;
                    }
                }
            });

            return (winCount, lostCount);
        }

        private (List<int>, int) GetTopPropertyWithCount(List<int> prop)
        {
            var countOfPosition = Enumerable.Repeat(0, 9).ToList();
            prop.ForEach(x => countOfPosition[x - 1] += 1);
            var topCount = countOfPosition.Max();

            if (topCount == 0)
                return (new List<int>(), 0);

            var result = new List<int>();
            var index = 0;
            countOfPosition.ForEach(x =>
            {
                if (x == topCount)
                    result.Add(index + 1);
                index++;
            });
            return (result, topCount);
        }

        private (TimeSpan, int) GetAllTimeAndCountsOfRooms(string login, DateTime startDate, DateTime endDate)
        {
            var allTime = TimeSpan.Zero;
            var countOfRooms = 0;
            _roomStorage.ForEach(room =>
            {
                if (room.Times.CreationRoomDate >= startDate && room.Times.FinishRoomDate <= endDate)
                {
                    if (login.Equals(room.FirstPlayer.Login, StringComparison.Ordinal)
                        || login.Equals(room.SecondPlayer.Login, StringComparison.Ordinal))
                    {
                        allTime += room.Times.FinishRoomDate - room.Times.CreationRoomDate;
                        countOfRooms++;
                    }
                }
            });
            return (allTime, countOfRooms);
        }
    }
}
