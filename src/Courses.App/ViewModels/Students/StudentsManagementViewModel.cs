using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Courses.App.Interfaces;
using Courses.App.Models;
using Microsoft.Extensions.Logging;

namespace Courses.App.ViewModels
{
    public partial class StudentsManagementViewModel : ViewModelBase
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly ILogger<StudentsManagementViewModel> _logger;
        private readonly Func<Student, IReadOnlyList<Group>, StudentItemViewModel> _studentItemFactory;

        public ObservableCollection<StudentItemViewModel> Students { get; } = new();

        public ObservableCollection<Group> Groups { get; } = new();

        [ObservableProperty] private bool _isCreating;
        [ObservableProperty] private string _newStudentFirstName = "";
        [ObservableProperty] private string _newStudentLastName = "";
        [ObservableProperty] private string _createError = "";
        [ObservableProperty] private Group? _newStudentGroup;

        public StudentsManagementViewModel(
            IStudentRepository studentRepository,
            IGroupRepository groupRepository,
            ILogger<StudentsManagementViewModel> logger,
            Func<Student, IReadOnlyList<Group>, StudentItemViewModel> studentItemFactory)
        {
            _studentRepository = studentRepository;
            _groupRepository = groupRepository;
            _logger = logger;
            _studentItemFactory = studentItemFactory;
            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            try
            {
                _logger.LogInformation("Loading students");
                var students = await _studentRepository.GetAllStudentsAsync();
                var groups = await _groupRepository.GetAllGroupsAsync();

                Groups.Clear();
                foreach (var g in groups)
                {
                    Groups.Add(g);
                }

                Students.Clear();
                foreach (var student in students)
                {
                    var item = _studentItemFactory(student, Groups);
                    item.RequestDelete = vm => Students.Remove(vm);
                    Students.Add(item);
                }

                _logger.LogInformation("Loaded {Count} students", Students.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load students");
                CreateError = "Failed to load data. Check connection.";
            }
        }

        [RelayCommand]
        private void CreateStudent()
        {
            IsCreating = true;
        }

        [RelayCommand]
        private async Task SaveCreateStudentAsync()
        {
            try
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

                var student = new Student
                {
                    Id = Guid.NewGuid(),
                    FirstName = NewStudentFirstName,
                    LastName = NewStudentLastName,
                    GroupId = NewStudentGroup!.Id
                };

                await _studentRepository.AddStudentAsync(student);

                student.Group = NewStudentGroup;
                var item = _studentItemFactory(student, Groups);
                item.RequestDelete = vm => Students.Remove(vm);
                Students.Add(item);
                _logger.LogInformation("Student {First} {Last} created", student.FirstName, student.LastName);

                CancelCreateStudent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create student");
                CreateError = "Failed to save. Try again.";
            }
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