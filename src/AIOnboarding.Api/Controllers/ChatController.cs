using Microsoft.AspNetCore.Mvc;

using AIOnboarding.Api.Models;
using AIOnboarding.Api.Providers;

namespace AIOnboarding.Api.Controllers;

[ApiController, Route("api/[controller]")]
public sealed class ChatController : ControllerBase
{
    private readonly IChatProvider chatProvider;

    public ChatController(IChatProvider chatProvider)
    {
        this.chatProvider = chatProvider;
    }


    [HttpPost("ask")]
    public async Task<IActionResult> AskAsync([FromBody] ChatRequest request)
    {
        var response = await chatProvider.AskAsync(request.Prompt);
        return Ok(new ChatResponse(response));
    }
}