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
    public async Task<IActionResult> AskAsync([FromBody] ChatRequest request, CancellationToken cancellationToken)
    {
        Response.Headers.CacheControl = "no-cache";
        Response.ContentType = "text/plain; charset=utf-8";

        await foreach (var chunk in chatProvider.AskAsync(request.Prompt, cancellationToken))
        {
            await Response.WriteAsync(chunk, cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }

        await Response.Body.FlushAsync(cancellationToken);
        return new EmptyResult();
    }

    [HttpPost("chat")]
    public async Task<IActionResult> ChatAsync([FromBody] ChatRequest request, CancellationToken cancellationToken)
    {
        Response.Headers.CacheControl = "no-cache";
        Response.ContentType = "text/plain; charset=utf-8";

        await foreach (var chunk in chatProvider.ChatAsync(request.Prompt, cancellationToken))
        {
            await Response.WriteAsync(chunk, cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }

        await Response.Body.FlushAsync(cancellationToken);
        return new EmptyResult();
    }
}