using Application.Core.Helper;
using Tests.Application.TestHelpers;
using Domain;
using FluentAssertions;
using OfficeOpenXml;
using Xunit;

namespace Tests.Application.Core;

public class ExcelExporterTests : IDisposable
{
    private readonly Persistence.AppDbContext _context;

    public ExcelExporterTests()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        _context = DbContextMockHelper.CreateInMemoryContext();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    // ─────────────────────────────────────────────
    // ExportToExcelBytes (Activities)
    // ─────────────────────────────────────────────

    [Fact]
    public void ExportToExcelBytes_EmptyDatabase_ReturnsByteArray()
    {
        // Arrange
        var exporter = new ExcelExporter(_context);

        // Act
        var result = exporter.ExportToExcelBytes();

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public void ExportToExcelBytes_WithActivities_ReturnsByteArrayWithContent()
    {
        // Arrange
        var activities = ActivityTestData.CreateActivityList(3);
        _context.Activities.AddRange(activities);
        _context.SaveChanges();
        var exporter = new ExcelExporter(_context);

        // Act
        var result = exporter.ExportToExcelBytes();

        // Assert
        result.Should().NotBeNull();
        result.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ExportToExcelBytes_ReturnedBytes_ContainWorksheetNamedActivities()
    {
        // Arrange
        var exporter = new ExcelExporter(_context);

        // Act
        var bytes = exporter.ExportToExcelBytes();

        // Assert - parse the returned bytes as an Excel package
        using var package = new ExcelPackage(new MemoryStream(bytes));
        package.Workbook.Worksheets.Should().ContainSingle(ws => ws.Name == "Activities");
    }

    [Fact]
    public void ExportToExcelBytes_WithOneActivity_ContainsDataRow()
    {
        // Arrange
        var activity = ActivityTestData.CreateValidActivity();
        _context.Activities.Add(activity);
        _context.SaveChanges();
        var exporter = new ExcelExporter(_context);

        // Act
        var bytes = exporter.ExportToExcelBytes();

        // Assert
        using var package = new ExcelPackage(new MemoryStream(bytes));
        var worksheet = package.Workbook.Worksheets["Activities"];
        worksheet.Should().NotBeNull();
        // Row 1 = headers, row 2 = first data row
        worksheet!.Dimension.Rows.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public void ExportToExcelBytes_WithMultipleActivities_ContainsCorrectRowCount()
    {
        // Arrange
        var activities = ActivityTestData.CreateActivityList(5);
        _context.Activities.AddRange(activities);
        _context.SaveChanges();
        var exporter = new ExcelExporter(_context);

        // Act
        var bytes = exporter.ExportToExcelBytes();

        // Assert
        using var package = new ExcelPackage(new MemoryStream(bytes));
        var worksheet = package.Workbook.Worksheets["Activities"];
        // 1 header row + 5 data rows = 6 total rows
        worksheet!.Dimension.Rows.Should().Be(6);
    }

    // ─────────────────────────────────────────────
    // ExportEventsToExcelBytes
    // ─────────────────────────────────────────────

    [Fact]
    public void ExportEventsToExcelBytes_EmptyDatabase_ReturnsByteArray()
    {
        // Arrange
        var exporter = new ExcelExporter(_context);

        // Act
        var result = exporter.ExportEventsToExcelBytes();

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public void ExportEventsToExcelBytes_ReturnedBytes_ContainWorksheetNamedEvents()
    {
        // Arrange
        var exporter = new ExcelExporter(_context);

        // Act
        var bytes = exporter.ExportEventsToExcelBytes();

        // Assert
        using var package = new ExcelPackage(new MemoryStream(bytes));
        package.Workbook.Worksheets.Should().ContainSingle(ws => ws.Name == "Events");
    }

    [Fact]
    public void ExportEventsToExcelBytes_ReturnedBytes_ContainCorrectHeaders()
    {
        // Arrange
        var exporter = new ExcelExporter(_context);

        // Act
        var bytes = exporter.ExportEventsToExcelBytes();

        // Assert
        using var package = new ExcelPackage(new MemoryStream(bytes));
        var worksheet = package.Workbook.Worksheets["Events"];
        worksheet.Should().NotBeNull();
        worksheet!.Cells[1, 1].Value.Should().Be("EventId");
        worksheet.Cells[1, 2].Value.Should().Be("EventName");
        worksheet.Cells[1, 3].Value.Should().Be("EventDescription");
        worksheet.Cells[1, 4].Value.Should().Be("Location");
        worksheet.Cells[1, 5].Value.Should().Be("GroupId");
        worksheet.Cells[1, 6].Value.Should().Be("GroupName");
        worksheet.Cells[1, 7].Value.Should().Be("GroupDescription");
    }

    [Fact]
    public void ExportEventsToExcelBytes_WithOneEvent_ContainsDataRow()
    {
        // Arrange
        var eventEntity = EventTestData.CreateValidEvent();
        _context.Events.Add(eventEntity);
        _context.SaveChanges();
        var exporter = new ExcelExporter(_context);

        // Act
        var bytes = exporter.ExportEventsToExcelBytes();

        // Assert
        using var package = new ExcelPackage(new MemoryStream(bytes));
        var worksheet = package.Workbook.Worksheets["Events"];
        worksheet!.Dimension.Rows.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public void ExportEventsToExcelBytes_WithOneEvent_ContainsCorrectEventData()
    {
        // Arrange
        var eventEntity = EventTestData.CreateValidEvent();
        _context.Events.Add(eventEntity);
        _context.SaveChanges();
        var exporter = new ExcelExporter(_context);

        // Act
        var bytes = exporter.ExportEventsToExcelBytes();

        // Assert
        using var package = new ExcelPackage(new MemoryStream(bytes));
        var worksheet = package.Workbook.Worksheets["Events"];
        worksheet.Should().NotBeNull();
        // Row 2 is the first data row
        worksheet!.Cells[2, 1].Value.Should().Be(eventEntity.EventId.ToString());
        worksheet.Cells[2, 2].Value.Should().Be(eventEntity.EventName);
        worksheet.Cells[2, 3].Value.Should().Be(eventEntity.EventDescription);
        worksheet.Cells[2, 4].Value.Should().Be(eventEntity.Location);
        worksheet.Cells[2, 5].Value.Should().Be(eventEntity.GroupId.ToString());
        worksheet.Cells[2, 6].Value.Should().Be(eventEntity.GroupName);
        worksheet.Cells[2, 7].Value.Should().Be(eventEntity.GroupDescription);
    }

    [Fact]
    public void ExportEventsToExcelBytes_WithMultipleEvents_ContainsCorrectRowCount()
    {
        // Arrange
        var events = EventTestData.CreateEventList(4);
        _context.Events.AddRange(events);
        _context.SaveChanges();
        var exporter = new ExcelExporter(_context);

        // Act
        var bytes = exporter.ExportEventsToExcelBytes();

        // Assert
        using var package = new ExcelPackage(new MemoryStream(bytes));
        var worksheet = package.Workbook.Worksheets["Events"];
        // 1 header row + 4 data rows = 5 total rows
        worksheet!.Dimension.Rows.Should().Be(5);
    }
}
