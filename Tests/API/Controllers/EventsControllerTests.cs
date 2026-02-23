using API.Controllers;
using Tests.API.TestHelpers;
using Application.Events.Commands;
using Application.Events.Queries;
using Domain;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Tests.API.Controllers;

public class EventsControllerTests
{
    private readonly IMediator _mediator;
    private readonly ILogger<BaseApiController> _logger;
    private readonly EventsController _controller;

    public EventsControllerTests()
    {
        _mediator = Substitute.For<IMediator>();
        _logger = Substitute.For<ILogger<BaseApiController>>();

        var httpContext = ControllerTestHelper.CreateHttpContext(_mediator, _logger);
        _controller = new EventsController();
        ControllerTestHelper.SetupController(_controller, httpContext);
    }

    #region GetEvents Tests

    [Fact]
    public async Task GetEvents_ReturnsListOfEvents()
    {
        // Arrange
        var events = EventTestData.CreateEventList(3);
        _mediator.Send(Arg.Any<GetEventList.Query>())
            .Returns(events);

        // Act
        var result = await _controller.GetEvents();

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().BeEquivalentTo(events);
        await _mediator.Received(1).Send(Arg.Any<GetEventList.Query>());
    }

    [Fact]
    public async Task GetEvents_ReturnsEmptyList_WhenNoEvents()
    {
        // Arrange
        _mediator.Send(Arg.Any<GetEventList.Query>())
            .Returns(new List<Event>());

        // Act
        var result = await _controller.GetEvents();

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetEvents_SendsCorrectQuery()
    {
        // Arrange
        _mediator.Send(Arg.Any<GetEventList.Query>())
            .Returns(new List<Event>());

        // Act
        await _controller.GetEvents();

        // Assert
        await _mediator.Received(1).Send(Arg.Is<GetEventList.Query>(q => q != null));
    }

    #endregion

    #region GetEvent Tests

    [Fact]
    public async Task GetEvent_WithValidId_ReturnsEvent()
    {
        // Arrange
        var eventEntity = EventTestData.CreateValidEvent();
        _mediator.Send(Arg.Any<GetEventDetails.Query>())
            .Returns(eventEntity);

        // Act
        var result = await _controller.GetEvent(eventEntity.EventId);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().BeEquivalentTo(eventEntity);
    }

    [Fact]
    public async Task GetEvent_SendsQueryWithCorrectEventId()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var eventEntity = EventTestData.CreateValidEvent(eventId);
        _mediator.Send(Arg.Any<GetEventDetails.Query>())
            .Returns(eventEntity);

        // Act
        await _controller.GetEvent(eventId);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<GetEventDetails.Query>(q => q.EventId == eventId));
    }

    #endregion

    #region CreateEvent Tests

    [Fact]
    public async Task CreateEvent_WithValidEvent_ReturnsCreatedEvent()
    {
        // Arrange
        var eventEntity = EventTestData.CreateValidEvent();
        _mediator.Send(Arg.Any<CreateEvent.Command>())
            .Returns(eventEntity);

        // Act
        var result = await _controller.CreateEvent(eventEntity);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().BeEquivalentTo(eventEntity);
    }

    [Fact]
    public async Task CreateEvent_SendsCommandWithCorrectEvent()
    {
        // Arrange
        var eventEntity = EventTestData.CreateValidEvent();
        _mediator.Send(Arg.Any<CreateEvent.Command>())
            .Returns(eventEntity);

        // Act
        await _controller.CreateEvent(eventEntity);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<CreateEvent.Command>(c => c.Event == eventEntity));
    }

    #endregion

    #region UpdateEvent Tests

    [Fact]
    public async Task UpdateEvent_WithMatchingIds_ReturnsNoContent()
    {
        // Arrange
        var eventEntity = EventTestData.CreateValidEvent();
        _mediator.Send(Arg.Any<EditEvent.Command>())
            .Returns(eventEntity);

        // Act
        var result = await _controller.UpdateEvent(eventEntity.EventId, eventEntity);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task UpdateEvent_WithMismatchedIds_ReturnsBadRequest()
    {
        // Arrange
        var eventEntity = EventTestData.CreateValidEvent();
        var differentId = Guid.NewGuid();

        // Act
        var result = await _controller.UpdateEvent(differentId, eventEntity);

        // Assert
        result.Should().BeOfType<BadRequestResult>();
        await _mediator.DidNotReceive().Send(Arg.Any<EditEvent.Command>());
    }

    [Fact]
    public async Task UpdateEvent_WhenEventNotFound_ReturnsNotFound()
    {
        // Arrange
        var eventEntity = EventTestData.CreateValidEvent();
        _mediator.Send(Arg.Any<EditEvent.Command>())
            .Returns(Task.FromException<Event>(new KeyNotFoundException()));

        // Act
        var result = await _controller.UpdateEvent(eventEntity.EventId, eventEntity);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task UpdateEvent_WhenEventNotFound_LogsWarning()
    {
        // Arrange
        var eventEntity = EventTestData.CreateValidEvent();
        _mediator.Send(Arg.Any<EditEvent.Command>())
            .Returns(Task.FromException<Event>(new KeyNotFoundException()));

        // Act
        await _controller.UpdateEvent(eventEntity.EventId, eventEntity);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Event not found")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task UpdateEvent_OnSuccess_LogsInformation()
    {
        // Arrange
        var eventEntity = EventTestData.CreateValidEvent();
        _mediator.Send(Arg.Any<EditEvent.Command>())
            .Returns(eventEntity);

        // Act
        await _controller.UpdateEvent(eventEntity.EventId, eventEntity);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Event updated successfully")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    #endregion

    #region DeleteEvent Tests

    [Fact]
    public async Task DeleteEvent_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        _mediator.Send(Arg.Any<DeleteEvent.Command>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteEvent(eventId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteEvent_WhenEventNotFound_ReturnsNotFound()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        _mediator.Send(Arg.Any<DeleteEvent.Command>())
            .Returns(Task.FromException(new KeyNotFoundException()));

        // Act
        var result = await _controller.DeleteEvent(eventId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task DeleteEvent_WhenEventNotFound_LogsWarning()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        _mediator.Send(Arg.Any<DeleteEvent.Command>())
            .Returns(Task.FromException(new KeyNotFoundException()));

        // Act
        await _controller.DeleteEvent(eventId);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Event not found")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task DeleteEvent_OnSuccess_LogsInformation()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        _mediator.Send(Arg.Any<DeleteEvent.Command>())
            .Returns(Task.CompletedTask);

        // Act
        await _controller.DeleteEvent(eventId);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Event deleted successfully")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task DeleteEvent_SendsCommandWithCorrectId()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        _mediator.Send(Arg.Any<DeleteEvent.Command>())
            .Returns(Task.CompletedTask);

        // Act
        await _controller.DeleteEvent(eventId);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<DeleteEvent.Command>(c => c.EventId == eventId));
    }

    #endregion

    #region ExportEvents Tests

    [Fact]
    public async Task ExportEvents_ReturnsFileResult()
    {
        // Arrange
        var excelBytes = new byte[] { 1, 2, 3, 4, 5 };
        _mediator.Send(Arg.Any<GetEventListExcel.Query>())
            .Returns(excelBytes);

        // Act
        var result = await _controller.ExportEvents();

        // Assert
        result.Should().BeOfType<FileContentResult>();
        var fileResult = result as FileContentResult;
        fileResult!.FileContents.Should().BeEquivalentTo(excelBytes);
    }

    [Fact]
    public async Task ExportEvents_ReturnsCorrectContentType()
    {
        // Arrange
        var excelBytes = new byte[] { 1, 2, 3 };
        _mediator.Send(Arg.Any<GetEventListExcel.Query>())
            .Returns(excelBytes);

        // Act
        var result = await _controller.ExportEvents();

        // Assert
        var fileResult = result as FileContentResult;
        fileResult!.ContentType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }

    [Fact]
    public async Task ExportEvents_ReturnsCorrectFileName()
    {
        // Arrange
        var excelBytes = new byte[] { 1, 2, 3 };
        _mediator.Send(Arg.Any<GetEventListExcel.Query>())
            .Returns(excelBytes);

        // Act
        var result = await _controller.ExportEvents();

        // Assert
        var fileResult = result as FileContentResult;
        fileResult!.FileDownloadName.Should().Be("events.xlsx");
    }

    [Fact]
    public async Task ExportEvents_SendsCorrectQuery()
    {
        // Arrange
        var excelBytes = new byte[] { 1, 2, 3 };
        _mediator.Send(Arg.Any<GetEventListExcel.Query>())
            .Returns(excelBytes);

        // Act
        await _controller.ExportEvents();

        // Assert
        await _mediator.Received(1).Send(Arg.Any<GetEventListExcel.Query>());
    }

    #endregion

    #region ExportEventsCSV Tests

    [Fact]
    public async Task ExportEventsCSV_ReturnsFileResult()
    {
        // Arrange
        var csvContent = "EventId,EventName\n123,Test Event\n";
        _mediator.Send(Arg.Any<GetEventListCSV.Query>())
            .Returns(csvContent);

        // Act
        var result = await _controller.ExportEventsCSV();

        // Assert
        result.Should().BeOfType<FileContentResult>();
    }

    [Fact]
    public async Task ExportEventsCSV_ReturnsCorrectContentType()
    {
        // Arrange
        var csvContent = "EventId,EventName\n";
        _mediator.Send(Arg.Any<GetEventListCSV.Query>())
            .Returns(csvContent);

        // Act
        var result = await _controller.ExportEventsCSV();

        // Assert
        var fileResult = result as FileContentResult;
        fileResult!.ContentType.Should().Be("text/csv");
    }

    [Fact]
    public async Task ExportEventsCSV_ReturnsCorrectFileName()
    {
        // Arrange
        var csvContent = "EventId,EventName\n";
        _mediator.Send(Arg.Any<GetEventListCSV.Query>())
            .Returns(csvContent);

        // Act
        var result = await _controller.ExportEventsCSV();

        // Assert
        var fileResult = result as FileContentResult;
        fileResult!.FileDownloadName.Should().Be("events.csv");
    }

    [Fact]
    public async Task ExportEventsCSV_SendsCorrectQuery()
    {
        // Arrange
        var csvContent = "EventId,EventName\n";
        _mediator.Send(Arg.Any<GetEventListCSV.Query>())
            .Returns(csvContent);

        // Act
        await _controller.ExportEventsCSV();

        // Assert
        await _mediator.Received(1).Send(Arg.Any<GetEventListCSV.Query>());
    }

    [Fact]
    public async Task ExportEventsCSV_ReturnsUtf8EncodedContent()
    {
        // Arrange
        var csvContent = "EventId,EventName\n123,Événement spécial\n"; // content with special chars
        _mediator.Send(Arg.Any<GetEventListCSV.Query>())
            .Returns(csvContent);

        // Act
        var result = await _controller.ExportEventsCSV();

        // Assert
        var fileResult = result as FileContentResult;
        var decoded = System.Text.Encoding.UTF8.GetString(fileResult!.FileContents);
        decoded.Should().Be(csvContent);
    }

    #endregion
}
