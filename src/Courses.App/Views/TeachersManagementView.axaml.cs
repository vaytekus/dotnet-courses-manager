using Avalonia.Controls;
using Courses.App.ViewModels;

namespace Courses.App.Views;

public partial class TeachersManagementView : Window
{
    public TeachersManagementView() { }

    public TeachersManagementView(TeachersManagementViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}