using System;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Application.Events.Queries;
using Application.Events.Commands;
using MediatR;

namespace API.Controllers;

public class EventsController : BaseApiController
{

    /// <summary>
    /// Get all events
    /// </summary>
    /// <returns>List of events</returns>
    [HttpGet]
    public async Task<ActionResult<List<Event>>> GetEvents()
    {
        return await mediator.Send(new GetEventList.Query());
    }

    /// <summary>
    /// Get a specific event by EventId
    /// </summary>
    /// <param name="eventId">The EventId (Guid) of the event</param>
    /// <returns>The event</returns>
    [HttpGet("{eventId}")]
    public async Task<ActionResult<Event>> GetEvent(Guid eventId)
    {
        return await mediator.Send(new GetEventDetails.Query { EventId = eventId });
    }

    /// <summary>
    /// Create a new event
    /// </summary>
    /// <param name="eventEntity">The event to create</param>
    /// <returns>The created event</returns>
    [HttpPost]
    public async Task<ActionResult<Event>> CreateEvent(Event eventEntity)
    {        
        return await mediator.Send(new CreateEvent.Command { Event = eventEntity });
    }

    /// <summary>
    /// Update an existing event
    /// </summary>
    /// <param name="eventId">The EventId (Guid) of the event to update</param>
    /// <param name="eventEntity">The updated event data</param>
    /// <returns>No content if successful</returns>
    [HttpPut("{eventId}")]
    public async Task<IActionResult> UpdateEvent(Guid eventId, Event eventEntity)
    {
        if (eventId != eventEntity.EventId) return BadRequest();

        try
        {
            await mediator.Send(new EditEvent.Command(eventEntity));
            logger.LogInformation("Event updated successfully");
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            logger.LogWarning("Event not found");
            return NotFound();
        }
    }

    /// <summary>
    /// Delete an event by EventId
    /// </summary>
    /// <param name="eventId">The EventId (Guid) of the event to delete</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{eventId}")]
    public async Task<IActionResult> DeleteEvent(Guid eventId)
    {
        try
        {
            await mediator.Send(new DeleteEvent.Command { EventId = eventId });
            logger.LogInformation("Event deleted successfully");
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            logger.LogWarning("Event not found");
            return NotFound();
        }
    }

    /// <summary>
    /// Export all events to Excel format
    /// </summary>
    /// <returns>Excel file download</returns>
    [HttpGet("export")]
    public async Task<IActionResult> ExportEvents()
    {
        var excelBytes = await mediator.Send(new GetEventListExcel.Query());
        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "events.xlsx");
    }

    /// <summary>
    /// Export all events to CSV format
    /// </summary>
    /// <returns>CSV file download</returns>
    [HttpGet("export/csv")]
    public async Task<IActionResult> ExportEventsCSV()
    {
        var csvContent = await mediator.Send(new GetEventListCSV.Query());
        var bytes = System.Text.Encoding.UTF8.GetBytes(csvContent);
        return File(bytes, "text/csv", "events.csv");
    }
    
}

