using API.Controllers;
using Tests.API.TestHelpers;
using Application.Activities.Queries;
using Domain;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Tests.API.Controllers;

public class ActivitiesControllerTests
{
    private readonly IMediator _mediator;
    private readonly ILogger<BaseApiController> _logger;
    private readonly ActivitiesController _controller;

    public ActivitiesControllerTests()
    {
        _mediator = Substitute.For<IMediator>();
        _logger = Substitute.For<ILogger<BaseApiController>>();

        var httpContext = ControllerTestHelper.CreateHttpContext(_mediator, _logger);
        _controller = new ActivitiesController();
        ControllerTestHelper.SetupController(_controller, httpContext);
    }

    #region GetActivities Tests

    [Fact]
    public async Task GetActivities_ReturnsListOfActivities()
    {
        // Arrange
        var activities = ActivityTestData.CreateActivityList(3);
        _mediator.Send(Arg.Any<GetActivityList.Query>())
            .Returns(activities);

        // Act
        var result = await _controller.GetActivities();

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().BeEquivalentTo(activities);
        await _mediator.Received(1).Send(Arg.Any<GetActivityList.Query>());
    }

    [Fact]
    public async Task GetActivities_ReturnsEmptyList_WhenNoActivities()
    {
        // Arrange
        _mediator.Send(Arg.Any<GetActivityList.Query>())
            .Returns(new List<Activity>());

        // Act
        var result = await _controller.GetActivities();

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetActivities_SendsCorrectQuery()
    {
        // Arrange
        _mediator.Send(Arg.Any<GetActivityList.Query>())
            .Returns(new List<Activity>());

        // Act
        await _controller.GetActivities();

        // Assert
        await _mediator.Received(1).Send(Arg.Is<GetActivityList.Query>(q => q != null));
    }

    #endregion

    #region GetActivity Tests

    [Fact]
    public async Task GetActivity_WithValidId_ReturnsActivity()
    {
        // Arrange
        var activity = ActivityTestData.CreateValidActivity();
        _mediator.Send(Arg.Any<GetActivityDetails.Query>())
            .Returns(activity);

        // Act
        var result = await _controller.GetActivity(activity.Id);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().BeEquivalentTo(activity);
    }

    [Fact]
    public async Task GetActivity_SendsQueryWithCorrectId()
    {
        // Arrange
        var activityId = Guid.NewGuid().ToString();
        var activity = ActivityTestData.CreateValidActivity(activityId);
        _mediator.Send(Arg.Any<GetActivityDetails.Query>())
            .Returns(activity);

        // Act
        await _controller.GetActivity(activityId);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<GetActivityDetails.Query>(q => q.Id == activityId));
    }

    #endregion

    #region CreateActivity Tests

    [Fact]
    public async Task CreateActivity_WithValidActivity_ReturnsCreatedActivity()
    {
        // Arrange
        var activity = ActivityTestData.CreateValidActivity();
        _mediator.Send(Arg.Any<CreateActivity.Command>())
            .Returns(activity);

        // Act
        var result = await _controller.CreateActivity(activity);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().BeEquivalentTo(activity);
    }

