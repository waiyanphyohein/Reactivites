using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using Persistence;

namespace Application.Core.Helper;
public class ExcelExporter
{
    private readonly AppDbContext _context; // Your EF Core context

    public ExcelExporter(AppDbContext context)
    {
        _context = context;
    }

    public void ExportToExcel(string filePath)
    {
        using (var excelPackage = new ExcelPackage(new FileInfo(filePath)))
        {
            // Create a worksheet
            var worksheet = excelPackage.Workbook.Worksheets.Add("Activities");

            // Get the data from Entity Framework
            var activities = _context.Activities.ToList(); // Or use a query to filter

            // Write headers
            // Dynamically map worksheet headers to activity fields
            // Dynamically fetch property info for the Activity type
            var activityType = _context.Activities.EntityType.ClrType;
            var activityProperties = activityType.GetProperties();
            var activityFields = new List<string>();

            foreach (var prop in activityProperties)
            {
                activityFields.Add(prop.Name);
            }

            for (int i = 0; i < activityFields.Count; i++)
            {
                worksheet.Cells[1, i + 1].Value = activityFields[i];
            }

            // Write data rows
            int rowNumber = 2; // Start from row 2 (after headers)
            foreach (var activity in activities)
            {
                worksheet.Cells[rowNumber, 1].Value = activity.Id;
                worksheet.Cells[rowNumber, 2].Value = activity.Title;
                worksheet.Cells[rowNumber, 3].Value = activity.Date;
                worksheet.Cells[rowNumber, 4].Value = activity.Description;
                worksheet.Cells[rowNumber, 5].Value = activity.Category;
                worksheet.Cells[rowNumber, 6].Value = activity.IsCancelled;
                worksheet.Cells[rowNumber, 7].Value = activity.City;
                worksheet.Cells[rowNumber, 8].Value = activity.Venue;
                worksheet.Cells[rowNumber, 9].Value = activity.Latitude;
                worksheet.Cells[rowNumber, 10].Value = activity.Longitude;
                rowNumber++;
            }

            // Create Excel Table
            var lastColumn = GetColumnLetter(activityFields.Count);
            var tableRange = worksheet.Cells[$"A1:{lastColumn}{rowNumber - 1}"]; // Range including headers and data
            var table = worksheet.Tables.Add(tableRange, "ActivitiesTable");

            // Set columns to be included in the Excel table
            table.ShowHeader = true;
            table.TableStyle = OfficeOpenXml.Table.TableStyles.Medium2; // Set a table style (optional)

            worksheet.Calculate();
            // Save changes to the worksheet with the table before saving the package
            // No additional code needed to "export" the table itself, as the Epplus Table object is a logical structure within the worksheet.
            
            // Auto-fit columns (optional)
            worksheet.Cells.AutoFitColumns();

            // Save the Excel file
            excelPackage.Save();
        }
    }

