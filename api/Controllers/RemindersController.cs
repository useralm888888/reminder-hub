using Api.Authentication;
using Api.Dtos;
using Api.Exceptions;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

// All reminder endpoints need a Bearer token (same one you get from POST /auth/login).
[ApiController]
[Route("reminders")]
[Authorize(AuthenticationSchemes = ApiKeyAuthenticationDefaults.AuthenticationScheme)]
public class RemindersController(IReminderService reminderService) : ControllerBase
{
    // Schedule a new reminder. sendAt must be UTC and in the future.
    // Email is optional — if Brevo is configured, we'll try to send it when the time comes.
    [HttpPost]
    [ProducesResponseType(typeof(CreateReminderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CreateReminderResponse>> Create(
        [FromBody] CreateReminderRequest request,
        CancellationToken cancellationToken)
    {
        var response = await reminderService.CreateAsync(request, cancellationToken);
        return Created($"/reminders/{response.Id}", response);
    }

    // Paginated list of everything we have — both scheduled and already sent.
    [HttpGet]
    [ProducesResponseType(typeof(ReminderListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ReminderListResponse>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = ReminderService.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var response = await reminderService.GetPagedAsync(page, pageSize, cancellationToken);
        return Ok(response);
    }

    // Change a reminder that's still in Scheduled status. Sent ones can't be touched.
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ReminderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReminderDto>> Update(
        Guid id,
        [FromBody] UpdateReminderRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var reminder = await reminderService.UpdateAsync(id, request, cancellationToken);
            return Ok(reminder);
        }
        catch (ReminderNotFoundException)
        {
            return NotFound();
        }
        catch (ReminderNotEditableException ex)
        {
            return Conflict(new ProblemDetails { Title = ex.Message });
        }
    }

    // Remove a scheduled reminder. Same rule — only while it's still Scheduled.
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await reminderService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (ReminderNotFoundException)
        {
            return NotFound();
        }
        catch (ReminderNotEditableException ex)
        {
            return Conflict(new ProblemDetails { Title = ex.Message });
        }
    }
}
