using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
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

    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MainWindowViewModel> _logger;
    private CancellationTokenSource? _cts;

    [ObservableProperty] private object? _selectedNode;
    [ObservableProperty] private string _loadError = "";
    [ObservableProperty] private string _searchQuery = "";
    [ObservableProperty] private bool _noResults;

    public MainWindowViewModel(ILogger<MainWindowViewModel> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _ = SafeLoadAsync();
    }
    
    public MainWindowViewModel()
    {
        _unitOfWork = null!;
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

    partial void OnSearchQueryChanged(string value)
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        _ = ReloadCoursesAsync(debounce: true, _cts.Token);
    }

    private async Task ReloadCoursesAsync(bool debounce = false, CancellationToken token = default)
    {
        try
        {
            if (debounce)
            {
                await Task.Delay(_searchDebounceMs, token);
            }

            var courses = string.IsNullOrWhiteSpace(SearchQuery)
                ? await _unitOfWork.Courses.GetAllCoursesWithDetailsAsync()
                : await _unitOfWork.Courses.SearchCoursesAsync(SearchQuery);
            
            Courses.Clear();
            foreach (var course in courses)
            {
                Courses.Add(course);
            }
            
            NoResults = !Courses.Any();
        }
        catch (OperationCanceledException) { }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to reload courses");
        }
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
        var data = await _unitOfWork.Courses.GetAllCoursesWithDetailsAsync();
        
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