using Avalonia.Controls;
using Courses.App.ViewModels;

namespace Courses.App.Views;

public partial class StudentsManagementView : Window
{
    public StudentsManagementView() { }

    public StudentsManagementView(StudentsManagementViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}