    [Fact]
    public async Task CreateActivity_SendsCommandWithCorrectActivity()
    {
        // Arrange
        var activity = ActivityTestData.CreateValidActivity();
        _mediator.Send(Arg.Any<CreateActivity.Command>())
            .Returns(activity);

        // Act
        await _controller.CreateActivity(activity);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<CreateActivity.Command>(c => c.Activity == activity));
    }

    #endregion

    #region UpdateActivity Tests

    [Fact]
    public async Task UpdateActivity_WithMatchingIds_ReturnsNoContent()
    {
        // Arrange
        var activity = ActivityTestData.CreateValidActivity();
        _mediator.Send(Arg.Any<EditActivity.Command>())
            .Returns(activity);

        // Act
        var result = await _controller.UpdateActivity(activity.Id, activity);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task UpdateActivity_WithMismatchedIds_ReturnsBadRequest()
    {
        // Arrange
        var activity = ActivityTestData.CreateValidActivity();
        var differentId = Guid.NewGuid().ToString();

        // Act
        var result = await _controller.UpdateActivity(differentId, activity);

        // Assert
        result.Should().BeOfType<BadRequestResult>();
        await _mediator.DidNotReceive().Send(Arg.Any<EditActivity.Command>());
    }

    [Fact]
    public async Task UpdateActivity_WhenActivityNotFound_ReturnsNotFound()
    {
        // Arrange
        var activity = ActivityTestData.CreateValidActivity();
        _mediator.Send(Arg.Any<EditActivity.Command>())
            .Returns(Task.FromException<Activity>(new KeyNotFoundException()));

        // Act
        var result = await _controller.UpdateActivity(activity.Id, activity);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task UpdateActivity_WhenActivityNotFound_LogsWarning()
    {
        // Arrange
        var activity = ActivityTestData.CreateValidActivity();
        _mediator.Send(Arg.Any<EditActivity.Command>())
            .Returns(Task.FromException<Activity>(new KeyNotFoundException()));

        // Act
        await _controller.UpdateActivity(activity.Id, activity);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Activity not found")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task UpdateActivity_OnSuccess_LogsInformation()
    {
        // Arrange
        var activity = ActivityTestData.CreateValidActivity();
        _mediator.Send(Arg.Any<EditActivity.Command>())
            .Returns(activity);

        // Act
        await _controller.UpdateActivity(activity.Id, activity);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Activity updated successfully")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task UpdateActivity_SendsCommandWithCorrectActivity()
    {
        // Arrange
        var activity = ActivityTestData.CreateValidActivity();
        _mediator.Send(Arg.Any<EditActivity.Command>())
            .Returns(activity);

        // Act
        await _controller.UpdateActivity(activity.Id, activity);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<EditActivity.Command>(c => c.Activity == activity));
    }

    #endregion

    #region DeleteActivity Tests

    [Fact]
    public async Task DeleteActivity_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var activityId = Guid.NewGuid().ToString();
        _mediator.Send(Arg.Any<DeleteActivity.Command>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteActivity(activityId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteActivity_WhenActivityNotFound_ReturnsNotFound()
    {
        // Arrange
        var activityId = Guid.NewGuid().ToString();
        _mediator.Send(Arg.Any<DeleteActivity.Command>())
            .Returns(Task.FromException(new KeyNotFoundException()));

        // Act
        var result = await _controller.DeleteActivity(activityId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task DeleteActivity_WhenActivityNotFound_LogsWarning()
    {
        // Arrange
        var activityId = Guid.NewGuid().ToString();
        _mediator.Send(Arg.Any<DeleteActivity.Command>())
            .Returns(Task.FromException(new KeyNotFoundException()));

        // Act
        await _controller.DeleteActivity(activityId);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Activity not found")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task DeleteActivity_OnSuccess_LogsInformation()
    {
        // Arrange
        var activityId = Guid.NewGuid().ToString();
        _mediator.Send(Arg.Any<DeleteActivity.Command>())
            .Returns(Task.CompletedTask);

        // Act
        await _controller.DeleteActivity(activityId);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Activity deleted successfully")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task DeleteActivity_SendsCommandWithCorrectId()
    {
        // Arrange
        var activityId = Guid.NewGuid().ToString();
        _mediator.Send(Arg.Any<DeleteActivity.Command>())
            .Returns(Task.CompletedTask);

        // Act
        await _controller.DeleteActivity(activityId);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<DeleteActivity.Command>(c => c.Id == activityId));
    }

    #endregion

    #region ExportActivities Tests

    [Fact]
    public async Task ExportActivities_ReturnsFileResult()
    {
        // Arrange
        var excelBytes = new byte[] { 1, 2, 3, 4, 5 };
        _mediator.Send(Arg.Any<GetActivityListExcel.Query>())
            .Returns(excelBytes);

        // Act
        var result = await _controller.ExportActivities();

        // Assert
        result.Should().BeOfType<FileContentResult>();
        var fileResult = result as FileContentResult;
        fileResult!.FileContents.Should().BeEquivalentTo(excelBytes);
    }

    [Fact]
    public async Task ExportActivities_ReturnsCorrectContentType()
    {
        // Arrange
        var excelBytes = new byte[] { 1, 2, 3 };
        _mediator.Send(Arg.Any<GetActivityListExcel.Query>())
            .Returns(excelBytes);

        // Act
        var result = await _controller.ExportActivities();

        // Assert
        var fileResult = result as FileContentResult;
        fileResult!.ContentType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }

    [Fact]
    public async Task ExportActivities_ReturnsCorrectFileName()
    {
        // Arrange
        var excelBytes = new byte[] { 1, 2, 3 };
        _mediator.Send(Arg.Any<GetActivityListExcel.Query>())
            .Returns(excelBytes);

        // Act
        var result = await _controller.ExportActivities();

        // Assert
        var fileResult = result as FileContentResult;
        fileResult!.FileDownloadName.Should().Be("activities.xlsx");
    }

    [Fact]
    public async Task ExportActivities_SendsCorrectQuery()
    {
        // Arrange
        var excelBytes = new byte[] { 1, 2, 3 };
        _mediator.Send(Arg.Any<GetActivityListExcel.Query>())
            .Returns(excelBytes);

        // Act
        await _controller.ExportActivities();

        // Assert
        await _mediator.Received(1).Send(Arg.Any<GetActivityListExcel.Query>());
    }

    #endregion
}
