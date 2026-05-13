using System.Collections.Generic;
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

namespace Courses.App.ViewModels
{
    public partial class GroupItemViewModel : ViewModelBase
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
        public Group Group { get; }
        public IReadOnlyList<Teacher> Teachers { get; }
        public IStorageProvider? StorageProvider { get; set; }

        [ObservableProperty] private bool _isEditing;
        [ObservableProperty] private bool _hasStudentsExist;
        [ObservableProperty] private string _editedName;
        [ObservableProperty] private Teacher? _selectedTeacher;
        [ObservableProperty] private string _deleteError = "";
        [ObservableProperty] private string _editError = "";

        public GroupItemViewModel(Group group, IReadOnlyList<Teacher> teachers, IDbContextFactory<AppDbContext> dbContextFactory)
        {
            Group = group;
            Teachers = teachers;
            _dbContextFactory = dbContextFactory;
            _editedName = group.Name;
            _selectedTeacher = group.Teacher;
            HasStudentsExist = group.Students.Any();
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

            await using var db = await _dbContextFactory.CreateDbContextAsync();
            var group = await db.Groups.FindAsync(Group.Id);
            if(group is null) return;
            
            group.Name = EditedName;
            group.TeacherId = SelectedTeacher?.Id;
            await db.SaveChangesAsync();
            
            Group.Name = EditedName;
            Group.TeacherId = SelectedTeacher?.Id;
            Group.Teacher = SelectedTeacher;
            
            IsEditing = false;
            OnPropertyChanged(nameof(Group));
        }

        [RelayCommand]
        private void Cancel()
        {
            EditedName = Group.Name;
            SelectedTeacher = Group.Teacher;
            IsEditing = false;
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
            ExportService.ExportToDocx(Group, path);
        }
    }
}