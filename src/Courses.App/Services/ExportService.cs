using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Courses.App.Interfaces;
using Courses.App.Models;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPdfDocument = QuestPDF.Fluent.Document;

namespace Courses.App.Services;

public class ExportService : IExportService
{
    private const int _initialCopyIndex = 1;
    private const int _pdfMargin = 40;
    private const int _titleFontSize = 18;
    private const int _subtitleFontSize = 14;
    private const int _sectionPadding = 16;
    private const string _csvHeader = "FirstName";
    private const int _csvHeaderRowCount = 1;
    private const int _csvColumnCount = 2;
    private const int _firstNameIndex = 0;
    private const int _lastNameIndex = 1;

    public void ExportToPdf(Group group, string filePath)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        filePath = GetUniqueFilePath(filePath);

        QuestPdfDocument.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(_pdfMargin);

                page.Content().Column(col =>
                {
                    col.Item().Text(group.Course.Name).FontSize(_titleFontSize).Bold();
                    col.Item().Text(group.Name).FontSize(_subtitleFontSize).SemiBold();
                    col.Item().PaddingTop(_sectionPadding);

                    var students = group.Students.ToList();
                    for (int i = 0; i < students.Count; i++)
                    {
                        col.Item().Text($"{i + 1}. {students[i].FirstName} {students[i].LastName}");
                    }
                });
            });
        }).GeneratePdf(filePath);
    }

    public void ExportToDocx(Group group, string filePath)
    {
        filePath = GetUniqueFilePath(filePath);
        using var doc = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document);
        var mainPart = doc.AddMainDocumentPart();
        mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document(new Body());
        var body = mainPart.Document.Body!;

        body.AppendChild(new Paragraph(
            new Run(new RunProperties(new Bold()), new Text(group.Course.Name))));

        body.AppendChild(new Paragraph(
            new Run(new RunProperties(new Bold()), new Text(group.Name))));

        body.AppendChild(new Paragraph());

        var students = group.Students.ToList();
        for (int i = 0; i < students.Count; i++)
        {
            body.AppendChild(new Paragraph(
                new Run(new Text($"{i + 1}. {students[i].FirstName} {students[i].LastName}"))));
        }

        mainPart.Document.Save();
    }

    public void ExportToCsv(Group group, string filePath)
    {
        var lines = group.Students
            .Select(s => $"{s.FirstName},{s.LastName}")
            .Prepend("FirstName,LastName");

        File.WriteAllLines(filePath, lines);
    }

    public List<(string FirstName, string LastName)> ImportFromCsv(string filePath)
    {
        var lines = File.ReadAllLines(filePath)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        if (lines.Count > 0 && lines[0].StartsWith(_csvHeader, StringComparison.OrdinalIgnoreCase))
        {
            lines = lines.Skip(_csvHeaderRowCount).ToList();
        }

        return lines
            .Select(line => line.Split(','))
            .Where(parts => parts.Length >= _csvColumnCount)
            .Select(parts => (parts[_firstNameIndex].Trim(), parts[_lastNameIndex].Trim()))
            .ToList();
    }

    private static string GetUniqueFilePath(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return filePath;
        }

        var dir = Path.GetDirectoryName(filePath)!;
        var name = Path.GetFileNameWithoutExtension(filePath);
        var ext = Path.GetExtension(filePath);

        int copy = _initialCopyIndex;
        string newPath;
        do
        {
            newPath = Path.Combine(dir, $"{name}({copy}){ext}");
            copy++;
        } while (File.Exists(newPath));

        return newPath;
    }
}
