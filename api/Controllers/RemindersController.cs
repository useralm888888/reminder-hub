using Api.Authentication;

using Api.Dtos;

using Api.Exceptions;

using Api.Services;

using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;



namespace Api.Controllers;



[ApiController]

[Route("reminders")]

public class RemindersController(IReminderService reminderService) : ControllerBase

{

    [HttpPost]

    [Authorize(AuthenticationSchemes = ApiKeyAuthenticationDefaults.AuthenticationScheme)]

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



    [HttpGet]

    [ProducesResponseType(typeof(ReminderListResponse), StatusCodes.Status200OK)]

    public async Task<ActionResult<ReminderListResponse>> GetAll(

        [FromQuery] int page = 1,

        [FromQuery] int pageSize = ReminderService.DefaultPageSize,

        CancellationToken cancellationToken = default)

    {

        var response = await reminderService.GetPagedAsync(page, pageSize, cancellationToken);

        return Ok(response);

    }



    [HttpPut("{id:guid}")]

    [Authorize(AuthenticationSchemes = ApiKeyAuthenticationDefaults.AuthenticationScheme)]

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



    [HttpDelete("{id:guid}")]

    [Authorize(AuthenticationSchemes = ApiKeyAuthenticationDefaults.AuthenticationScheme)]

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


