using System.Linq;
using Avalonia.Controls;
using Courses.App.ViewModels;

namespace Courses.App.Views;

public partial class StudentsManagementView : Window
{
    private bool _forceClose;

    public StudentsManagementView() { }

    public StudentsManagementView(StudentsManagementViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    protected override async void OnClosing(WindowClosingEventArgs e)
    {
        if (!_forceClose &&
            DataContext is StudentsManagementViewModel vm &&
            (vm.IsCreateFormDirty || vm.Students.Any(s => s.IsDirty)))
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