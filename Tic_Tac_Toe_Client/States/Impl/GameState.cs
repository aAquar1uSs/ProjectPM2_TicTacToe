using System.Net;
using Microsoft.Extensions.Logging;
using TicTacToe.Client.DTO;
using TicTacToe.Client.Models;
using TicTacToe.Client.Services;
using TicTacToe.Client.Tools;

namespace TicTacToe.Client.States.Impl
{
    public class GameState : IGameState
    {
        private readonly IGameService _gameService;

        private readonly ILogger<GameState> _logger;

        private readonly Board _board;

        private bool _isFirst;

        private bool _isEndOfGame;

        private bool _isActivePlayer;

        public GameState(IGameService gameService,
            ILogger<GameState> logger)
        {
            _gameService = gameService;
            _logger = logger;
            _board = new Board();
        }

        public async Task InvokeMenuAsync()
        {
            LogInformationAboutClass(nameof(InvokeMenuAsync), "Execute method");
            await CheckRoundStateAsync();

            while (!_isEndOfGame)
            {
                Console.Clear();
                _board.Draw();

                if (_isActivePlayer == _isFirst)
                {
                    LogInformationAboutClass(nameof(InvokeMenuAsync), "Player waiting opponent's move");
                    ConsoleHelper.WriteInConsole(new[] { "Please, Wait till the other player moves" },
                        ConsoleColor.Green, "");
                    await WaitMoveOpponentAsync();
                }
                else
                {
                    var color = _isFirst ? ConsoleColor.Green : ConsoleColor.Red;
                    ConsoleHelper.WriteInConsole($"Your color is {color}\n", color);
                    ConsoleHelper.WriteInConsole(new[]
                    {
                        "You have limit time to move",
                        "1 -- Do move",
                        "2 -- Surrender",
                    },
                        ConsoleColor.Cyan);
                    try
                    {
                        ConsoleHelper.ReadIntFromConsole(out var choose);
                        switch (choose)
                        {
                            case 1:
                                var validMove = await MakeMoveAsync();
                                if (!validMove)
                                    continue;
                                break;

                            case 2:
                                await SurrenderAsync();
                                break;

                            default:
                                continue;
                        }
                    }
                    catch (FormatException ex)
                    {
                        _logger.LogError("Exception invalid format::{Message}", ex.Message);
                        ConsoleHelper.WriteInConsole(new[] { "It's not a number!" },
                            ConsoleColor.Red);
                        _ = Console.ReadLine();
                    }
                    catch (HttpRequestException ex)
                    {
                        _logger.LogError("The connection to the server is gone: {Message}", ex.Message);
                        ConsoleHelper.WriteInConsole(new[] { "Failed to connect with server!" },
                            ConsoleColor.Red);
                        _ = Console.ReadLine();
                    }
                    catch (OverflowException ex)
                    {
                        _logger.LogError("Number is very large: {Message}", ex.Message);
                        ConsoleHelper.WriteInConsole(new[] { "Number is very large!" },
                            ConsoleColor.Red);
                        _ = Console.ReadLine();
                    }
                }
            }

            ResultMenu();
        }

        public void LogInformationAboutClass(string methodName, string message)
        {
            _logger.LogInformation("{ClassName}::{MethodName}::{Message}",
                nameof(GameState), methodName, message);
        }

        private void ResultMenu()
        {
            LogInformationAboutClass(nameof(ResultMenu), "Invoke result menu");

            Console.Clear();
            _board.Draw();
            if (_isFirst == _isActivePlayer)
                DrawWin();
            else
                DrawLose();

            _ = Console.ReadLine();
        }

        private async Task CheckRoundStateAsync()
        {
            var response = await _gameService.CheckRoundStateAsync();
            LogInformationAboutClass(nameof(CheckRoundStateAsync), $"Response: {response.StatusCode}");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var roundState = await response.Content.ReadAsAsync<RoundStateDto>();
                _board.SetBoard(roundState.Board);
                _isEndOfGame = roundState.IsFinished;
                _isFirst = roundState.IsFirstPlayer;
                _isActivePlayer = roundState.IsActiveFirstPlayer;
            }
        }

