using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Courses.App.Data;
using Courses.App.Models;
using Microsoft.EntityFrameworkCore;

namespace Courses.App.ViewModels
{
    public partial class GroupsManagementViewModel : ViewModelBase
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

        private IStorageProvider? _storageProvider;
        
        public IStorageProvider? StorageProvider
        {
            get => _storageProvider;
            set
            {
                _storageProvider = value;
                foreach (var g in Groups)
                    g.StorageProvider = value;
            }
        }

        public ObservableCollection<GroupItemViewModel> Groups { get; } = new();

        public ObservableCollection<Course> Courses { get; } = new();

        private List<Teacher> _teachers = new();
        
        public IReadOnlyList<Teacher> Teachers => _teachers;

        [ObservableProperty] private bool _isCreating;
        [ObservableProperty] private string _newGroupName = "";
        [ObservableProperty] private Course? _newGroupCourse;
        [ObservableProperty] private Teacher? _newGroupTeacher;
        [ObservableProperty] private string _errorMessage = "";

        public GroupsManagementViewModel(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            var groups = await db.Groups
                .Include(g => g.Teacher)
                .Include(g => g.Students)
                .Include(g => g.Course)
                .ToListAsync();

            _teachers = await db.Teachers.ToListAsync();
            OnPropertyChanged(nameof(Teachers));
            
            var courses = await db.Courses.ToListAsync();

            Courses.Clear();
            foreach (var c in courses) Courses.Add(c);

            Groups.Clear();
            foreach (var g in groups)
                Groups.Add(new GroupItemViewModel(g, _teachers, _dbContextFactory) { StorageProvider = StorageProvider });
        }
        
        [RelayCommand]
        private void CreateGroup() => IsCreating = true;

        [RelayCommand]
        private void CancelCreate()
        {
            IsCreating = false;
            NewGroupName = "";
            NewGroupCourse = null;
            NewGroupTeacher = null;
            ErrorMessage = "";
        }

        [RelayCommand]
        private async Task SaveCreateGroup()
        {
            if (string.IsNullOrWhiteSpace(NewGroupName) || NewGroupCourse is null || NewGroupTeacher is null)
            {
                ErrorMessage = "All fields are required.";
                return;
            }
            
            ErrorMessage = "";
            
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            var group = new Group
            {
                Id = Guid.NewGuid(),
                Name = NewGroupName,
                CourseId = NewGroupCourse.Id,
                TeacherId = NewGroupTeacher?.Id
            };

            db.Groups.Add(group);
            await db.SaveChangesAsync();

            group.Course = NewGroupCourse;
            group.Teacher = NewGroupTeacher;
            
            Groups.Add(new GroupItemViewModel(group, _teachers, _dbContextFactory) { StorageProvider = StorageProvider });
            
            CancelCreate();
        }

        [RelayCommand]
        private async Task DeleteGroup(GroupItemViewModel item)
        {
            if (item.Group.Students.Count > 0)
            {
                item.DeleteError = "Cannot delete group with students.";
                return;
            }
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            var group = await db.Groups.FindAsync(item.Group.Id);
            if(group is null) return;
            db.Groups.Remove(group);
            await db.SaveChangesAsync();
            Groups.Remove(item);
        }
    }
}