using Application.Profiles.Queries;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class ProfilesController : BaseApiController
{
    [HttpGet("{username}")]
    public async Task<ActionResult<UserProfileDto>> GetUserProfile(string username)
    {
        return await mediator.Send(new GetUserProfile.Query { Username = username });
    }
}
