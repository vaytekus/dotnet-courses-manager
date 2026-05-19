using System;
using Avalonia.Controls;
using Courses.App.ViewModels;

namespace Courses.App.Views;

public partial class GroupsManagementView : Window
{
    public GroupsManagementView() { }

    public GroupsManagementView(GroupsManagementViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
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