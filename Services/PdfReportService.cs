using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SHSWassceResultsMgt.Models;

namespace SHSWassceResultsMgt.Services;

public static class PdfReportService
{
    /// <summary>
    /// Generates an individual student result slip as a PDF file.
    /// </summary>
    public static void GenerateResultSlip(Student student, List<Score> scores, string outputPath,
        string schoolName = "HASCO SHS", string academicYear = "", string term = "")
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var totalScore = scores.Sum(s => s.Total);
        var average = scores.Count > 0 ? Math.Round(totalScore / scores.Count, 2) : 0;
        var aggregate = scores.OrderBy(s => s.Grade).Take(6).Sum(s => s.Grade);

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(11));

                // ── Header ──
                page.Header().Column(col =>
                {
                    col.Item().AlignCenter().Text(schoolName)
                        .FontSize(20).Bold();
                    col.Item().AlignCenter().Text("STUDENT RESULT SLIP")
                        .FontSize(13).SemiBold();
                    col.Item().PaddingTop(8).LineHorizontal(1);
                });

                // ── Content ──
                page.Content().PaddingVertical(15).Column(col =>
                {
                    // Student info block
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text($"Name: {student.Name}").Bold();
                            c.Item().Text($"Index No.: {student.IndexNumber}");
                            c.Item().Text($"Class: {student.ClassName}");
                        });
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text($"Program: {student.Program}");
                            c.Item().Text($"Academic Year: {academicYear}");
                            c.Item().Text($"Term: {term}");
                        });
                    });

                    col.Item().PaddingVertical(10);

                    // Score table
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3); // Subject
                            columns.RelativeColumn(1); // Class Score
                            columns.RelativeColumn(1); // Exam Score
                            columns.RelativeColumn(1); // Total
                            columns.RelativeColumn(1); // Grade
                            columns.RelativeColumn(2); // Remark
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderCell).Text("Subject");
                            header.Cell().Element(HeaderCell).Text("Class (50)");
                            header.Cell().Element(HeaderCell).Text("Exam (50)");
                            header.Cell().Element(HeaderCell).Text("Total");
                            header.Cell().Element(HeaderCell).Text("Grade");
                            header.Cell().Element(HeaderCell).Text("Remark");

                            static IContainer HeaderCell(IContainer c) => c
                                .Background(Colors.Grey.Lighten2)
                                .Padding(5).DefaultTextStyle(x => x.Bold());
                        });

                        foreach (var s in scores.OrderBy(s => s.Subject.SortOrder))
                        {
                            table.Cell().Element(Cell).Text(s.Subject.Name);
                            table.Cell().Element(Cell).Text(s.ClassScore.ToString("0.##"));
                            table.Cell().Element(Cell).Text(s.ExamScore.ToString("0.##"));
                            table.Cell().Element(Cell).Text(s.Total.ToString("0.##"));
                            table.Cell().Element(Cell).Text(s.Grade.ToString());
                            table.Cell().Element(Cell).Text(s.Remark);

                            static IContainer Cell(IContainer c) => c
                                .BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1)
                                .Padding(5);
                        }
                    });

                    col.Item().PaddingTop(15);

                    // Summary
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text($"Total Score: {totalScore:0.##}").Bold();
                        row.RelativeItem().Text($"Average: {average:0.##}").Bold();
                        row.RelativeItem().Text($"Best 6 Aggregate: {aggregate}").Bold();
                    });

                    col.Item().PaddingTop(40);

                    // Signature lines
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().LineHorizontal(0.5f);
                            c.Item().PaddingTop(4).Text("Form Master / Mistress");
                        });
                        row.ConstantItem(40);
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().LineHorizontal(0.5f);
                            c.Item().PaddingTop(4).Text("Head Teacher");
                        });
                    });
                });

                // ── Footer ──
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Generated by SHS WASSCE Results Management — ").FontSize(8);
                    text.Span(DateTime.Now.ToString("dd MMM yyyy HH:mm")).FontSize(8);
                });
            });
        })
        .GeneratePdf(outputPath);
    }
}
