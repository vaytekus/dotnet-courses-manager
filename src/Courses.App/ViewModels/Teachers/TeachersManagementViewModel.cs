using System;
using System.Collections.ObjectModel;
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
        private readonly ITeacherRepository _teacherRepository;
        private readonly ILogger<TeachersManagementViewModel> _logger;
        private readonly Func<Teacher, TeacherItemViewModel> _teacherItemFactory;

        public ObservableCollection<TeacherItemViewModel> Teachers { get; } = new();

        [ObservableProperty] private bool _isCreating;
        [ObservableProperty] private string _newTeacherFirstName = "";
        [ObservableProperty] private string _newTeacherLastName = "";
        [ObservableProperty] private string _createError = "";

        public TeachersManagementViewModel(
            ITeacherRepository teacherRepository,
            ILogger<TeachersManagementViewModel> logger,
            Func<Teacher, TeacherItemViewModel> teacherItemFactory)
        {
            _teacherRepository = teacherRepository;
            _logger = logger;
            _teacherItemFactory = teacherItemFactory;
            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            try
            {
                _logger.LogInformation("Loading teachers");
                var teachers = await _teacherRepository.GetAllTeachersAsync();

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

        [RelayCommand]
        public void CreateTeacher()
        {
            IsCreating = true;
        }

        [RelayCommand]
        public async Task SaveCreateTeacherAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NewTeacherFirstName) || string.IsNullOrWhiteSpace(NewTeacherLastName))
                {
                    CreateError = "Name is required.";
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

                await _teacherRepository.AddTeacherAsync(teacher);

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
        public void CancelCreateTeacher()
        {
            NewTeacherFirstName = "";
            NewTeacherLastName = "";
            CreateError = "";
            IsCreating = false;
        }
    }
}