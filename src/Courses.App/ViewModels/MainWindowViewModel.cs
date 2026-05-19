using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Courses.App.Data;
using Courses.App.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Courses.App.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IDbContextFactory<AppDbContext>? _dbContextFactory;
    
    private readonly ILogger<MainWindowViewModel>? _logger;

    public ObservableCollection<Course> Courses { get; } = new();

    [ObservableProperty] private object? _selectedNode;

    public IReadOnlyList<Student> Students =>
        SelectedNode is Group g ? g.Students.ToList() : new List<Student>();

    partial void OnSelectedNodeChanged(object? value)
    {
        OnPropertyChanged(nameof(Students));
    }

    public MainWindowViewModel(IDbContextFactory<AppDbContext> dbContextFactory, ILoggerFactory loggerFactory)
    {
        _dbContextFactory = dbContextFactory;
        _logger = loggerFactory.CreateLogger<MainWindowViewModel>();
        _ = SafeLoadAsync();
    }
    
    public MainWindowViewModel()
    {
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

    private async Task SafeLoadAsync()
    {
        try
        {
            await LoadAsync();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to load courses");
        } 
    }

    private async Task LoadAsync()
    {
        if (_dbContextFactory is null)
        {
            throw new InvalidOperationException("DbContextFactory is null"); 
        };

        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var data = await db.Courses
            .Include(c => c.Groups)
            .ThenInclude(g => g.Students)
            .ToListAsync();
        
        Courses.Clear();
        foreach (var course in data) Courses.Add(course);
    }

    public async Task ReloadSync() => await SafeLoadAsync();
}