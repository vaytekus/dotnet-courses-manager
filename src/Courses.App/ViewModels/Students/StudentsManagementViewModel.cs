using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Courses.App.Data;
using Courses.App.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Courses.App.ViewModels
{
    public partial class StudentsManagementViewModel : ViewModelBase
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<StudentsManagementViewModel> _logger;

        public ObservableCollection<StudentItemViewModel> Students { get; } = new();

        public ObservableCollection<Group> Groups { get; } = new();

        [ObservableProperty] private bool _isCreating;
        [ObservableProperty] private string _newStudentFirstName = "";
        [ObservableProperty] private string _newStudentLastName = "";
        [ObservableProperty] private string _createError = "";
        [ObservableProperty] private Group? _newStudentGroup;

        public StudentsManagementViewModel(
            IDbContextFactory<AppDbContext> dbContextFactory, 
            ILoggerFactory loggerFactory)
        {
            _dbContextFactory = dbContextFactory;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<StudentsManagementViewModel>();
            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            _logger.LogInformation("Loading students");
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            var students = await db.Students
                .Include(g => g.Group)
                .ToListAsync();

            var groups = await db.Groups.ToListAsync();

            Groups.Clear();
            foreach (var g in groups)
                Groups.Add(g);

            Students.Clear();
            foreach (var student in students)
            {
                var item = new StudentItemViewModel(student, Groups, _dbContextFactory, _loggerFactory);
                item.RequestDelete = vm => Students.Remove(vm);
                Students.Add(item);
            }
            _logger.LogInformation("Loaded {Count} students", Students.Count);
        }

        [RelayCommand]
        private void CreateStudent() => IsCreating = true;

        [RelayCommand]
        private async Task SaveCreateStudent()
        {
            if (string.IsNullOrWhiteSpace(NewStudentFirstName) ||
                string.IsNullOrWhiteSpace(NewStudentLastName) ||
                NewStudentGroup is null)
            {
                CreateError = "Name or Group is required.";
                return;
            }

            CreateError = "";
            _logger.LogInformation("Creating student {First} {Last}", NewStudentFirstName, NewStudentLastName);

            await using var db = await _dbContextFactory.CreateDbContextAsync();
            var student = new Student
            {
                Id = Guid.NewGuid(),
                FirstName = NewStudentFirstName,
                LastName = NewStudentLastName,
                GroupId = NewStudentGroup!.Id
            };

            db.Students.Add(student);
            await db.SaveChangesAsync();

            student.Group = NewStudentGroup;
            var item = new StudentItemViewModel(student, Groups, _dbContextFactory, _loggerFactory);
            item.RequestDelete = vm => Students.Remove(vm);
            Students.Add(item);
            _logger.LogInformation("Student {First} {Last} created", student.FirstName, student.LastName);

            CancelCreateStudent();
        }

        [RelayCommand]
        private void CancelCreateStudent()
        {
            IsCreating = false;
            NewStudentFirstName = "";
            NewStudentLastName = "";
            NewStudentGroup = null;
            CreateError = "";
        }
    }
}