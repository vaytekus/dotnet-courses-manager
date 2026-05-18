using Avalonia.Controls;
using Courses.App.Data;
using Courses.App.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Courses.App.Views;

public partial class StudentsManagementView : Window
{
    public StudentsManagementView(IDbContextFactory<AppDbContext>? dbContextFactory)
    {
        InitializeComponent();
        if (dbContextFactory is not null)
        {
            DataContext = new StudentsManagementViewModel(dbContextFactory);
        }
    }
}