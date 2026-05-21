using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Courses.App.Interfaces;
using Courses.App.Models;
using Microsoft.Extensions.Logging;

namespace Courses.App.ViewModels
{
    public partial class GroupsManagementViewModel : ViewModelBase
    {
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
        
        private readonly IGroupRepository _groupRepository;
        private readonly ITeacherRepository _teacherRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ILogger<GroupsManagementViewModel> _logger;
        private readonly Func<Group, IReadOnlyList<Teacher>, GroupItemViewModel> _groupItemFactory;
        private IStorageProvider? _storageProvider;
        private List<Teacher> _teachers = new();
        
        [ObservableProperty] private bool _isCreating;
        [ObservableProperty] private string _newGroupName = "";
        [ObservableProperty] private Course? _newGroupCourse;
        [ObservableProperty] private Teacher? _newGroupTeacher;
        [ObservableProperty] private string _createError = "";

        public GroupsManagementViewModel(
            IGroupRepository groupRepository,
            ITeacherRepository teacherRepository,
            ICourseRepository courseRepository,
            ILogger<GroupsManagementViewModel> logger,
            Func<Group, IReadOnlyList<Teacher>, GroupItemViewModel> groupItemFactory)
        {
            _groupRepository = groupRepository;
            _teacherRepository = teacherRepository;
            _courseRepository = courseRepository;
            _logger = logger;
            _groupItemFactory = groupItemFactory;
            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            try
            {
                _logger.LogInformation("Loading groups");
                var groups = await _groupRepository.GetAllGroupsWidthDetailsAsync();
                var courses = await _courseRepository.GetAllCoursesAsync();
                _teachers = await _teacherRepository.GetAllTeachersAsync();
                
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

        [RelayCommand]
        private async Task SaveCreateGroupAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NewGroupName) || NewGroupCourse is null || NewGroupTeacher is null)
                {
                    CreateError = "All fields are required.";
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

                await _groupRepository.AddGroupAsync(group);

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

                var group = await _groupRepository.GetByIdAsync(item.Group.Id);
                if (group is null)
                {
                    return;
                }

                await _groupRepository.DeleteGroupAsync(group);
                Groups.Remove(item);
                _logger.LogInformation("Group {Name} deleted", item.Group.Name);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to delete group");                                                                                                                                                                                          
                CreateError = "Failed to delete. Try again.";
            }
        }
    }
}