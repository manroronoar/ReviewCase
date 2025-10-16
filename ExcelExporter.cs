using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using A = DocumentFormat.OpenXml.Drawing;
using C = DocumentFormat.OpenXml.Drawing.Charts;
using X = DocumentFormat.OpenXml.Drawing.Spreadsheet; // ใช้สำหรับ Drawing.Spreadsheet
using D = DocumentFormat.OpenXml.Spreadsheet; // ใช้สำหรับ Spreadsheet ทั่วไป
using System;
using System.Collections.Generic;
using System.Linq;
using WpfTestCase;

public class ExcelExporter
{
    public class ChartDataItem
    {
        public string Label { get; set; }
        public int Count { get; set; }
    }
    public void ExportOrdersToExcel(IEnumerable<Order> orders, string filePath)
    {
        try
        {
            using (var spreadsheet = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook))
            {
                // Create workbook parts
                var workbookPart = spreadsheet.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                // Add stylesheet
                AddBasicStylesheet(workbookPart);

                // Create sheets collection
                var sheets = workbookPart.Workbook.AppendChild(new Sheets());

                // ===== Worksheet 1: Data =====
                var worksheetPart1 = workbookPart.AddNewPart<WorksheetPart>();
                var sheetData1 = new D.SheetData();

                worksheetPart1.Worksheet = new D.Worksheet(
                    new D.SheetViews(new D.SheetView { WorkbookViewId = 0 }),
                    new D.SheetFormatProperties { DefaultRowHeight = 15D },
                    sheetData1,
                    new D.PageMargins
                    {
                        Left = 0.7D,
                        Right = 0.7D,
                        Top = 0.75D,
                        Bottom = 0.75D,
                        Header = 0.3D,
                        Footer = 0.3D
                    }
                );

                sheets.Append(new D.Sheet
                {
                    Id = workbookPart.GetIdOfPart(worksheetPart1),
                    SheetId = 1,
                    Name = "Orders"
                });

                // Add data to worksheet
                AddDataToWorksheet(sheetData1, orders);

                // ===== Worksheet 2: Charts =====
                var worksheetPart2 = workbookPart.AddNewPart<WorksheetPart>();
                var sheetData2 = new D.SheetData();

                worksheetPart2.Worksheet = new D.Worksheet(
                    new D.SheetViews(new D.SheetView { WorkbookViewId = 0 }),
                    new D.SheetFormatProperties { DefaultRowHeight = 15D },
                    sheetData2,
                    new D.PageMargins
                    {
                        Left = 0.7D,
                        Right = 0.7D,
                        Top = 0.75D,
                        Bottom = 0.75D,
                        Header = 0.3D,
                        Footer = 0.3D
                    }
                );

                sheets.Append(new D.Sheet
                {
                    Id = workbookPart.GetIdOfPart(worksheetPart2),
                    SheetId = 2,
                    Name = "Charts"
                });

                // Add chart to worksheet
                AddChartToWorksheet(workbookPart, worksheetPart2, orders);

                // Save all parts
                worksheetPart1.Worksheet.Save();
                worksheetPart2.Worksheet.Save();
                workbookPart.Workbook.Save();
            }
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Error creating Excel file", ex);
        }
    }
    private void AddBasicStylesheet(WorkbookPart workbookPart)
    {
        var stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
        stylesPart.Stylesheet = new D.Stylesheet(
            new D.Fonts(new D.Font()),
            new D.Fills(new D.Fill()),
            new D.Borders(new D.Border()),
            new D.CellFormats(new D.CellFormat())
        );
        stylesPart.Stylesheet.Save();
    }
    private void AddDataToWorksheet(D.SheetData sheetData, IEnumerable<Order> orders)
    {
        // Header row
        var headerRow = new D.Row();
        headerRow.Append(
            CreateCell("ORDER_ID", D.CellValues.String),
            CreateCell("CASE REVIEWS", D.CellValues.String),
            CreateCell("ERROR", D.CellValues.String),
            CreateCell("ROOT CAUSE", D.CellValues.String)
        );
        sheetData.Append(headerRow);

        // Data rows
        foreach (var order in orders)
        {
            var dataRow = new D.Row();
            dataRow.Append(
                CreateCell(order.OrderId.ToString(), D.CellValues.String),
                CreateCell(order.CaseReviews, D.CellValues.String),
                CreateCell(order.Error, D.CellValues.String),
                CreateCell(order.RootCause, D.CellValues.String)
            );
            sheetData.Append(dataRow);
        }
    }
    private D.Cell CreateCell(string text, D.CellValues dataType)
    {
        return new D.Cell
        {
            DataType = dataType,
            CellValue = new D.CellValue(text)
        };
    }
    private void AddChartToWorksheet(WorkbookPart workbookPart, WorksheetPart worksheetPart, IEnumerable<Order> orders)
    {
        // Create drawings part
        var drawingsPart = worksheetPart.AddNewPart<DrawingsPart>();
        worksheetPart.Worksheet.Append(new Drawing
        {
            Id = worksheetPart.GetIdOfPart(drawingsPart)
        });

        // Prepare chart data
        var chartData = orders
            .GroupBy(o => o.CaseReviews)
            .Select(g => new ChartDataItem { Label = g.Key ?? "Unknown", Count = g.Count() })
            .ToList();

        // Create chart part
        var chartPart = drawingsPart.AddNewPart<ChartPart>();
        chartPart.ChartSpace = new C.ChartSpace(
            new C.Date1904 { Val = false },
            new C.EditingLanguage { Val = "en-US" },
            new C.RoundedCorners { Val = false },
            new C.Chart(
                new C.AutoTitleDeleted { Val = true },
                new C.PlotVisibleOnly { Val = true },
                new C.PlotArea(
                    new C.Layout(),
                    new C.PieChart(
                        new C.VaryColors { Val = true },
                        CreatePieChartSeries(chartData)
                    )
                ),
                new C.Legend(
                    new C.LegendPosition { Val = C.LegendPositionValues.Right }
                )
            )
        );

        // Create worksheet drawing
        drawingsPart.WorksheetDrawing = new X.WorksheetDrawing();
        drawingsPart.WorksheetDrawing.Append(
            new X.TwoCellAnchor(
                new X.FromMarker(
                    new X.ColumnId("1"),
                    new X.ColumnOffset("0"),
                    new X.RowId("3"),
                    new X.RowOffset("0")
                ),
                new X.ToMarker(
                    new X.ColumnId("19"),
                    new X.ColumnOffset("0"),
                    new X.RowId("40"),
                    new X.RowOffset("0")
                ),
                new X.GraphicFrame(
                    new X.NonVisualGraphicFrameProperties(
                        new X.NonVisualDrawingProperties { Id = 1U, Name = "Chart 1" },
                        new X.NonVisualGraphicFrameDrawingProperties()
                    ),
                    new X.Transform(
                        new A.Offset { X = 0L, Y = 0L },
                        new A.Extents { Cx = 6000000L, Cy = 4000000L }
                    ),
                    new A.Graphic(
                        new A.GraphicData(
                            new C.ChartReference { Id = drawingsPart.GetIdOfPart(chartPart) }
                        )
                        { Uri = "http://schemas.openxmlformats.org/drawingml/2006/chart" }
                    )
                ),
                new X.ClientData()
            )
        );
    }
    private C.PieChartSeries CreatePieChartSeries(List<ChartDataItem> chartData)
    {
        return new C.PieChartSeries(
             new C.Index { Val = 0U },
             new C.Order { Val = 0U },
             new C.SeriesText(new C.StringReference(
                 new C.StringCache(
                     new C.PointCount { Val = 0U }
                 )
             )),
             CreateCategoryAxisData(chartData),
             CreateValues(chartData),
             new C.DataLabels(
                 new C.ShowLegendKey { Val = false },
                 new C.ShowValue { Val = true },
                 new C.ShowCategoryName { Val = true },
                 new C.ShowPercent { Val = false }
             )
         );
    }
    private C.CategoryAxisData CreateCategoryAxisData(List<ChartDataItem> chartData)
    {
        var categoryAxisData = new C.CategoryAxisData();
        var stringLiteral = new C.StringLiteral();

        stringLiteral.Append(new C.PointCount { Val = (uint)chartData.Count });

        for (uint i = 0; i < chartData.Count; i++)
        {
            stringLiteral.Append(new C.StringPoint
            {
                Index = i,
                NumericValue = new C.NumericValue(chartData[(int)i].Label)
            });
        }

        categoryAxisData.Append(stringLiteral);
        return categoryAxisData;
    }
    private C.Values CreateValues(List<ChartDataItem> chartData)
    {
        var values = new C.Values();
        var numberLiteral = new C.NumberLiteral();

        numberLiteral.Append(new C.PointCount { Val = (uint)chartData.Count });

        for (uint i = 0; i < chartData.Count; i++)
        {
            numberLiteral.Append(new C.NumericPoint
            {
                Index = i,
                NumericValue = new C.NumericValue(chartData[(int)i].Count.ToString())
            });
        }

        values.Append(numberLiteral);
        return values;
    }
}