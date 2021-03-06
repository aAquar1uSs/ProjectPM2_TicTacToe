using System.Net;
using Microsoft.Extensions.Logging;
using TicTacToe.Client.Services;
using TicTacToe.Client.Tools;

namespace TicTacToe.Client.States.Impl
{
    public class MainMenuState : IMainMenuState
    {
        private readonly IRoomMenuState _roomMenuState;

        private readonly ISettingsState _settingsState;

        private readonly IUserService _userService;

        private readonly IPrivateStatisticState _privateUserStatistic;

        private readonly ILogger<MainMenuState> _logger;

        public MainMenuState(IRoomMenuState roomMenuState,
            ISettingsState settingsState,
            IUserService userService,
            IPrivateStatisticState privateUserStatistic,
            ILogger<MainMenuState> logger)
        {
            _roomMenuState = roomMenuState;
            _settingsState = settingsState;
            _userService = userService;
            _privateUserStatistic = privateUserStatistic;
            _logger = logger;
        }

        public async Task InvokeMenuAsync()
        {
            LogInformationAboutClass(nameof(InvokeMenuAsync), "Execute method");

            while (true)
            {
                Console.Clear();
                ConsoleHelper.WriteInConsole(new[]
                {
                    "Main Menu",
                    "Please choose action:",
                    "1 -- Start room",
                    "2 -- Private statistic",
                    "3 -- Time settings",
                    "0 -- Logout"
                }, ConsoleColor.Cyan);

                try
                {
                    ConsoleHelper.ReadIntFromConsole(out var choose);
                    switch (choose)
                    {
                        case 1:
                            await ExecuteRoomMenuAsync();
                            break;

                        case 2:
                            await ExecutePrivateUserStatisticAsync();
                            break;

                        case 3:
                            await ExecuteSettingsStateAsync();
                            break;

                        case 0:
                            await LogoutAsync();
                            return;

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

        public async Task ExecuteRoomMenuAsync()
        {
            LogInformationAboutClass(nameof(ExecuteRoomMenuAsync),
                $"Execute {nameof(RoomMenuState)}.");
            await _roomMenuState.InvokeMenuAsync();
        }

        public async Task ExecutePrivateUserStatisticAsync()
        {
            LogInformationAboutClass(nameof(ExecutePrivateUserStatisticAsync),
                $"Execute {nameof(PrivateStatisticState)}");
            await _privateUserStatistic.InvokeMenuAsync();
        }

        public async Task ExecuteSettingsStateAsync()
        {
            LogInformationAboutClass(nameof(ExecuteSettingsStateAsync),
                $"Execute {nameof(SettingsState)}.");
            await _settingsState.InvokeMenuAsync();
        }

        public async Task LogoutAsync()
        {
            var response = await _userService.LogoutAsync();

            LogInformationAboutClass(nameof(LogoutAsync), $"Response: {response.StatusCode}");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var message = await response.Content.ReadAsStringAsync();
                ConsoleHelper.WriteInConsole(
                    new[] { message }, ConsoleColor.Yellow);
                _ = Console.ReadLine();
            }
        }

        public void LogInformationAboutClass(string methodName, string message)
        {
            _logger.LogInformation("{ClassName}::{MethodName}::{Message}",
                nameof(MainMenuState), methodName, message);
        }
    }
}
