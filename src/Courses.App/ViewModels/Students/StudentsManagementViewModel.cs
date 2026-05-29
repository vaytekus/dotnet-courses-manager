using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<StudentsManagementViewModel> _logger;
        private readonly Func<Student, IReadOnlyList<Group>, StudentItemViewModel> _studentItemFactory;
        private CancellationTokenSource? _cts;

        [ObservableProperty] private bool _isCreating;
        [ObservableProperty] private string _newStudentFirstName = "";
        [ObservableProperty] private string _newStudentLastName = "";
        [ObservableProperty] private string _createError = "";
        [ObservableProperty] private Group? _newStudentGroup;
        [ObservableProperty] private string _searchQuery = "";
        [ObservableProperty] private bool _noResults;
        [ObservableProperty] private Group? _selectedGroupFilter;

        public ObservableCollection<StudentItemViewModel> Students { get; } = new();
        public ObservableCollection<Group> Groups { get; } = new();
        public bool IsCreateFormDirty => IsCreating &&
            !string.IsNullOrWhiteSpace(NewStudentFirstName) ||
            !string.IsNullOrWhiteSpace(NewStudentLastName) ||
            NewStudentGroup is not null;

        public StudentsManagementViewModel(
            IUnitOfWork unitOfWork,
            ILogger<StudentsManagementViewModel> logger,
            Func<Student, IReadOnlyList<Group>, StudentItemViewModel> studentItemFactory)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _studentItemFactory = studentItemFactory;
            _ = LoadAsync();
        }

        private bool CanSaveCreate() =>
            !string.IsNullOrWhiteSpace(NewStudentFirstName) &&
            !string.IsNullOrWhiteSpace(NewStudentLastName) &&
            NewStudentGroup is not null;

        partial void OnIsCreatingChanged(bool value) =>
            OnPropertyChanged(nameof(IsCreateFormDirty));

        partial void OnNewStudentFirstNameChanged(string value)
        {
            SaveCreateStudentCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(IsCreateFormDirty));
        }

        partial void OnNewStudentLastNameChanged(string value)
        {
            SaveCreateStudentCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(IsCreateFormDirty));
        }

        partial void OnNewStudentGroupChanged(Group? value)
        {
            SaveCreateStudentCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(IsCreateFormDirty));
        }

        partial void OnSearchQueryChanged(string value)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            _ = ReloadStudentsAsync(debounce: true, _cts.Token);
        }

        partial void OnSelectedGroupFilterChanged(Group? value) => _ = ReloadStudentsAsync();
        
        private async Task LoadAsync()
        {
            try
            {
                _logger.LogInformation("Loading students");
                var students = await _unitOfWork.Students.GetAllStudentsAsync();
                var groups = await _unitOfWork.Groups.GetAllGroupsAsync();

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

        private async Task ReloadStudentsAsync(bool debounce = false, CancellationToken cancellationToken = default)
        {
            try
            {
                if (debounce)
                {
                    await Task.Delay(_searchDebounceMs, cancellationToken);
                }

                var students = await _unitOfWork.Students.GetFilteredStudentsAsync(
                    SearchQuery, SelectedGroupFilter?.Id);

                Students.Clear();
                foreach (var student in students)
                {
                    var item = _studentItemFactory(student, Groups);
                    item.RequestDelete = vm => Students.Remove(vm);
                    Students.Add(item);
                }

                NoResults = Students.Count == 0;
            }
            catch (OperationCanceledException) { }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to reload students");
            }
        }

        private string BuildCreateError()
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(NewStudentFirstName)) errors.Add("First Name");
            if (string.IsNullOrWhiteSpace(NewStudentLastName)) errors.Add("Last Name");
            if (NewStudentGroup is null) errors.Add("Group");
            return BuildRequiredError(errors);
        }

        [RelayCommand]
        private void CreateStudent()
        {
            IsCreating = true;
        }

        [RelayCommand(CanExecute = nameof(CanSaveCreate))]
        private async Task SaveCreateStudentAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NewStudentFirstName) ||
                    string.IsNullOrWhiteSpace(NewStudentLastName) ||
                    NewStudentGroup is null)
                {
                    CreateError = BuildCreateError();
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

                _unitOfWork.Students.AddStudent(student);
                await _unitOfWork.SaveAsync();

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
        
        [RelayCommand]
        private void ClearGroupFilter()
        {
            SelectedGroupFilter = null;
        }
    }
}