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
    public partial class TeachersManagementViewModel : ViewModelBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TeachersManagementViewModel> _logger;
        private readonly Func<Teacher, TeacherItemViewModel> _teacherItemFactory;
        private CancellationTokenSource? _cts;

        [ObservableProperty] private bool _isCreating;
        [ObservableProperty] private string _newTeacherFirstName = "";
        [ObservableProperty] private string _newTeacherLastName = "";
        [ObservableProperty] private string _createError = "";
        [ObservableProperty] private string _searchQuery = "";
        [ObservableProperty] private bool _noResults;

        public ObservableCollection<TeacherItemViewModel> Teachers { get; } = new();

        public bool IsCreateFormDirty => IsCreating && 
            !string.IsNullOrWhiteSpace(NewTeacherFirstName) ||
            !string.IsNullOrWhiteSpace(NewTeacherLastName);

        public TeachersManagementViewModel(
            IUnitOfWork unitOfWork,
            ILogger<TeachersManagementViewModel> logger,
            Func<Teacher, TeacherItemViewModel> teacherItemFactory)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _teacherItemFactory = teacherItemFactory;
            _ = LoadAsync();
        }

        private bool CanSaveCreate() =>
            !string.IsNullOrWhiteSpace(NewTeacherFirstName) &&
            !string.IsNullOrWhiteSpace(NewTeacherLastName);

        partial void OnIsCreatingChanged(bool value) =>
            OnPropertyChanged(nameof(IsCreateFormDirty));

        partial void OnNewTeacherFirstNameChanged(string value)
        {
            SaveCreateTeacherCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(IsCreateFormDirty));
        }

        partial void OnNewTeacherLastNameChanged(string value)
        {
            SaveCreateTeacherCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(IsCreateFormDirty));
        }

        partial void OnSearchQueryChanged(string value)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            _ = SearchTeachersAsync(value, _cts.Token);
        }

        private async Task LoadAsync()
        {
            try
            {
                _logger.LogInformation("Loading teachers");
                var teachers = await _unitOfWork.Teachers.GetAllTeachersAsync();

                Teachers.Clear();
                foreach (var t in teachers)
                {
                    var item = _teacherItemFactory(t);
                    item.RequestDelete = vm => Teachers.Remove(vm);
                    Teachers.Add(item);
                }

                _logger.LogInformation("Loaded {Count} teachers", Teachers.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load teachers");
                CreateError = "Failed to load data. Check connection.";
            }
        }

        private async Task SearchTeachersAsync(string searchQuery, CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(_searchDebounceMs, cancellationToken);
                var teachers = string.IsNullOrWhiteSpace(searchQuery)
                    ? await _unitOfWork.Teachers.GetAllTeachersAsync()
                    : await _unitOfWork.Teachers.SearchTeachersAsync(searchQuery);

                Teachers.Clear();
                foreach (var teacher in teachers)
                {
                    var item = _teacherItemFactory(teacher);
                    item.RequestDelete = vm => Teachers.Remove(vm);
                    Teachers.Add(item);
                }

                NoResults = Teachers.Count == 0;
            }
            catch (OperationCanceledException) { }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to search teachers");
            }
        }

        private string BuildCreateError()
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(NewTeacherFirstName)) errors.Add("First Name");
            if (string.IsNullOrWhiteSpace(NewTeacherLastName)) errors.Add("Last Name");
            return BuildRequiredError(errors);
        }

        [RelayCommand]
        private void CreateTeacher()
        {
            IsCreating = true;
        }

        [RelayCommand(CanExecute = nameof(CanSaveCreate))]
        private async Task SaveCreateTeacherAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NewTeacherFirstName) || string.IsNullOrWhiteSpace(NewTeacherLastName))
                {
                    CreateError = BuildCreateError();
                    return;
                }

                CreateError = "";
                _logger.LogInformation("Creating teacher {First} {Last}", NewTeacherFirstName, NewTeacherLastName);

                var teacher = new Teacher
                {
                    Id = Guid.NewGuid(),
                    FirstName = NewTeacherFirstName,
                    LastName = NewTeacherLastName
                };

                _unitOfWork.Teachers.AddTeacher(teacher);
                await _unitOfWork.SaveAsync();

                var item = _teacherItemFactory(teacher);
                item.RequestDelete = vm => Teachers.Remove(vm);
                Teachers.Add(item);

                _logger.LogInformation("Teacher {First} {Last} created", teacher.FirstName, teacher.LastName);
                CancelCreateTeacher();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create teacher");
                CreateError = "Failed to save. Try again.";
            }
        }

        [RelayCommand]
        private void CancelCreateTeacher()
        {
            NewTeacherFirstName = "";
            NewTeacherLastName = "";
            CreateError = "";
            IsCreating = false;
        }
    }
}
