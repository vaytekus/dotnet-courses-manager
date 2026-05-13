using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Courses.App.Data;
using Microsoft.EntityFrameworkCore;

namespace Courses.App.Views;

public partial class MainWindowView : Window
{
    private readonly IDbContextFactory<AppDbContext>? _dbContextFactory;
    
    public MainWindowView(IDbContextFactory<AppDbContext>? dbContextFactory)
    {
        InitializeComponent();
        _dbContextFactory = dbContextFactory;
    }

    private async void OnManageGroupsClick(object sender, RoutedEventArgs e)
    {
        var dialog = new GroupsManagementView(_dbContextFactory);
        dialog.Opened += (_, _) => dialog.Activate();
        await dialog.ShowDialog(this);
    }
}