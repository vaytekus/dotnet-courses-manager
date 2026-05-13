using System;
using Avalonia.Controls;
using Courses.App.Data;
using Courses.App.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Courses.App.Views;

public partial class GroupsManagementView : Window
{
    public GroupsManagementView(IDbContextFactory<AppDbContext>? dbContextFactory)
    {
        InitializeComponent();
        if(dbContextFactory is not null)
            DataContext = new GroupsManagementViewModel(dbContextFactory);
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        if (DataContext is GroupsManagementViewModel viewModel)
        {
            viewModel.StorageProvider = StorageProvider;
        }
    }
}