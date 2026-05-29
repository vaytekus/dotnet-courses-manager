using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Courses.App.Interfaces;
using Courses.App.Models;
using Courses.App.Services;
using Microsoft.Extensions.Logging;

namespace Courses.App.ViewModels
{
    public partial class GroupItemViewModel : ViewModelBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GroupItemViewModel> _logger;
        private readonly IExportService _exportService;

        [ObservableProperty] private bool _isEditing;
        [ObservableProperty] private bool _hasStudentsExist;
        [ObservableProperty] private string _editedName = "";
        [ObservableProperty] private Teacher? _selectedTeacher;
        [ObservableProperty] private string _deleteError = "";
        [ObservableProperty] private string _saveError = "";
        [ObservableProperty] private string _exportError = "";

        public Group Group { get; }
        public IReadOnlyList<Teacher> Teachers { get; }
        public IStorageProvider? StorageProvider { get; set; }
        public ObservableCollection<Student> Students { get; } = new();
        public bool IsDirty => IsEditing && (
            EditedName != Group.Name ||
            SelectedTeacher?.Id != Group.TeacherId); 

        public GroupItemViewModel(
            Group group,
            IReadOnlyList<Teacher> teachers,
            IUnitOfWork unitOfWork,
            ILogger<GroupItemViewModel> logger,
            IExportService exportService)
        {
            Group = group;
            Teachers = teachers;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _exportService = exportService;
            _editedName = group.Name;
            _selectedTeacher = group.Teacher;
            HasStudentsExist = group.Students.Any();
            foreach (var s in group.Students)
            {
                Students.Add(s);
            }
        }

        private bool CanSave() =>
            !string.IsNullOrWhiteSpace(EditedName);
        
        partial void OnIsEditingChanged(bool value) =>
            OnPropertyChanged(nameof(IsDirty));

        partial void OnEditedNameChanged(string value)
        {
            SaveCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(IsDirty));
        }
        
        partial void OnSelectedTeacherChanged(Teacher? value) =>
            OnPropertyChanged(nameof(IsDirty));

        [RelayCommand]
        private void Edit()
        {
            EditedName = Group.Name;
            SelectedTeacher = Group.Teacher;
            IsEditing = true;
        }

        [RelayCommand(CanExecute = nameof(CanSave))]
        private async Task SaveAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(EditedName))
                {
                    SaveError = "Name is required.";
                    return;
                }

                SaveError = "";
                _logger.LogInformation("Saving group {Name}", Group.Name);

                Group.Name = EditedName;
                Group.TeacherId = SelectedTeacher?.Id;
                Group.Teacher = SelectedTeacher;
                
                _unitOfWork.Groups.UpdateGroup(Group);
                await _unitOfWork.SaveAsync();
                
                _logger.LogInformation("Group {Name} saved", Group.Name);
                IsEditing = false;
                OnPropertyChanged(nameof(Group));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save group");
                SaveError = "Failed to save. Try again.";
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            EditedName = Group.Name;
            SelectedTeacher = Group.Teacher;
            IsEditing = false;
            SaveError = "";
        }

        [RelayCommand]
        private async Task ExportPdfAsync()
        {
            try
            {
                if (StorageProvider is null)
                {
                    return;
                }

                var folder = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                {
                    Title = "Select folder to save PDF"
                });

                if (folder.Count == 0)
                {
                    return;
                }

                var path = Path.Combine(folder[0].Path.LocalPath, $"{Group.Name}.pdf");
                _logger.LogInformation("Exporting group {Name} to PDF: {Path}", Group.Name, path);
                _exportService.ExportToPdf(Group, path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export PDF");
                ExportError = "Failed to export PDF. Try again.";
            }
        }

        [RelayCommand]
        private async Task ExportDocxAsync()
        {
            try
            {
                if (StorageProvider is null)
                {
                    return;
                }

                var folder = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                {
                    Title = "Select folder to save DOCX"
                });

                if (folder.Count == 0)
                {
                    return;
                }

                var path = Path.Combine(folder[0].Path.LocalPath, $"{Group.Name}.docx");
                _logger.LogInformation("Exporting group {Name} to DOCX: {Path}", Group.Name, path);
                _exportService.ExportToDocx(Group, path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export DOCX");
                ExportError = "Failed to export DOCX. Try again.";
            }
        }

        [RelayCommand]
        private async Task ExportToCsvAsync()
        {
            try
            {
                if (StorageProvider is null)
                {
                    return;
                }

                var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "Save CSV",
                    SuggestedFileName = $"{Group.Name}.csv",
                    FileTypeChoices = new[] { new FilePickerFileType("CSV") { Patterns = new[] { "*.csv" } } }
                });

                if (file is null)
                {
                    return;
                }

                _logger.LogInformation("Exporting group {Name} to CSV: {Path}", Group.Name, file.Path.LocalPath);
                _exportService.ExportToCsv(Group, file.Path.LocalPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export CSV");
                ExportError = "Failed to export CSV. Try again.";
            }
        }

        [RelayCommand]
        private async Task ImportFromCsvAsync()
        {
            try
            {
                if (StorageProvider is null)
                {
                    return;
                }

                var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Select csv file",
                    AllowMultiple = false,
                    FileTypeFilter = new[] { new FilePickerFileType("CSV") { Patterns = new[] { "*.csv" } } }
                });

                if (files.Count == 0)
                {
                    return;
                }

                _logger.LogInformation("Importing students for group {Name} from CSV: {Path}", Group.Name, files[0].Path.LocalPath);

                var students = _exportService.ImportFromCsv(files[0].Path.LocalPath);

                await _unitOfWork.Students.DeleteAllStudentsByGroupAsync(Group.Id);

                var newStudents = students.Select(s => new Student
                {
                    Id = Guid.NewGuid(),
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    GroupId = Group.Id
                }).ToList();

                _unitOfWork.Students.AddStudentsRange(newStudents);
                await _unitOfWork.SaveAsync();
                
                Group.Students.Clear();
                foreach (var s in newStudents)
                {
                    Group.Students.Add(s);
                }

                Students.Clear();
                foreach (var s in newStudents)
                {
                    Students.Add(s);
                }

                HasStudentsExist = Students.Any();
                _logger.LogInformation("Imported {Count} students to group {Name}", newStudents.Count, Group.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import CSV");
                ExportError = "Failed to import CSV. Try again.";
            }
        }

        [RelayCommand]
        private async Task DeleteAllAsync()
        {
            try
            {
                _logger.LogInformation("Deleting all students from group {Name}", Group.Name);
                await _unitOfWork.Students.DeleteAllStudentsByGroupAsync(Group.Id);
                await _unitOfWork.SaveAsync();

                Group.Students.Clear();
                Students.Clear();
                HasStudentsExist = false;
                _logger.LogInformation("All students deleted from group {Name}", Group.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete all students");
                DeleteError = "Failed to delete students. Try again.";
            }
        }

        [RelayCommand]
        private async Task RemoveStudentAsync(Student student)
        {
            try
            {
                _logger.LogInformation("Removing student {First} {Last} from group {Group}", student.FirstName, student.LastName, Group.Name);
                var s = await _unitOfWork.Students.GetStudentByIdAsync(student.Id);
                if (s is null)
                {
                    return;
                }

                _unitOfWork.Students.RemoveStudent(student);
                await _unitOfWork.SaveAsync();
                
                Group.Students.Remove(student);
                Students.Remove(student);

                HasStudentsExist = Students.Any();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove student");
                DeleteError = "Failed to remove student. Try again.";
            }
        }
    }
}