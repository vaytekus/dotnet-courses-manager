using System;
using System.Linq;
using Avalonia.Controls;
using Courses.App.ViewModels;

namespace Courses.App.Views;

public partial class TeachersManagementView : Window
{
    private bool _forceClose;
    
    public TeachersManagementView() { }

    public TeachersManagementView(TeachersManagementViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    protected override async void OnClosing(WindowClosingEventArgs e)
    {
        if (!_forceClose &&
            DataContext is TeachersManagementViewModel viewModel && 
            (viewModel.IsCreateFormDirty || viewModel.Teachers.Any(t => t.IsDirty)))
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