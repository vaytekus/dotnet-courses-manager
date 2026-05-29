using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Courses.App.Enums;
using Courses.App.Interfaces;
using Courses.App.Models;
using Microsoft.Extensions.Logging;

namespace Courses.App.ViewModels
{
    public partial class GroupsManagementViewModel : ViewModelBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GroupsManagementViewModel> _logger;
        private readonly Func<Group, IReadOnlyList<Teacher>, GroupItemViewModel> _groupItemFactory;
        private IStorageProvider? _storageProvider;
        private List<Teacher> _teachers = new();
        private CancellationTokenSource? _cts;

        [ObservableProperty] private bool _isCreating;
        [ObservableProperty] private string _newGroupName = "";
        [ObservableProperty] private Course? _newGroupCourse;
        [ObservableProperty] private Teacher? _newGroupTeacher;
        [ObservableProperty] private string _createError = "";
        [ObservableProperty] private string _deleteError = "";
        [ObservableProperty] private string _searchQuery = "";
        [ObservableProperty] private bool _noResults;
        [ObservableProperty] private Course? _selectedCourseFilter;
        public IReadOnlyList<StudentFilterOption> StudentFilters { get; } = new[]
        {
            new StudentFilterOption { Value = GroupStudentFilter.All, Label = "All Groups" },
            new StudentFilterOption { Value = GroupStudentFilter.WithStudents, Label = "With Students" },
            new StudentFilterOption { Value = GroupStudentFilter.WithoutStudents, Label = "Without Students" },
        };

        [ObservableProperty] private StudentFilterOption _selectedStudentFilter = null!;

        public ObservableCollection<GroupItemViewModel> Groups { get; } = new();
        public ObservableCollection<Course> Courses { get; } = new();
        public IReadOnlyList<Teacher> Teachers
        {
            get { return _teachers; }
        }
        public IStorageProvider? StorageProvider
        {
            get { return _storageProvider; }
            set
            {
                _storageProvider = value;
                foreach (var g in Groups)
                {
                    g.StorageProvider = value;
                }
            }
        }
        public bool IsCreateFormDirty => IsCreating &&
            !string.IsNullOrWhiteSpace(NewGroupName) ||
            NewGroupCourse is not null ||
            NewGroupTeacher is not null;