        public async Task<bool> MakeMoveAsync()
        {
            LogInformationAboutClass(nameof(MakeMoveAsync), "Player make a move");

            while (true)
            {
                var move = GetMoveFromPlayer();
                var response = await _gameService.MakeMoveAsync(move);
                var errorMsg = await response.Content.ReadAsStringAsync();

                LogInformationAboutClass(nameof(MakeMoveAsync), $"Response: {response.StatusCode}");

                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        await CheckRoundStateAsync();
                        return true;

                    case HttpStatusCode.BadRequest:
                        ConsoleHelper.WriteInConsole(new[] { errorMsg }, ConsoleColor.DarkRed);
                        _ = Console.ReadLine();
                        return false;

                    case HttpStatusCode.NotFound:
                        ConsoleHelper.WriteInConsole(new[] { errorMsg }, ConsoleColor.Red);
                        _isEndOfGame = true;
                        _ = Console.ReadLine();
                        return false;

                    case HttpStatusCode.Conflict:
                        _isEndOfGame = true;
                        ConsoleHelper.WriteInConsole(new[] { errorMsg + "\n" }, ConsoleColor.DarkRed);
                        _ = Console.ReadLine();
                        return false;

                    default:
                        break;
                }
            }
        }

        public async Task WaitMoveOpponentAsync()
        {
            LogInformationAboutClass(nameof(WaitMoveOpponentAsync), "Waiting opponent's move");
            while (true)
            {
                var responseMessage = await _gameService.CheckMoveAsync();

                if (responseMessage.StatusCode == HttpStatusCode.OK)
                {
                    LogInformationAboutClass(nameof(WaitMoveOpponentAsync),
                        $"Response: {responseMessage.StatusCode}");

                    var roundState = await responseMessage.Content.ReadAsAsync<RoundStateDto>();
                    _board.SetBoard(roundState.Board);
                    _isFirst = roundState.IsFirstPlayer;
                    _isEndOfGame = roundState.IsFinished;
                    _isActivePlayer = roundState.IsActiveFirstPlayer;
                    break;
                }

                if (responseMessage.StatusCode == HttpStatusCode.Conflict)
                {
                    LogInformationAboutClass(nameof(WaitMoveOpponentAsync),
                        $"Response: {responseMessage.StatusCode}");

                    _isEndOfGame = true;
                    var errorMsg = await responseMessage.Content.ReadAsStringAsync();
                    ConsoleHelper.WriteInConsole(errorMsg + "\n", ConsoleColor.DarkRed);
                    _ = Console.ReadLine();
                    break;
                }
            }
        }

        private MoveDto GetMoveFromPlayer()
        {
            LogInformationAboutClass(nameof(GetMoveFromPlayer), "Get player's move");
            while (true)
            {
                Console.Clear();
                _board.Draw();

                int index;
                int number;
                try
                {
                    ConsoleHelper.WriteInConsole(new[] { "Input number of cell[1;9]:" }, ConsoleColor.Green, "");
                    ConsoleHelper.ReadIntFromConsole(out index);
                    ConsoleHelper.WriteInConsole(new[] { "Input number[1;9]:" }, ConsoleColor.Green, "");
                    ConsoleHelper.ReadIntFromConsole(out number);
                    index -= 1;
                }
                catch (FormatException ex)
                {
                    _logger.LogError("Exception invalid format::{Message}", ex.Message);
                    ConsoleHelper.WriteInConsole(new[] { "It's not a number!" }, ConsoleColor.DarkRed);
                    _ = Console.ReadLine();
                    continue;
                }

                return new MoveDto(index, number);
            }
        }

        private static void DrawWin()
        {
            ConsoleHelper.WriteInConsole(new[]
            {
                "╔╗╔╗╔══╗╔╗╔╗───╔╗╔╗╔╗╔══╗╔╗─╔╗",
                "║║║║║╔╗║║║║║───║║║║║║╚╗╔╝║╚═╝║",
                "║╚╝║║║║║║║║║───║║║║║║─║║─║╔╗─║",
                "╚═╗║║║║║║║║║───║║║║║║─║║─║║╚╗║",
                "─╔╝║║╚╝║║╚╝║───║╚╝╚╝║╔╝╚╗║║─║║",
                "─╚═╝╚══╝╚══╝───╚═╝╚═╝╚══╝╚╝─╚╝"
            }, ConsoleColor.Green);
        }

        private static void DrawLose()
        {
            ConsoleHelper.WriteInConsole(new[]
            {
                "╔╗╔╗╔══╗╔╗╔╗───╔╗──╔══╗╔══╗╔═══╗",
                "║║║║║╔╗║║║║║───║║──║╔╗║║╔═╝║╔══╝",
                "║╚╝║║║║║║║║║───║║──║║║║║╚═╗║╚══╗",
                "╚═╗║║║║║║║║║───║║──║║║║╚═╗║║╔══╝",
                "─╔╝║║╚╝║║╚╝║───║╚═╗║╚╝║╔═╝║║╚══╗",
                "─╚═╝╚══╝╚══╝───╚══╝╚══╝╚══╝╚═══╝"
            }, ConsoleColor.Red);
        }

        public async Task SurrenderAsync()
        {
            LogInformationAboutClass(nameof(SurrenderAsync), "Player surrendered...");

            var responseSurrender = await _gameService.SurrenderAsync();
            if (responseSurrender.StatusCode == HttpStatusCode.OK)
            {
                await CheckRoundStateAsync();
            }
        }
    }
}
