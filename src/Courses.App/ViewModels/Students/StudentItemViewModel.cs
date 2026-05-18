using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Courses.App.Data;
using Courses.App.Models;
using Microsoft.EntityFrameworkCore;

namespace Courses.App.ViewModels
{
    public partial class StudentItemViewModel : ViewModelBase
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

        public Student Student { get; }

        public IReadOnlyList<Group> Groups { get; }

        [ObservableProperty] private Group? _selectedGroup;
        [ObservableProperty] private bool _isEditing;
        [ObservableProperty] private string _editFirstName = "";
        [ObservableProperty] private string _editLastName = "";
        [ObservableProperty] private bool _firstNameError;
        [ObservableProperty] private bool _lastNameError;

        public Action<StudentItemViewModel>? RequestDelete { get; set; }

        public StudentItemViewModel(Student student, IReadOnlyList<Group> groups, IDbContextFactory<AppDbContext> dbContextFactory)
        {
            Student = student;
            Groups = groups;
            _dbContextFactory = dbContextFactory;
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
            IsEditing = false;
        }

        [RelayCommand]
        public async Task Delete()
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            var student = await db.Students.FindAsync(Student.Id);
            if(student is null) return;
            db.Students.Remove(student);
            await db.SaveChangesAsync();
            RequestDelete?.Invoke(this);
        }
    }
}
