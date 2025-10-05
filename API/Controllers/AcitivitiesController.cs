using System;
using Domain;
using Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Application.Activities.Queries;
using MediatR;

namespace API.Controllers;

public class ActivitiesController(ILogger<ActivitiesController> logger, AppDbContext context, IMediator mediator) : BaseApiController(logger)
{
    private readonly AppDbContext _context = context;
    private readonly IMediator _mediator = mediator;


    /// <summary>
    /// Get all activities
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<List<Activity>>> GetActivities()
    {
        return await _mediator.Send(new GetActivityList.Query());
    }

    /// <summary>
    /// Get a specific activity by ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<Activity>> GetActivity(string id)
    {
        return await _mediator.Send(new GetActivityDetails.Query { Id = id });
    }

    /// <summary>
    /// Create a new activity
    /// </summary>
    /// <param name="activity"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<Activity>> CreateActivity(Activity activity)
    {
        return await _mediator.Send(new CreateActivity.Query { Activity = activity });
    }

    /// <summary>
    /// Update an existing activity
    /// </summary>
    /// <param name="id"></param>
    /// <param name="activity"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateActivity(string id, Activity activity)
    {
        if (id != activity.Id) return BadRequest();

        _context.Entry(activity).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ActivityExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    /// <summary>
    /// Delete an activity by ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteActivity(string id)
    {
        var activity = await _context.Activities.FindAsync(id);
        if (activity == null) return NotFound();

        _context.Activities.Remove(activity);
        await _context.SaveChangesAsync();

        return NoContent();
    }
    
    /// <summary>
    /// Check if an activity exists by ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private bool ActivityExists(string id)
    {
        return _context.Activities.Any(e => e.Id == id);
    }
    
}
