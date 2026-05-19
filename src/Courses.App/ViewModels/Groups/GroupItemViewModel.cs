using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Courses.App.Data;
using Courses.App.Models;
using Courses.App.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Courses.App.ViewModels
{
    public partial class GroupItemViewModel : ViewModelBase
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
        private readonly ILogger<GroupItemViewModel> _logger;

        public Group Group { get; }
        public IReadOnlyList<Teacher> Teachers { get; }
        public IStorageProvider? StorageProvider { get; set; }

        [ObservableProperty] private bool _isEditing;
        [ObservableProperty] private bool _hasStudentsExist;
        [ObservableProperty] private string _editedName = "";
        [ObservableProperty] private Teacher? _selectedTeacher;
        [ObservableProperty] private string _deleteError = "";
        [ObservableProperty] private string _editError = "";

        public ObservableCollection<Student> Students { get; } = new();

        public GroupItemViewModel(
            Group group, 
            IReadOnlyList<Teacher> teachers, 
            IDbContextFactory<AppDbContext> dbContextFactory, 
            ILoggerFactory loggerFactory)
        {
            Group = group;
            Teachers = teachers;
            _dbContextFactory = dbContextFactory;
            _logger = loggerFactory.CreateLogger<GroupItemViewModel>();
            _editedName = group.Name;
            _selectedTeacher = group.Teacher;
            HasStudentsExist = group.Students.Any();
            foreach (var s in group.Students) Students.Add(s);
        }

        [RelayCommand]
        private void Edit()
        {
            EditedName = Group.Name;
            SelectedTeacher = Group.Teacher;
            IsEditing = true;
        }

        [RelayCommand]
        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(EditedName))
            {
                EditError = "Name is required.";
                return;
            }
            EditError = "";

            _logger.LogInformation("Saving group {Name}", Group.Name);

            await using var db = await _dbContextFactory.CreateDbContextAsync();
            var group = await db.Groups.FindAsync(Group.Id);
            if(group is null) return;

            group.Name = EditedName;
            group.TeacherId = SelectedTeacher?.Id;
            await db.SaveChangesAsync();

            Group.Name = EditedName;
            Group.TeacherId = SelectedTeacher?.Id;
            Group.Teacher = SelectedTeacher;

            _logger.LogInformation("Group {Name} saved", Group.Name);
            IsEditing = false;
            OnPropertyChanged(nameof(Group));
        }

        [RelayCommand]
        private void Cancel()
        {
            EditedName = Group.Name;
            SelectedTeacher = Group.Teacher;
            IsEditing = false;
            EditError = "";
        }

        [RelayCommand]
        private async Task ExportPdf()
        {
            if (StorageProvider is null) return;
            var folder = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select folder to save PDF"
            });
            if (folder.Count == 0) return;
            var path = Path.Combine(folder[0].Path.LocalPath, $"{Group.Name}.pdf");
            _logger.LogInformation("Exporting group {Name} to PDF: {Path}", Group.Name, path);
            ExportService.ExportToPdf(Group, path);
        }

        [RelayCommand]
        private async Task ExportDocx()
        {
            if (StorageProvider is null) return;
            var folder = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select folder to save DOCX"
            });
            if (folder.Count == 0) return;
            var path = Path.Combine(folder[0].Path.LocalPath, $"{Group.Name}.docx");
            _logger.LogInformation("Exporting group {Name} to DOCX: {Path}", Group.Name, path);
            ExportService.ExportToDocx(Group, path);
        }

        [RelayCommand]
        private async Task ExportToCsv()
        {
            if(StorageProvider is null) return;

            var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save CSV",
                SuggestedFileName = $"{Group.Name}.csv",
                FileTypeChoices = new[] { new FilePickerFileType("CSV") { Patterns = new[] { "*.csv" } } }
            });

            if(file is null) return;
            _logger.LogInformation("Exporting group {Name} to CSV: {Path}", Group.Name, file.Path.LocalPath);
            ExportService.ExportToCsv(Group, file.Path.LocalPath);
        }

        [RelayCommand]
        private async Task ImportToCsv()
        {
            if(StorageProvider is null) return;

            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select csv file",
                AllowMultiple = false,
                FileTypeFilter = new[] { new FilePickerFileType("CSV") { Patterns = new[] { "*.csv" } } }
            });

            if(files.Count == 0) return;
            _logger.LogInformation("Importing students for group {Name} from CSV: {Path}", Group.Name, files[0].Path.LocalPath);

            var students = ExportService.ImportFromCsv(files[0].Path.LocalPath);

            await using var db = await _dbContextFactory.CreateDbContextAsync();
            await db.Students.Where(s => s.GroupId == Group.Id).ExecuteDeleteAsync();

            var newStudents = students.Select(s => new Student
            {
                Id = Guid.NewGuid(),
                FirstName = s.FirstName,
                LastName = s.LastName,
                GroupId = Group.Id
            }).ToList();

            db.Students.AddRange(newStudents);
            await db.SaveChangesAsync();

            Group.Students.Clear();
            foreach (var s in newStudents) Group.Students.Add(s);
            Students.Clear();
            foreach (var s in newStudents) Students.Add(s);

            HasStudentsExist = Students.Any();
            _logger.LogInformation("Imported {Count} students to group {Name}", newStudents.Count, Group.Name);
        }

        [RelayCommand]
        public async Task DeleteAll()
        {
            _logger.LogInformation("Deleting all students from group {Name}", Group.Name);
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            await db.Students.Where(s => s.GroupId == Group.Id).ExecuteDeleteAsync();

            Group.Students.Clear();
            Students.Clear();
            HasStudentsExist = false;
            _logger.LogInformation("All students deleted from group {Name}", Group.Name);
        }

        [RelayCommand]
        public async Task RemoveStudent(Student student)
        {
            _logger.LogInformation("Removing student {First} {Last} from group {Group}", student.FirstName, student.LastName, Group.Name);
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            var s = await db.Students.FindAsync(student.Id);
            if(s is null) return;

            db.Students.Remove(s);
            await db.SaveChangesAsync();

            Group.Students.Remove(student);
            Students.Remove(student);

            HasStudentsExist = Students.Any();
        }
    }
}