using ClosedXML.Excel;
using EarlyWarning.ViewModels;

namespace EarlyWarning.Services;

/// <summary>
/// Writes a <see cref="WeeklyMatrixReport"/> as an .xlsx in the layout of the regional weekly summary:
/// columns A (ተ.ቁ) and B (ዝርዝር ስራዎች), then a two-column block per zone/woreda. Like the paper workbook,
/// at most <see cref="UnitsPerSheet"/> blocks go on one sheet; the rest continue on the next sheet.
/// </summary>
public static class WeeklyMatrixExcelWriter
{
    public const int UnitsPerSheet = 8;
    private const double NumberColumnWidth = 6;
    private const double LabelColumnWidth = 26;
    private const double UnitColumnWidth = 12;   // each of the two sub-columns
    private const double LineHeight = 14.5;

    private static readonly XLColor HeaderFill = XLColor.FromHtml("#D9E7F1");
    private static readonly XLColor SectionFill = XLColor.FromHtml("#F2F2F2");
    private static readonly XLColor TotalFill = XLColor.FromHtml("#FFF5D6");

    public static byte[] Write(WeeklyMatrixReport report)
    {
        using var workbook = new XLWorkbook();
        var pages = Pages(report.Units.Count);
        var unitNumber = 0;
        for (var p = 0; p < pages.Count; p++)
        {
            var (first, count) = pages[p];
            var sheet = workbook.Worksheets.Add(pages.Count == 1 ? "ሪፖርት" : $"ገጽ {p + 1}");
            WriteSheet(sheet, report, first, count, ref unitNumber);
        }
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    // Splits the units into sheets; the trailing total column stays on the last sheet with the units before it.
    internal static List<(int First, int Count)> Pages(int units)
    {
        var pages = new List<(int, int)>();
        if (units == 0) return [(0, 0)];
        for (var first = 0; first < units; first += UnitsPerSheet)
            pages.Add((first, Math.Min(UnitsPerSheet, units - first)));
        // A lone total column on its own sheet is useless: move it back onto the previous sheet.
        if (pages.Count > 1 && pages[^1].Item2 == 1)
        {
            var (first, count) = pages[^2];
            pages[^2] = (first, count + 1);
            pages.RemoveAt(pages.Count - 1);
        }
        return pages;
    }

    private static void WriteSheet(IXLWorksheet sheet, WeeklyMatrixReport report, int first, int count, ref int unitNumber)
    {
        var lastColumn = 2 + count * 2;
        sheet.Style.Font.FontSize = 10;
        sheet.Style.Alignment.WrapText = true;
        sheet.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        sheet.Column(1).Width = NumberColumnWidth;
        sheet.Column(2).Width = LabelColumnWidth;
        for (var c = 3; c <= lastColumn; c++) sheet.Column(c).Width = UnitColumnWidth;

        // Title and scope.
        var title = sheet.Range(1, 1, 1, lastColumn).Merge();
        title.Value = report.Title;
        title.Style.Font.SetBold().Font.FontSize = 14;
        title.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        sheet.Row(1).Height = 30;
        var scope = sheet.Range(2, 1, 2, lastColumn).Merge();
        scope.Value = report.ScopeName;
        scope.Style.Font.SetBold().Font.FontSize = 12;
        scope.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        var firstNumber = unitNumber;
        var row = 3;
        var firstHeaderRow = 0;
        foreach (var line in report.Rows)
        {
            sheet.Cell(row, 1).Value = line.Number;
            sheet.Cell(row, 2).Value = line.Label;
            var lines = EstimatedLines(line.Label, LabelColumnWidth);
            for (var i = 0; i < count; i++)
            {
                var unit = report.Units[first + i];
                var left = sheet.Cell(row, 3 + i * 2);
                var right = sheet.Cell(row, 4 + i * 2);
                var block = sheet.Range(left, right);
                if (unit.IsTotal) block.Style.Fill.BackgroundColor = TotalFill;
                switch (line.Kind)
                {
                    case MatrixRowKind.UnitHeader:
                        block.Merge().Value = unit.IsTotal ? unit.Name : $"{unit.Name} ({firstNumber + i + 1})";
                        block.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        lines = Math.Max(lines, EstimatedLines(unit.Name + " (00)", UnitColumnWidth * 2));
                        break;
                    case MatrixRowKind.Section when line.Cells.Count > 0:
                        left.Value = line.Cells[first + i].Text;
                        right.Value = line.Cells[first + i].RightText;
                        block.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        lines = Math.Max(lines, Math.Max(EstimatedLines(line.Cells[first + i].Text, UnitColumnWidth),
                            EstimatedLines(line.Cells[first + i].RightText, UnitColumnWidth)));
                        break;
                    case MatrixRowKind.Text:
                        var text = line.Cells[first + i].Text;
                        block.Merge().Value = text;
                        block.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        lines = Math.Max(lines, EstimatedLines(text, UnitColumnWidth * 2));
                        break;
                    case MatrixRowKind.Pair:
                        var cell = line.Cells[first + i];
                        if (cell.Left.HasValue) left.Value = cell.Left.Value;
                        if (cell.Right.HasValue) right.Value = cell.Right.Value;
                        block.Style.NumberFormat.Format = line.NumberFormat;
                        block.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        break;
                }
            }

            var rowRange = sheet.Range(row, 1, row, lastColumn);
            switch (line.Kind)
            {
                case MatrixRowKind.UnitHeader:
                    if (firstHeaderRow == 0) firstHeaderRow = row;
                    rowRange.Style.Font.Bold = true;
                    rowRange.Style.Fill.BackgroundColor = HeaderFill;
                    sheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    break;
                case MatrixRowKind.Section:
                    rowRange.Style.Font.Bold = true;
                    sheet.Range(row, 1, row, 2).Style.Fill.BackgroundColor = SectionFill;
                    break;
            }
            // Excel does not size rows to wrapped text in merged cells, so size them from the estimate.
            sheet.Row(row).Height = lines * LineHeight + 4;
            row++;
        }

        var used = sheet.Range(3, 1, row - 1, lastColumn);
        used.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        used.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        sheet.Range(3, 1, row - 1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        sheet.SheetView.FreezeColumns(2);

        sheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
        sheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
        sheet.PageSetup.FitToPages(1, 0);
        sheet.PageSetup.Margins.SetLeft(0.3).SetRight(0.3).SetTop(0.4).SetBottom(0.4);
        if (firstHeaderRow > 0) sheet.PageSetup.SetRowsToRepeatAtTop(firstHeaderRow, firstHeaderRow);

        unitNumber += report.Units.Skip(first).Take(count).Count(x => !x.IsTotal);
    }

    // Ethiopic characters are wide: about one character per width unit.
    internal static double EstimatedLines(string? text, double width) =>
        string.IsNullOrEmpty(text) ? 1 : text.Split('\n').Sum(part => Math.Max(1, Math.Ceiling(part.Length / Math.Max(1, width - 1))));
}
