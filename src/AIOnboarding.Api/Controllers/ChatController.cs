using Microsoft.AspNetCore.Mvc;

using AIOnboarding.Api.Models;
using AIOnboarding.Api.Providers;

namespace AIOnboarding.Api.Controllers;

[ApiController, Route("api/llm")]
public sealed class ChatApiController : ControllerBase
{
    private readonly IChatProvider chatProvider;

    public ChatApiController(IChatProvider chatProvider)
    {
        this.chatProvider = chatProvider;
    }


    [HttpPost("ask")]
    public async Task<IActionResult> AskAsync([FromBody] ChatRequest request)
    {
        var response = await chatProvider.AskAsync(request.Prompt);
        return Ok(new ChatResponse(response));
    }


    [HttpPost("chat")]
    public async Task<IActionResult> ChatAsync([FromBody] ChatRequest request)
    {
        var response = await chatProvider.ChatAsync(request.Prompt);
        return Ok(new ChatResponse(response));
    }
}