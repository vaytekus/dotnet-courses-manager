using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Courses.App.Data;
using Courses.App.Models;
using Microsoft.EntityFrameworkCore;

namespace Courses.App.ViewModels
{
    public partial class TeachersManagementViewModel : ViewModelBase
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

        public ObservableCollection<TeacherItemViewModel> Teachers { get; } = new();

        [ObservableProperty] private bool _isCreating;
        [ObservableProperty] private string _newTeacherFirstName = "";
        [ObservableProperty] private string _newTeacherLastName = "";
        [ObservableProperty] private string _createError = "";

        public TeachersManagementViewModel(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            var teachers = await db.Teachers.ToListAsync();
            
            Teachers.Clear();
            foreach (var t in teachers)
            {
                var item = new TeacherItemViewModel(t, _dbContextFactory);
                item.RequestDelete = vm => Teachers.Remove(vm);
                Teachers.Add(item);
            }
        }
        
        [RelayCommand]
        public void CreateTeacher() => IsCreating = true;

        [RelayCommand]
        public async Task SaveCreateTeacher()
        {
            if(string.IsNullOrWhiteSpace(NewTeacherFirstName) || string.IsNullOrWhiteSpace(NewTeacherLastName))
            {
                CreateError = "Name is required.";
                return;
            }
            
            CreateError = "";
            
            await using var db = await _dbContextFactory.CreateDbContextAsync();

            var teacher = new Teacher
            {
                Id = Guid.NewGuid(),
                FirstName = NewTeacherFirstName,
                LastName = NewTeacherLastName
            };
            
            var item = new TeacherItemViewModel(teacher, _dbContextFactory);
            item.RequestDelete = vm => Teachers.Remove(vm);
            
            db.Teachers.Add(teacher);
            await db.SaveChangesAsync();
            
            Teachers.Add(item);
            CancelCreateTeacher();
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