        public GroupsManagementViewModel(
            IUnitOfWork unitOfWork,
            ILogger<GroupsManagementViewModel> logger,
            Func<Group, IReadOnlyList<Teacher>, GroupItemViewModel> groupItemFactory)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _groupItemFactory = groupItemFactory;
            _selectedStudentFilter = StudentFilters[0];
            _ = LoadAsync();
        }

        private bool CanSaveCreate() =>
            !string.IsNullOrWhiteSpace(NewGroupName) &&
            NewGroupCourse is not null &&
            NewGroupTeacher is not null;

        partial void OnIsCreatingChanged(bool value) =>
            OnPropertyChanged(nameof(IsCreateFormDirty));

        partial void OnNewGroupNameChanged(string value)
        {
            SaveCreateGroupCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(IsCreateFormDirty));
        }

        partial void OnNewGroupCourseChanged(Course? value)
        {
            SaveCreateGroupCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(IsCreateFormDirty));
        }

        partial void OnNewGroupTeacherChanged(Teacher? value)
        {
            SaveCreateGroupCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(IsCreateFormDirty));
        }

        partial void OnSearchQueryChanged(string value)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            _ = ReloadGroupsAsync(debounce: true, _cts.Token);
        }

        public bool IsStudentFilterActive => SelectedStudentFilter.Value != GroupStudentFilter.All;

        partial void OnSelectedCourseFilterChanged(Course? value) => _ = ReloadGroupsAsync();
        partial void OnSelectedStudentFilterChanged(StudentFilterOption value)
        {
            OnPropertyChanged(nameof(IsStudentFilterActive));
            _ = ReloadGroupsAsync();
        }
        
        private async Task LoadAsync()
        {
            try
            {
                _logger.LogInformation("Loading groups");
                var groups = await _unitOfWork.Groups.GetAllGroupsWithDetailsAsync();
                var courses = await _unitOfWork.Courses.GetAllCoursesAsync();
                _teachers = await _unitOfWork.Teachers.GetAllTeachersAsync();

                OnPropertyChanged(nameof(Teachers));

                Courses.Clear();
                foreach (var c in courses)
                {
                    Courses.Add(c);
                }

                Groups.Clear();
                foreach (var group in groups)
                {
                    var item = _groupItemFactory(group, _teachers);
                    item.StorageProvider = StorageProvider;
                    Groups.Add(item);
                }

                _logger.LogInformation("Loaded {Count} groups", Groups.Count);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to load groups");
                CreateError = "Failed to load data. Check connection.";
            }
        }

        private async Task ReloadGroupsAsync(bool debounce = false, CancellationToken cancellationToken = default)
        {
            try
            {
                if (debounce)
                {
                    await Task.Delay(_searchDebounceMs, cancellationToken);
                }

                var groups = await _unitOfWork.Groups.GetFilteredGroupsAsync(
                    SearchQuery, SelectedCourseFilter?.Id, SelectedStudentFilter.Value);

                Groups.Clear();
                foreach (var group in groups)
                {
                    var item = _groupItemFactory(group, _teachers);
                    item.StorageProvider = StorageProvider;
                    Groups.Add(item);
                }

                NoResults = !Groups.Any();
            }
            catch (OperationCanceledException) { }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to reload groups");
            }
        }

        private string BuildCreateError()
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(NewGroupName)) errors.Add("Name");
            if (NewGroupCourse is null) errors.Add("Course");
            if (NewGroupTeacher is null) errors.Add("Teacher");
            return BuildRequiredError(errors);
        }

        [RelayCommand]
        private void CreateGroup()
        {
            IsCreating = true;
        }

        [RelayCommand]
        private void CancelCreate()
        {
            IsCreating = false;
            NewGroupName = "";
            NewGroupCourse = null;
            NewGroupTeacher = null;
            CreateError = "";
        }

        [RelayCommand(CanExecute = nameof(CanSaveCreate))]
        private async Task SaveCreateGroupAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NewGroupName) || NewGroupCourse is null || NewGroupTeacher is null)
                {
                    CreateError = BuildCreateError();
                    return;
                }

                CreateError = "";
                _logger.LogInformation("Creating group {Name}", NewGroupName);

                var group = new Group
                {
                    Id = Guid.NewGuid(),
                    Name = NewGroupName,
                    CourseId = NewGroupCourse.Id,
                    TeacherId = NewGroupTeacher?.Id
                };

                _unitOfWork.Groups.AddGroup(group);
                await _unitOfWork.SaveAsync();

                group.Course = NewGroupCourse;
                group.Teacher = NewGroupTeacher;

                var newItem = _groupItemFactory(group, _teachers);
                newItem.StorageProvider = StorageProvider;
                Groups.Add(newItem);

                _logger.LogInformation("Group {Name} created successfully", group.Name);
                CancelCreate();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to save group");
                CreateError = "Failed to save. Try again.";
            }
        }

        [RelayCommand]
        private async Task DeleteGroupAsync(GroupItemViewModel item)
        {
            try
            {
                if (item.Group.Students.Count > 0)
                {
                    item.DeleteError = "Cannot delete group with students.";
                    return;
                }

                _logger.LogInformation("Deleting group {Name}", item.Group.Name);

                var group = await _unitOfWork.Groups.GetByIdAsync(item.Group.Id);
                if (group is null)
                {
                    return;
                }

                _unitOfWork.Groups.DeleteGroup(group);
                await _unitOfWork.SaveAsync();
                Groups.Remove(item);
                _logger.LogInformation("Group {Name} deleted", item.Group.Name);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to delete group");
                DeleteError = "Failed to delete. Try again.";
            }
        }

        [RelayCommand]
        private void ClearCourseFilter()
        {
            SelectedCourseFilter = null;
            SelectedStudentFilter = StudentFilters[0];
        }
    }
}
