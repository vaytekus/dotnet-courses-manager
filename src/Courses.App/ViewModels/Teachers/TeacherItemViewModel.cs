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
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TeacherItemViewModel> _logger;

        [ObservableProperty] private bool _isEditing;
        [ObservableProperty] private string _editFirstName = "";
        [ObservableProperty] private string _editLastName = "";
        [ObservableProperty] private bool _firstNameError;
        [ObservableProperty] private bool _lastNameError;
        [ObservableProperty] private string _saveError = "";
        [ObservableProperty] private string _deleteError = "";

        public Teacher Teacher { get; }
        public Action<TeacherItemViewModel>? RequestDelete { get; set; }

        public bool IsDirty => IsEditing && (
            EditFirstName != Teacher.FirstName ||
            EditLastName != Teacher.LastName);

        public TeacherItemViewModel(
            Teacher teacher,
            IUnitOfWork unitOfWork,
            ILogger<TeacherItemViewModel> logger)
        {
            Teacher = teacher;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _editFirstName = Teacher.FirstName;
            _editLastName = Teacher.LastName;
        }

        private bool CanSave() =>
            !string.IsNullOrWhiteSpace(EditFirstName) &&
            !string.IsNullOrWhiteSpace(EditLastName);

        partial void OnIsEditingChanged(bool value) =>
            OnPropertyChanged(nameof(IsDirty));
        
        partial void OnEditFirstNameChanged(string value)
        {
            SaveCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(IsDirty));
        }

        partial void OnEditLastNameChanged(string value)
        {
            SaveCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(IsDirty));
        }

        [RelayCommand]
        private void Edit()
        {
            EditFirstName = Teacher.FirstName;
            EditLastName = Teacher.LastName;
            IsEditing = true;
        }

        [RelayCommand]
        private void Cancel()
        {
            EditFirstName = Teacher.FirstName;
            EditLastName = Teacher.LastName;
            FirstNameError = false;
            LastNameError = false;
            SaveError = "";
            IsEditing = false;
        }

        [RelayCommand(CanExecute = nameof(CanSave))]
        private async Task SaveAsync()
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
                _unitOfWork.Teachers.UpdateTeacher(Teacher);
                await _unitOfWork.SaveAsync();
                
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
        private async Task DeleteAsync()
        {
            try
            {
                _logger.LogInformation("Deleting teacher {First} {Last}", Teacher.FirstName, Teacher.LastName);
                await _unitOfWork.Teachers.NullifyGroupTeacherAsync(Teacher.Id);
                _unitOfWork.Teachers.DeleteTeacher(Teacher);
                await _unitOfWork.SaveAsync();
                    
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