    /// <summary>
    /// Exports activities to Excel format in memory and returns as byte array
    /// </summary>
    public byte[] ExportToExcelBytes()
    {
        using (var excelPackage = new ExcelPackage())
        {
            // Create a worksheet
            var worksheet = excelPackage.Workbook.Worksheets.Add("Activities");

            // Get the data from Entity Framework
            var activities = _context.Activities.ToList();

            // Write headers
            var activityType = _context.Activities.EntityType.ClrType;
            var activityProperties = activityType.GetProperties();
            var activityFields = new List<string>();

            foreach (var prop in activityProperties)
            {
                activityFields.Add(prop.Name);
            }

            for (int i = 0; i < activityFields.Count; i++)
            {
                worksheet.Cells[1, i + 1].Value = activityFields[i];
            }

            // Write data rows
            int rowNumber = 2; // Start from row 2 (after headers)
            foreach (var activity in activities)
            {
                worksheet.Cells[rowNumber, 1].Value = activity.Id;
                worksheet.Cells[rowNumber, 2].Value = activity.Title;
                worksheet.Cells[rowNumber, 3].Value = activity.Date;
                worksheet.Cells[rowNumber, 4].Value = activity.Description;
                worksheet.Cells[rowNumber, 5].Value = activity.Category;
                worksheet.Cells[rowNumber, 6].Value = activity.IsCancelled;
                worksheet.Cells[rowNumber, 7].Value = activity.City;
                worksheet.Cells[rowNumber, 8].Value = activity.Venue;
                worksheet.Cells[rowNumber, 9].Value = activity.Latitude;
                worksheet.Cells[rowNumber, 10].Value = activity.Longitude;
                rowNumber++;
            }

            // Create Excel Table
            var lastColumn = GetColumnLetter(activityFields.Count);
            var tableRange = worksheet.Cells[$"A1:{lastColumn}{rowNumber - 1}"];
            var table = worksheet.Tables.Add(tableRange, "ActivitiesTable");

            // Set columns to be included in the Excel table
            table.ShowHeader = true;
            table.TableStyle = OfficeOpenXml.Table.TableStyles.Medium2;

            worksheet.Calculate();
            
            // Auto-fit columns
            worksheet.Cells.AutoFitColumns();

            // Return as byte array
            return excelPackage.GetAsByteArray();
        }
    }

    /// <summary>
    /// Exports events to Excel format in memory and returns as byte array
    /// </summary>
    public byte[] ExportEventsToExcelBytes()
    {
        using (var excelPackage = new ExcelPackage())
        {
            // Create a worksheet
            var worksheet = excelPackage.Workbook.Worksheets.Add("Events");

            // Get the data from Entity Framework
            var events = _context.Events.ToList();

            // Write headers - Event properties
            var headers = new List<string> 
            { 
                "EventId", 
                "EventName", 
                "EventDescription", 
                "Location",
                "GroupId",
                "GroupName",
                "GroupDescription"
            };
            
            for (int i = 0; i < headers.Count; i++)
            {
                worksheet.Cells[1, i + 1].Value = headers[i];
            }

            // Write data rows
            int rowNumber = 2; // Start from row 2 (after headers)
            foreach (var eventEntity in events)
            {
                worksheet.Cells[rowNumber, 1].Value = eventEntity.EventId.ToString();
                worksheet.Cells[rowNumber, 2].Value = eventEntity.EventName;
                worksheet.Cells[rowNumber, 3].Value = eventEntity.EventDescription;
                worksheet.Cells[rowNumber, 4].Value = eventEntity.Location;
                worksheet.Cells[rowNumber, 5].Value = eventEntity.GroupId.ToString();
                worksheet.Cells[rowNumber, 6].Value = eventEntity.GroupName;
                worksheet.Cells[rowNumber, 7].Value = eventEntity.GroupDescription;
                rowNumber++;
            }

            // Create Excel Table
            var lastColumn = GetColumnLetter(headers.Count);
            var tableRange = worksheet.Cells[$"A1:{lastColumn}{rowNumber - 1}"];
            var table = worksheet.Tables.Add(tableRange, "EventsTable");

            // Set columns to be included in the Excel table
            table.ShowHeader = true;
            table.TableStyle = OfficeOpenXml.Table.TableStyles.Medium2;

            worksheet.Calculate();
            
            // Auto-fit columns
            worksheet.Cells.AutoFitColumns();

            // Return as byte array
            return excelPackage.GetAsByteArray();
        }
    }

    /// <summary>
    /// Helper method to convert column number to Excel column letter (1 -> A, 27 -> AA, etc.)
    /// </summary>
    private string GetColumnLetter(int columnNumber)
    {
        string columnLetter = "";
        while (columnNumber > 0)
        {
            columnNumber--;
            columnLetter = (char)('A' + columnNumber % 26) + columnLetter;
            columnNumber /= 26;
        }
        return columnLetter;
    }
}