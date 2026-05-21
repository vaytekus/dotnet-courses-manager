using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Courses.App.Interfaces;
using Courses.App.Models;
using Microsoft.Extensions.Logging;

namespace Courses.App.ViewModels
{
    public partial class TeacherItemViewModel : ViewModelBase
    {
        public Teacher Teacher { get; }
        public Action<TeacherItemViewModel>? RequestDelete { get; set; }

        private readonly ITeacherRepository _teacherRepository;
        private readonly ILogger<TeacherItemViewModel> _logger;

        [ObservableProperty] private bool _isEditing;
        [ObservableProperty] private string _editFirstName = "";
        [ObservableProperty] private string _editLastName = "";
        [ObservableProperty] private bool _firstNameError;
        [ObservableProperty] private bool _lastNameError;
        [ObservableProperty] private string _saveError = "";
        [ObservableProperty] private string _deleteError = "";

        public TeacherItemViewModel(
            Teacher teacher,
            ITeacherRepository teacherRepository,
            ILogger<TeacherItemViewModel> logger)
        {
            Teacher = teacher;
            _teacherRepository = teacherRepository;
            _logger = logger;
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
            SaveError = "";
            IsEditing = false;
        }

        [RelayCommand]
        public async Task SaveAsync()
        {
            try
            {
                FirstNameError = string.IsNullOrWhiteSpace(EditFirstName);
                LastNameError = string.IsNullOrWhiteSpace(EditLastName);

                if (FirstNameError || LastNameError)
                {
                    return;
                }

                SaveError = "";
                _logger.LogInformation("Saving teacher {First} {Last}", Teacher.FirstName, Teacher.LastName);

                Teacher.FirstName = EditFirstName;
                Teacher.LastName = EditLastName;
                await _teacherRepository.UpdateTeacherAsync(Teacher);

                _logger.LogInformation("Teacher {First} {Last} saved", Teacher.FirstName, Teacher.LastName);
                IsEditing = false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save teacher");
                SaveError = "Failed to save. Try again.";
            }
        }

        [RelayCommand]
        public async Task DeleteAsync()
        {
            try
            {
                _logger.LogInformation("Deleting teacher {First} {Last}", Teacher.FirstName, Teacher.LastName);
                await _teacherRepository.NullifyGroupTeacherAsync(Teacher.Id);
                await _teacherRepository.DeleteTeacherAsync(Teacher);
                _logger.LogInformation("Teacher {First} {Last} deleted", Teacher.FirstName, Teacher.LastName);
                RequestDelete?.Invoke(this);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete teacher");
                DeleteError = "Failed to delete. Try again.";
            }
        }
    }
}