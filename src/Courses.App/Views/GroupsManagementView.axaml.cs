using System;
using System.Linq;
using Avalonia.Controls;
using Courses.App.ViewModels;

namespace Courses.App.Views;

public partial class GroupsManagementView : Window
{
    private bool _forceClose;
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

    protected override async void OnClosing(WindowClosingEventArgs e)
    {
        if (!_forceClose &&
            DataContext is GroupsManagementViewModel viewModel &&
            (viewModel.IsCreateFormDirty || viewModel.Groups.Any(g => g.IsDirty)))
        {
            e.Cancel = true;
            if (await ConfirmDialog.ShowAsync(this, "You have unsaved changes. Close anyway?"))
            {
                _forceClose = true;
                Close();
            }
        }

        base.OnClosing(e);
    }
}