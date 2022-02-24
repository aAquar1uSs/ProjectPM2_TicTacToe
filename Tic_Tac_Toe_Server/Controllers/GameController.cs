﻿using System.Net;
using Microsoft.AspNetCore.Mvc;
using TicTacToe.Server.Models;
using TicTacToe.Server.Services;

namespace TicTacToe.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GameController : ControllerBase
{
    private readonly IRoomService _roomService;

    private readonly IAccountService _accountService;

    private readonly ILogger<GameController> _logger;

    public GameController(IRoomService roomService,
        IAccountService accountService,
        ILogger<GameController> logger)
    {
        _roomService = roomService;
        _accountService = accountService;
        _logger = logger;
    }

    [FromHeader(Name = "Login")]
    public string LoginUser { get; set; }

    [HttpPost("create_room")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> StartRoomAsync([FromBody] RoomSettings? settings)
    {
        if (!await FindUser())
        {
            _logger.LogWarning("Unauthorized users");
            return Unauthorized("Unauthorized users");
        }

        if (settings is null)
        {
            _logger.LogWarning("Settings is null");
            return BadRequest("Settings is null");
        }

        var response = await _roomService.CreateRoomAsync(LoginUser, settings);
        if (response is null)
            return BadRequest("Such a token does not exist.");
        return Ok(response);
    }
    
    [HttpGet("check_room/{id}")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> CheckRoomAsync(string id)
    {
        if (!await FindUser())
        {
            _logger.LogWarning("Unauthorized users");
            return Unauthorized("Unauthorized users");
        }
        
        var room = await _roomService.FindRoomByIdAsync(id);
        
        if (room is null)
            return BadRequest();
        
        if (room.IsCompleted)
            return Ok();
        
        return NotFound();
    }

    [NonAction]
    public async Task<bool> FindUser()
    {
        return await _accountService.FindAccountByLogin(LoginUser);
    }
    
}
