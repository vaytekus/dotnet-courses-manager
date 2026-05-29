using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Courses.App.Interfaces;
using Courses.App.Models;
using Microsoft.Extensions.Logging;

namespace Courses.App.ViewModels
{
    public partial class StudentItemViewModel : ViewModelBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<StudentItemViewModel> _logger;

        [ObservableProperty] private Group? _selectedGroup;
        [ObservableProperty] private bool _isEditing;
        [ObservableProperty] private string _editFirstName = "";
        [ObservableProperty] private string _editLastName = "";
        [ObservableProperty] private bool _firstNameError;
        [ObservableProperty] private bool _lastNameError;
        [ObservableProperty] private string _saveError = "";
        [ObservableProperty] private string _deleteError = "";

        public Student Student { get; }
        public IReadOnlyList<Group> Groups { get; }
        public Action<StudentItemViewModel>? RequestDelete { get; set; }
        public bool IsDirty => IsEditing && (
            EditFirstName != Student.FirstName ||
            EditLastName != Student.LastName ||
            SelectedGroup?.Id != Student.GroupId
        );

        public StudentItemViewModel(
            Student student,
            IReadOnlyList<Group> groups,
            IUnitOfWork unitOfWork,
            ILogger<StudentItemViewModel> logger)
        {
            Student = student;
            Groups = groups;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _editFirstName = student.FirstName;
            _editLastName = student.LastName;
            _selectedGroup = student.Group;
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

        partial void OnSelectedGroupChanged(Group? value) =>
            OnPropertyChanged(nameof(IsDirty));

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
                _logger.LogInformation("Saving student {First} {Last}", Student.FirstName, Student.LastName);

                Student.FirstName = EditFirstName;
                Student.LastName = EditLastName;
                Student.GroupId = SelectedGroup?.Id ?? Student.GroupId;
                Student.Group = SelectedGroup ?? Student.Group;
                _unitOfWork.Students.UpdateStudent(Student);
                await _unitOfWork.SaveAsync();
                _logger.LogInformation("Student {First} {Last} saved", Student.FirstName, Student.LastName);
                IsEditing = false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save student");
                SaveError = "Failed to save. Try again.";
            }
        }

        [RelayCommand]
        private async Task DeleteAsync()
        {
            try
            {
                _logger.LogInformation("Deleting student {First} {Last}", Student.FirstName, Student.LastName);
                var student = await _unitOfWork.Students.GetStudentByIdAsync(Student.Id);
                if (student is null)
                {
                    return;
                }

                _unitOfWork.Students.RemoveStudent(student);
                await _unitOfWork.SaveAsync();

                _logger.LogInformation("Student {First} {Last} deleted", Student.FirstName, Student.LastName);
                RequestDelete?.Invoke(this);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete student");
                DeleteError = "Failed to delete. Try again.";
            }
        }
    }
}