using System;
using Domain;
using Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Application.Activities.Queries;
using MediatR;

namespace API.Controllers;

public class ActivitiesController : BaseApiController
{

    /// <summary>
    /// Get all activities
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<List<Activity>>> GetActivities()
    {
        return await mediator.Send(new GetActivityList.Query());
    }

    /// <summary>
    /// Get a specific activity by ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<Activity>> GetActivity(string id)
    {
        return await mediator.Send(new GetActivityDetails.Query { Id = id });
    }

    /// <summary>
    /// Create a new activity
    /// </summary>
    /// <param name="activity"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<Activity>> CreateActivity(Activity activity)
    {        
        return await mediator.Send(new CreateActivity.Command { Activity = activity });
    }

    /// <summary>
    /// Update an existing activity
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateActivity(string id, Activity activity)
    {
        if (id != activity.Id) return BadRequest();

        try
        {
            await mediator.Send(new EditActivity.Command(activity));
            logger.LogInformation("Activity updated successfully");
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            logger.LogWarning("Activity not found");
            return NotFound();
        }
    }

    /// <summary>
    /// Delete an activity by ID
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteActivity(string id)
    {
        try
        {
            await mediator.Send(new DeleteActivity.Command { Id = id });
            logger.LogInformation("Activity deleted successfully");
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            logger.LogWarning("Activity not found");
            return NotFound();
        }
    }

    /// <summary>
    /// Export all activities to Excel format
    /// </summary>
    /// <returns>Excel file download</returns>
    [HttpGet("export")]
    public async Task<IActionResult> ExportActivities()
    {
        var excelBytes = await mediator.Send(new GetActivityListExcel.Query());
        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "activities.xlsx");
    }
    
}
