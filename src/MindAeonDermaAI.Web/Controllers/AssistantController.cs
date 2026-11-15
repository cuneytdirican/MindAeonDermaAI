using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindAeonDermaAI.Core.Ai;
using MindAeonDermaAI.Infrastructure.Data;
using MindAeonDermaAI.Web.Services;

namespace MindAeonDermaAI.Web.Controllers;

/// <summary>Data-grounded AI assistant (text + voice). Answers questions and analyzes the platform's data.</summary>
[Authorize]
public class AssistantController : Controller
{
    private readonly IAssistantService _assistant;
    private readonly AppDbContext _db;

    public AssistantController(IAssistantService assistant, AppDbContext db)
    {
        _assistant = assistant;
        _db = db;
    }

    public async Task<IActionResult> Index(int? assessmentId, string? q, string? mode, CancellationToken ct)
    {
        ViewData["AiReady"] = await _assistant.AiReadyAsync(ct);
        ViewBag.FocusId = assessmentId;
        ViewBag.Prefill = q;
        ViewBag.Mode = mode;
        ViewBag.Focus = await _db.Assessments
            .Include(a => a.Subject)
            .OrderBy(a => a.Subject!.Code).ThenBy(a => a.TimePoint)
            .Take(80)
            .Select(a => new { a.Id, Label = a.Subject!.Code + (a.Subject.DisplayName != null ? " · " + a.Subject.DisplayName : "") + " [" + a.TimePoint + "]" })
            .ToListAsync(ct);
        return View();
    }

    public record ChatTurn(string Role, string Content);
    public record AskRequest(List<ChatTurn> Messages, int? AssessmentId, string? PageContext, string? Mode);

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Ask([FromBody] AskRequest body, CancellationToken ct)
    {
        if (body?.Messages is null || body.Messages.Count == 0)
            return BadRequest(new { error = "No message." });

        var history = body.Messages
            .Where(m => !string.IsNullOrWhiteSpace(m.Content))
            .Select(m => new AiMessage(m.Role == "assistant" ? "assistant" : "user", m.Content))
            .ToList();

        var reply = await _assistant.AskAsync(history, body.AssessmentId, body.PageContext, body.Mode, ct);
        return Ok(new { reply });
    }
}
