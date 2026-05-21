using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Courses.App.Interfaces;
using Courses.App.Models;
using Microsoft.Extensions.Logging;

namespace Courses.App.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public ObservableCollection<Course> Courses { get; } = new();
    public IReadOnlyList<Student> Students
    {
        get
        {
            if (SelectedNode is Group g)
            {
                return g.Students.ToList();
            }
            return new List<Student>();
        }
    }

    private readonly ICourseRepository _courseRepository;
    private readonly ILogger<MainWindowViewModel> _logger;

    [ObservableProperty] private object? _selectedNode;
    [ObservableProperty] private string _loadError = "";

    public MainWindowViewModel(ILogger<MainWindowViewModel> logger, ICourseRepository courseRepository)
    {
        _logger = logger;
        _courseRepository = courseRepository;
        _ = SafeLoadAsync();
    }
    
    public MainWindowViewModel()
    {
        _courseRepository = null!;
        _logger = null!;
        Courses.Add(new Course
        {
            Id = Guid.NewGuid(),
            Name = "Sample Course",
            Description = "Preview only",
            Groups =
            {
                new Group
                {
                    Id = Guid.NewGuid(),
                    Name = "Sample Group",
                    Students =
                    {
                        new Student { Id = Guid.NewGuid(), FirstName = "Preview", LastName = "User" },
                    }
                }
            }
        });
    }

    partial void OnSelectedNodeChanged(object? value)
    {
        OnPropertyChanged(nameof(Students));
    }

    private async Task SafeLoadAsync()
    {
        try
        {
            await LoadAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load courses");
            LoadError = "Failed to load data. Check connection.";
        }
    }

    private async Task LoadAsync()
    {
        var data = await _courseRepository.GetAllCoursesWithDetailsAsync();
        
        Courses.Clear();
        foreach (var course in data)
        {
            Courses.Add(course);
        }
    }

    public async Task ReloadAsync()
    {
        await SafeLoadAsync();
    }
}