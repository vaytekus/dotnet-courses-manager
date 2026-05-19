using System;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Courses.App.Data;
using Courses.App.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Courses.App.ViewModels
{
    public partial class TeacherItemViewModel : ViewModelBase
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
        private readonly ILogger<TeacherItemViewModel> _logger;

        public Teacher Teacher { get; }

        [ObservableProperty] private bool _isEditing;
        [ObservableProperty] private string _editFirstName = "";
        [ObservableProperty] private string _editLastName = "";
        [ObservableProperty] private bool _firstNameError;
        [ObservableProperty] private bool _lastNameError;

        public Action<TeacherItemViewModel>? RequestDelete { get; set; }

        public TeacherItemViewModel(
            Teacher teacher, 
            IDbContextFactory<AppDbContext> dbContextFactory, 
            ILoggerFactory loggerFactory)
        {
            _dbContextFactory = dbContextFactory;
            _logger = loggerFactory.CreateLogger<TeacherItemViewModel>();
            Teacher = teacher;
            _editFirstName = Teacher.FirstName;
            _editLastName = Teacher.LastName;
        }

        [RelayCommand]
        public void Edit()
        {
            EditFirstName = Teacher.FirstName;
            EditLastName = Teacher.LastName;
            IsEditing = true;
        }

        [RelayCommand]
        public void Cancel()
        {
            EditFirstName = Teacher.FirstName;
            EditLastName = Teacher.LastName;
            FirstNameError = false;
            LastNameError = false;
            IsEditing = false;
        }

        [RelayCommand]
        public async Task Save()
        {
            FirstNameError = string.IsNullOrWhiteSpace(EditFirstName);
            LastNameError = string.IsNullOrWhiteSpace(EditLastName);

            if (FirstNameError || LastNameError) return;

            _logger.LogInformation("Saving teacher {First} {Last}", Teacher.FirstName, Teacher.LastName);

            await using var db = await _dbContextFactory.CreateDbContextAsync();
            var teacher = await db.Teachers.FindAsync(Teacher.Id);
            if(teacher is null) return;

            teacher.FirstName = EditFirstName;
            teacher.LastName = EditLastName;

            await db.SaveChangesAsync();

            Teacher.FirstName = EditFirstName;
            Teacher.LastName = EditLastName;
            _logger.LogInformation("Teacher {First} {Last} saved", Teacher.FirstName, Teacher.LastName);
            IsEditing = false;
        }

        [RelayCommand]
        public async Task Delete()
        {
            _logger.LogInformation("Deleting teacher {First} {Last}", Teacher.FirstName, Teacher.LastName);

            await using var db = await _dbContextFactory.CreateDbContextAsync();

            await db.Groups
                .Where(g => g.TeacherId == Teacher.Id)
                .ExecuteUpdateAsync(s => s.SetProperty(g => g.TeacherId, (Guid?)null));

            var teacher = await db.Teachers.FindAsync(Teacher.Id);
            if(teacher is null) return;

            db.Teachers.Remove(teacher);
            await db.SaveChangesAsync();
            _logger.LogInformation("Teacher {First} {Last} deleted", Teacher.FirstName, Teacher.LastName);
            RequestDelete?.Invoke(this);
        }
    }
}