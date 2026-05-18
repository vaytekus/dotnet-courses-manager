using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Courses.App.Data;
using Courses.App.Models;
using Microsoft.EntityFrameworkCore;

namespace Courses.App.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IDbContextFactory<AppDbContext>? _dbContextFactory;

    public ObservableCollection<Course> Courses { get; } = new();

    [ObservableProperty] private object? _selectedNode;

    public IReadOnlyList<Student> Students =>
        SelectedNode is Group g ? g.Students.ToList() : new List<Student>();

    partial void OnSelectedNodeChanged(object? value)
    {
        OnPropertyChanged(nameof(Students));
    }

    public MainWindowViewModel(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
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
        try { await LoadAsync(); }
        catch (Exception ex) { Console.Error.WriteLine($"Failed to load courses: {ex}");   } 
    }

    private async Task LoadAsync()
    {
        if (_dbContextFactory is null) return;

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