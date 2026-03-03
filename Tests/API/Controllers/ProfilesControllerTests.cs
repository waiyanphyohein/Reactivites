using API.Controllers;
using Application.Profiles.Queries;
using FluentAssertions;
using MediatR;
using NSubstitute;
using Tests.API.TestHelpers;
using Xunit;

namespace Tests.API.Controllers;

public class ProfilesControllerTests
{
    private readonly IMediator _mediator;
    private readonly ProfilesController _controller;

    public ProfilesControllerTests()
    {
        _mediator = Substitute.For<IMediator>();

        var httpContext = ControllerTestHelper.CreateHttpContext(_mediator);
        _controller = new ProfilesController();
        ControllerTestHelper.SetupController(_controller, httpContext);
    }

    [Fact]
    public async Task GetUserProfile_ReturnsJeffProfile()
    {
        var username = "jeff";
        var profile = new UserProfileDto
        {
            Username = username,
            DisplayName = "Jeff",
            AvatarUrl = "/images/jeff-placeholder.svg",
            PastEvents = new List<ProfileActivityDto>(),
            FutureEvents = new List<ProfileActivityDto>()
        };

        _mediator.Send(Arg.Any<GetUserProfile.Query>())
            .Returns(profile);

        var result = await _controller.GetUserProfile(username);

        result.Value.Should().NotBeNull();
        result.Value!.Username.Should().Be("jeff");
        result.Value.DisplayName.Should().Be("Jeff");
    }

    [Fact]
    public async Task GetUserProfile_SendsQueryWithUsername()
    {
        var username = "jeff";
        var profile = new UserProfileDto
        {
            Username = username,
            DisplayName = "Jeff",
            AvatarUrl = "/images/jeff-placeholder.svg",
            PastEvents = new List<ProfileActivityDto>(),
            FutureEvents = new List<ProfileActivityDto>()
        };

        _mediator.Send(Arg.Any<GetUserProfile.Query>())
            .Returns(profile);

        await _controller.GetUserProfile(username);

        await _mediator.Received(1).Send(
            Arg.Is<GetUserProfile.Query>(query => query.Username == username));
    }
}
