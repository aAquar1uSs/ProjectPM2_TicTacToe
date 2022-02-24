﻿using Microsoft.Extensions.Logging;
using TicTacToe.Client.Services;

namespace TicTacToe.Client.States.Impl;

public class LeaderMenuState : ILeaderMenuState
{
    private readonly IStatisticService _statisticService;
    
    private readonly ILogger<LeaderMenuState> _logger;
    
    public LeaderMenuState(IStatisticService statisticService, ILogger<LeaderMenuState> logger)
    {
        _statisticService = statisticService;
        _logger = logger;
    }
    
    public async Task InvokeAsync()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("LeaderBoard menu");
            Console.WriteLine("1 -- By wins");
            Console.WriteLine("2 -- By times");
            Console.WriteLine("0 -- Close");
            
            try
            {
                ConsoleHelper.ReadIntFromConsole(out var choose);
                switch (choose)
                {
                    case 1:
                        break;
                        
                    case 2:
                        break;
                    
                    case 0:
                        return;
                }
            }
            catch (FormatException ex)
            {
                _logger.LogError(ex.Message);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex.Message);
                ConsoleHelper.WriteInConsole(new[] { "Failed to connect with server!" },
                    ConsoleColor.Red);
            }
        }
    }

    public Task<string> GetMessageFromResponseAsync(HttpResponseMessage response)
    {
        throw new NotImplementedException();
    }
}
