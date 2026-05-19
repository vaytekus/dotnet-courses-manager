using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Courses.App.Data;
using Courses.App.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Courses.App.ViewModels
{
    public partial class StudentItemViewModel : ViewModelBase
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
        private readonly ILogger<StudentItemViewModel> _logger;

        public Student Student { get; }

        public IReadOnlyList<Group> Groups { get; }

        [ObservableProperty] private Group? _selectedGroup;
        [ObservableProperty] private bool _isEditing;
        [ObservableProperty] private string _editFirstName = "";
        [ObservableProperty] private string _editLastName = "";
        [ObservableProperty] private bool _firstNameError;
        [ObservableProperty] private bool _lastNameError;

        public Action<StudentItemViewModel>? RequestDelete { get; set; }

        public StudentItemViewModel(
            Student student, 
            IReadOnlyList<Group> groups, 
            IDbContextFactory<AppDbContext> dbContextFactory, 
            ILoggerFactory loggerFactory)
        {
            Student = student;
            Groups = groups;
            _dbContextFactory = dbContextFactory;
            _logger = loggerFactory.CreateLogger<StudentItemViewModel>();
            _editFirstName = student.FirstName;
            _editLastName = student.LastName;
            _selectedGroup = student.Group;
        }

        [RelayCommand]
        private void Edit()
        {
            EditFirstName = Student.FirstName;
            EditLastName = Student.LastName;
            SelectedGroup = Student.Group;
            IsEditing = true;
        }

        [RelayCommand]
        private void Cancel()
        {
            EditFirstName = Student.FirstName;
            EditLastName = Student.LastName;
            SelectedGroup = Student.Group;
            FirstNameError = false;
            LastNameError = false;
            IsEditing = false;
        }

        [RelayCommand]
        private async Task Save()
        {
            FirstNameError = string.IsNullOrWhiteSpace(EditFirstName);
            LastNameError = string.IsNullOrWhiteSpace(EditLastName);

            if (FirstNameError || LastNameError) return;

            _logger.LogInformation("Saving student {First} {Last}", Student.FirstName, Student.LastName);

            await using var db = await _dbContextFactory.CreateDbContextAsync();
            var student = await db.Students.FindAsync(Student.Id);
            if(student is null) return;

            student.FirstName = EditFirstName;
            student.LastName = EditLastName;
            student.GroupId = SelectedGroup?.Id ?? Student.GroupId;
            await db.SaveChangesAsync();

            Student.FirstName = EditFirstName;
            Student.LastName = EditLastName;
            Student.GroupId = student.GroupId;
            Student.Group = SelectedGroup ?? Student.Group;
            _logger.LogInformation("Student {First} {Last} saved", Student.FirstName, Student.LastName);
            IsEditing = false;
        }

        [RelayCommand]
        public async Task Delete()
        {
            _logger.LogInformation("Deleting student {First} {Last}", Student.FirstName, Student.LastName);
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            var student = await db.Students.FindAsync(Student.Id);
            if(student is null) return;
            db.Students.Remove(student);
            await db.SaveChangesAsync();
            _logger.LogInformation("Student {First} {Last} deleted", Student.FirstName, Student.LastName);
            RequestDelete?.Invoke(this);
        }
    }
}