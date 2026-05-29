using System.Collections.Generic;
using Courses.App.Models;

namespace Courses.App.Interfaces
{
    public interface IExportService
    {
        void ExportToPdf(Group group, string filePath);
        void ExportToDocx(Group group, string filePath);
        void ExportToCsv(Group group, string filePath);
        List<(string FirstName, string LastName)> ImportFromCsv(string filePath);
    }
}