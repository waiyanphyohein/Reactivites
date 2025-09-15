using System;
using Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Domain;

namespace API.Controllers;

public class ActivitiesController(ILogger<ActivitiesController> logger, AppDbContext context) : BaseApiController(logger){
    private readonly AppDbContext _context = context;

    [HttpGet]
    public async Task<ActionResult<List<Activity>>> GetActivities(){
        return await _context.Activities.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Activity>> GetActivity(string id){
        var activity = await _context.Activities.FindAsync(id);
        if (activity == null) return NotFound();
        return activity;
    }

    [HttpPost]
    public async Task<ActionResult<Activity>> CreateActivity(Activity activity){
        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();
        return activity;
    }
}
