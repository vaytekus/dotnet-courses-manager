using Avalonia.Controls;
using Courses.App.Data;
using Courses.App.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Courses.App.Views;

public partial class TeachersManagementView : Window
{
    public TeachersManagementView(IDbContextFactory<AppDbContext>? dbContextFactory)
    {
        InitializeComponent();
        if (dbContextFactory is not null)
        {
            DataContext = new TeachersManagementViewModel(dbContextFactory);
        }
    }
}