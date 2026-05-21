using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Courses.App.Models;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPdfDocument = QuestPDF.Fluent.Document;

namespace Courses.App.Services;

public static class ExportService
{
    private static string GetUniqueFilePath(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return filePath;
        }
        var dir = Path.GetDirectoryName(filePath)!;
        var name = Path.GetFileNameWithoutExtension(filePath);
        var ext = Path.GetExtension(filePath);

        int copy = 1;
        string newPath;
        do
        {
            newPath = Path.Combine(dir, $"{name}({copy}){ext}");
            copy++;
        } while(File.Exists(newPath));
        
        return  newPath;
    }
    
    public static void ExportToPdf(Group group, string filePath)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        
        filePath = GetUniqueFilePath(filePath); 

        QuestPdfDocument.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);

                page.Content().Column(col =>
                {
                    col.Item().Text(group.Course.Name).FontSize(18).Bold();
                    col.Item().Text(group.Name).FontSize(14).SemiBold();
                    col.Item().PaddingTop(16);

                    var students = group.Students.ToList();
                    for (int i = 0; i < students.Count; i++)
                    {
                        col.Item().Text($"{i + 1}. {students[i].FirstName} {students[i].LastName}");
                    }
                });
            });
        }).GeneratePdf(filePath);
    }

    public static void ExportToDocx(Group group, string filePath)
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

    public static void ExportToCsv(Group group, string filePath)
    {
        var lines = group.Students
            .Select(s => $"{s.FirstName},{s.LastName}")
            .Prepend("FirstName,LastName");

        File.WriteAllLines(filePath, lines);
    }

    public static List<(string FirstName, string LastName)> ImportFromCsv(string filePath)
    {
        var lines = File.ReadAllLines(filePath)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        if (lines.Count > 0 && lines[0].StartsWith("FirstName", StringComparison.OrdinalIgnoreCase))
            lines = lines.Skip(1).ToList();

        return lines
            .Select(line => line.Split(','))
            .Where(parts => parts.Length >= 2)
            .Select(parts => (parts[0].Trim(), parts[1].Trim()))
            .ToList();
    }
}