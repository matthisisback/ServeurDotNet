using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using BlazorSignalRApp.Hubs;

namespace BlazorSignalRApp.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[IgnoreAntiforgeryToken]
public class ChatApiController : ControllerBase
{
    private readonly IHubContext<ChatHub> _hubContext;

    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok("pong");
    }
    public ChatApiController(IHubContext<ChatHub> hubContext)
    {
        _hubContext = hubContext;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] ChatMessage message)
    {
        await _hubContext.Clients.All.SendAsync("ReceiveMessage", message.User, message.Message);
        return Ok();
    }
}

public record ChatMessage(string User, string Message);
